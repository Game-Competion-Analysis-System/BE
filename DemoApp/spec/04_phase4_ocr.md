# Phase 4 — OCR Extraction (Tesseract)

## Goal
Integrate Tesseract OCR into `services/ocr_service.py`. This service calls the preprocessing pipeline from Phase 3, runs Tesseract, and returns structured data (text blocks with bounding boxes and confidence scores) ready for the response layer in Phase 5.

---

## Tesseract Installation

### Windows
1. Download installer from: https://github.com/UB-Mannheim/tesseract/wiki
2. Install to `C:\Program Files\Tesseract-OCR\`
3. Add to PATH, or set in `.env`:
   ```
   TESSERACT_CMD=C:/Program Files/Tesseract-OCR/tesseract.exe
   ```

### macOS
```bash
brew install tesseract
```

### Linux (Ubuntu/Debian)
```bash
sudo apt-get install tesseract-ocr
```

Verify installation:
```bash
tesseract --version
```

---

## How pytesseract Works

`pytesseract.image_to_data()` returns a dictionary with per-word data:

| Key | Description |
|---|---|
| `text` | Recognized word string |
| `conf` | Confidence score (0–100); `-1` means non-text segment |
| `left` | X coordinate of bounding box |
| `top` | Y coordinate of bounding box |
| `width` | Width of bounding box |
| `height` | Height of bounding box |
| `level` | Hierarchy level (page/block/line/word) |

We filter entries where `conf == -1` (structural segments with no text) and where `text.strip()` is empty.

---

## OCR Service

### `services/ocr_service.py`
```python
import os
import time
import logging
import pytesseract
from pytesseract import Output

from utils.image_utils import preprocess
from models.schemas import OCRResponse, TextBlock, BoundingBox, ImageSize

logger = logging.getLogger(__name__)

# Set Tesseract binary path from environment (needed on Windows)
tesseract_cmd = os.getenv("TESSERACT_CMD")
if tesseract_cmd:
    pytesseract.pytesseract.tesseract_cmd = tesseract_cmd

# Minimum confidence threshold — words below this are discarded
CONFIDENCE_THRESHOLD = 30.0


def process(image_bytes: bytes, filename: str) -> OCRResponse:
    """Run the full OCR pipeline on raw image bytes.

    Args:
        image_bytes: raw bytes of the uploaded image
        filename: original filename (used in response only)

    Returns:
        OCRResponse with text blocks, full text, and image metadata
    """
    start_time = time.perf_counter()

    # Step 1: Preprocess image
    try:
        processed_img, (orig_width, orig_height) = preprocess(image_bytes)
    except ValueError as e:
        logger.error("Preprocessing failed: %s", e)
        raise

    # Step 2: Run Tesseract
    logger.info("Running Tesseract OCR on '%s'", filename)
    data = pytesseract.image_to_data(
        processed_img,
        output_type=Output.DICT,
        config="--psm 3",   # PSM 3 = fully automatic page segmentation
    )

    # Step 3: Build text blocks
    text_blocks: list[TextBlock] = []
    num_words = len(data["text"])

    for i in range(num_words):
        conf = float(data["conf"][i])
        word = data["text"][i].strip()

        # Skip non-text segments and low-confidence words
        if conf == -1 or conf < CONFIDENCE_THRESHOLD or not word:
            continue

        text_blocks.append(
            TextBlock(
                text=word,
                confidence=round(conf, 2),
                bounding_box=BoundingBox(
                    x=data["left"][i],
                    y=data["top"][i],
                    width=data["width"][i],
                    height=data["height"][i],
                ),
            )
        )

    # Step 4: Assemble full text (preserve line structure)
    full_text = pytesseract.image_to_string(processed_img, config="--psm 3").strip()

    elapsed_ms = round((time.perf_counter() - start_time) * 1000, 2)
    logger.info("OCR complete: %d words in %.1f ms", len(text_blocks), elapsed_ms)

    return OCRResponse(
        success=True,
        filename=filename,
        image_size=ImageSize(width=orig_width, height=orig_height),
        full_text=full_text,
        text_blocks=text_blocks,
        processing_time_ms=elapsed_ms,
    )
```

---

## Tesseract Page Segmentation Modes (PSM)

| PSM | Mode | Use When |
|---|---|---|
| 3 | Fully automatic (default) | General photos, mixed content |
| 6 | Single uniform block of text | Clean document pages |
| 11 | Sparse text — find as much as possible | Receipts, sparse layouts |
| 13 | Raw line | Single-line images |

Change via `config="--psm <n>"` in `image_to_data()` and `image_to_string()`.

---

## Wire Up in Router (`routers/ocr.py`)

Replace the Phase 2 stub with the real service call:
```python
from services import ocr_service
from utils.image_utils import preprocess
from fastapi import HTTPException

# Inside the endpoint, after reading contents:
try:
    result = ocr_service.process(contents, image.filename or "unknown")
except ValueError as e:
    raise HTTPException(status_code=422, detail=str(e))

return result
```

---

## Verification Checklist
- [ ] `tesseract --version` works in terminal
- [ ] `ocr_service.process()` returns `OCRResponse` for a clear photo with text
- [ ] `text_blocks` contains words with `confidence > 30`
- [ ] `full_text` contains the expected readable text
- [ ] `processing_time_ms` is a positive number
- [ ] Uploading a blank white image returns empty `text_blocks` and `full_text`
- [ ] Corrupt image bytes raise `ValueError` (returns HTTP 422)
