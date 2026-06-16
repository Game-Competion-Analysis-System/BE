import os
import logging
from datetime import datetime
from pathlib import Path

import cv2
import numpy as np

logger = logging.getLogger(__name__)

_DEBUG = os.getenv("DEBUG_SAVE_IMAGES", "false").lower() == "true"
_DEBUG_DIR = Path(os.getenv("DEBUG_OUTPUT_DIR", "debug_output"))


def _save_debug(img: np.ndarray, stem: str, step: str) -> None:
    """Write an intermediate pipeline image to the debug directory.

    No-op unless DEBUG_SAVE_IMAGES=true in the environment.
    Files are written as: debug_output/{timestamp}_{stem}_{step}.png
    """
    if not _DEBUG:
        return
    _DEBUG_DIR.mkdir(parents=True, exist_ok=True)
    ts = datetime.now().strftime("%Y%m%d_%H%M%S_%f")
    path = _DEBUG_DIR / f"{ts}_{stem}_{step}.png"
    cv2.imwrite(str(path), img)
    logger.debug("Saved debug image: %s", path)


def decode_image(image_bytes: bytes) -> np.ndarray:
    """Convert raw bytes to an OpenCV BGR image array."""
    np_arr = np.frombuffer(image_bytes, np.uint8)
    img = cv2.imdecode(np_arr, cv2.IMREAD_COLOR)
    if img is None:
        raise ValueError("Could not decode image. File may be corrupt or unsupported.")
    return img


def preprocess(image_bytes: bytes, filename: str = "image") -> tuple[np.ndarray, tuple[int, int]]:
    """Decode image bytes and return the colour image ready for EasyOCR.

    EasyOCR operates natively on RGB/BGR images — no grayscale conversion,
    binarization, or heuristic inversion is required. This means images with
    any text/background colour combination (e.g. white text on blue background)
    are handled correctly by the model without preprocessing.

    Returns:
        img: decoded BGR image as a NumPy array (H x W x 3)
        original_size: (width, height) of the image
    """
    stem = Path(filename).stem[:40]
    img = decode_image(image_bytes)
    original_size = (img.shape[1], img.shape[0])
    _save_debug(img, stem, "1_original")
    return img, original_size
