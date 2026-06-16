import time
import logging

import cv2
import easyocr

from utils.image_utils import preprocess
from models.schemas import OCRResponse, TextBlock, BoundingBox, ImageSize

logger = logging.getLogger(__name__)

# Minimum confidence threshold — detections below this are discarded.
# EasyOCR returns confidence in [0, 1]; the threshold is kept in [0, 100]
# to match the existing OCRResponse schema and test expectations.
CONFIDENCE_THRESHOLD = 30.0

# EasyOCR's Japanese model uses a separate character set and cannot share a
# reader instance with Latin-script models (Vietnamese, English).
# We maintain two readers and select the appropriate one per request.
try:
    _reader_latin = easyocr.Reader(["en", "vi"], gpu=False)
    _reader_japanese = easyocr.Reader(["ja", "en"], gpu=False)
except Exception as exc:
    logger.critical("EasyOCR failed to initialize: %s", exc)
    raise RuntimeError("OCR engine unavailable") from exc

# Maps Tesseract-style API language codes to (EasyOCR code, reader) pairs.
_LANG_CONFIG: dict[str, tuple[str, easyocr.Reader]] = {
    "eng": ("en", _reader_latin),
    "vie": ("vi", _reader_latin),
    "jpn": ("ja", _reader_japanese),
}


def process(image_bytes: bytes, filename: str, language: str = "eng") -> OCRResponse:
    """Run EasyOCR on raw image bytes and return a structured OCRResponse.

    Args:
        image_bytes: raw bytes of the uploaded image
        filename: original filename (included in the response)
        language: Tesseract-style language code ('eng', 'vie', or 'jpn')

    Returns:
        OCRResponse matching the existing API contract
    """
    start_time = time.perf_counter()

    # Step 1: Decode image — validate bytes and capture original dimensions.
    try:
        bgr_img, (orig_width, orig_height) = preprocess(image_bytes, filename)
    except ValueError as e:
        logger.error("Image decode failed: %s", e)
        raise

    # Step 2: Convert BGR → RGB (EasyOCR expects RGB channel order).
    rgb_img = cv2.cvtColor(bgr_img, cv2.COLOR_BGR2RGB)

    # Step 3: Select the appropriate reader for the requested language.
    _, reader = _LANG_CONFIG.get(language, ("en", _reader_latin))

    # Step 4: Run EasyOCR.
    # detail=1  → returns [[bbox, text, confidence], ...]
    # paragraph=False → word/phrase-level detections (finer bounding boxes)
    logger.info("Running EasyOCR on '%s' (language=%s)", filename, language)
    results = reader.readtext(rgb_img, detail=1, paragraph=False)

    # Step 5: Build text blocks, filtering by confidence.
    text_blocks: list[TextBlock] = []
    for bbox, text, conf in results:
        text = text.strip()
        conf_scaled = round(conf * 100, 2)   # normalise [0,1] → [0,100]
        if not text or conf_scaled < CONFIDENCE_THRESHOLD:
            continue

        # bbox is [[x0,y0],[x1,y1],[x2,y2],[x3,y3]] (polygon corners).
        xs = [pt[0] for pt in bbox]
        ys = [pt[1] for pt in bbox]
        text_blocks.append(
            TextBlock(
                text=text,
                confidence=conf_scaled,
                bounding_box=BoundingBox(
                    x=int(min(xs)),
                    y=int(min(ys)),
                    width=int(max(xs) - min(xs)),
                    height=int(max(ys) - min(ys)),
                ),
            )
        )

    # Step 6: Reconstruct full text preserving approximate line structure.
    full_text = _build_full_text(results)

    elapsed_ms = round((time.perf_counter() - start_time) * 1000, 2)
    logger.info("OCR complete: %d blocks in %.1f ms", len(text_blocks), elapsed_ms)

    return OCRResponse(
        success=True,
        filename=filename,
        language=language,
        image_size=ImageSize(width=orig_width, height=orig_height),
        full_text=full_text,
        text_blocks=text_blocks,
        processing_time_ms=elapsed_ms,
    )


def _build_full_text(results: list, threshold_fraction: float = 0.6) -> str:
    """Group EasyOCR detections into lines and assemble a plain-text string.

    Detections are already ordered top-to-bottom by EasyOCR's CRAFT detector.
    We cluster them into lines using each detection's y_center: if the next
    detection's y_center falls within `threshold_fraction * median_height` of
    the current line's representative y, it is placed on the same line.
    Each line is then sorted left-to-right before joining.
    """
    if not results:
        return ""

    entries = []
    for bbox, text, conf in results:
        text = text.strip()
        if not text:
            continue
        ys = [pt[1] for pt in bbox]
        xs = [pt[0] for pt in bbox]
        y_center = (min(ys) + max(ys)) / 2
        height = max(ys) - min(ys)
        x_left = min(xs)
        entries.append((y_center, x_left, height, text))

    if not entries:
        return ""

    median_height = sorted(e[2] for e in entries)[len(entries) // 2]
    tolerance = threshold_fraction * median_height

    lines: list[list[tuple]] = []
    current_line: list[tuple] = [entries[0]]
    current_y = entries[0][0]

    for entry in entries[1:]:
        y_center = entry[0]
        if abs(y_center - current_y) <= tolerance:
            current_line.append(entry)
        else:
            lines.append(sorted(current_line, key=lambda e: e[1]))
            current_line = [entry]
            current_y = y_center

    lines.append(sorted(current_line, key=lambda e: e[1]))

    return "\n".join(" ".join(e[3] for e in line) for line in lines).strip()
