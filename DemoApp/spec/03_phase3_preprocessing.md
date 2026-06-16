# Phase 3 — Image Preprocessing (OpenCV)

## Goal
Build a composable OpenCV preprocessing pipeline in `utils/image_utils.py`. Each step is a standalone function so individual steps can be tested, skipped, or tuned independently. The pipeline normalizes raw images to maximize Tesseract accuracy.

---

## Why Preprocessing Matters
Raw photos often suffer from:
- **Noise** — digital noise from cameras or scanners
- **Uneven lighting** — shadows, glare, low contrast
- **Tilt/skew** — slightly rotated documents or handheld photos
- **Color** — Tesseract performs better on grayscale/binary images

---

## Preprocessing Pipeline (in order)

### Step 1 — Decode Bytes to OpenCV Array
```python
import cv2
import numpy as np

def decode_image(image_bytes: bytes) -> np.ndarray:
    """Convert raw bytes to an OpenCV BGR image array."""
    np_arr = np.frombuffer(image_bytes, np.uint8)
    img = cv2.imdecode(np_arr, cv2.IMREAD_COLOR)
    if img is None:
        raise ValueError("Could not decode image. File may be corrupt or unsupported.")
    return img
```

### Step 2 — Convert to Grayscale
```python
def to_grayscale(img: np.ndarray) -> np.ndarray:
    """Convert BGR image to grayscale."""
    return cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
```

### Step 3 — Denoise (Gaussian Blur)
```python
def denoise(img: np.ndarray, kernel_size: int = 3) -> np.ndarray:
    """Apply Gaussian blur to reduce noise.
    
    kernel_size: must be odd (3, 5, 7). Larger = more smoothing.
    Use 3 for most cases; increase for very noisy images.
    """
    return cv2.GaussianBlur(img, (kernel_size, kernel_size), 0)
```

### Step 4 — Adaptive Thresholding (Binarization)
```python
def binarize(img: np.ndarray) -> np.ndarray:
    """Convert grayscale to black-and-white using adaptive thresholding.
    
    Adaptive thresholding handles uneven lighting across the image
    (shadows, glare) better than a single global threshold.
    """
    return cv2.adaptiveThreshold(
        img,
        maxValue=255,
        adaptiveMethod=cv2.ADAPTIVE_THRESH_GAUSSIAN_C,
        thresholdType=cv2.THRESH_BINARY,
        blockSize=11,   # size of pixel neighborhood (must be odd)
        C=2             # constant subtracted from mean
    )
```

### Step 5 — Deskew
```python
def deskew(img: np.ndarray) -> np.ndarray:
    """Detect and correct image tilt using the minimum bounding rectangle.
    
    Only corrects if tilt angle is between 0.5° and 45° to avoid
    over-rotating images that are legitimately angled.
    """
    coords = np.column_stack(np.where(img > 0))
    if len(coords) == 0:
        return img
    angle = cv2.minAreaRect(coords)[-1]
    if angle < -45:
        angle = 90 + angle
    if abs(angle) < 0.5:
        return img   # skip — negligible tilt
    (h, w) = img.shape[:2]
    center = (w // 2, h // 2)
    M = cv2.getRotationMatrix2D(center, angle, 1.0)
    return cv2.warpAffine(
        img, M, (w, h),
        flags=cv2.INTER_CUBIC,
        borderMode=cv2.BORDER_REPLICATE
    )
```

---

## Full Pipeline Function

```python
def preprocess(image_bytes: bytes) -> tuple[np.ndarray, tuple[int, int]]:
    """Run the full preprocessing pipeline.
    
    Returns:
        processed_img: binarized, deskewed image ready for OCR
        original_size: (width, height) of the original image
    """
    img = decode_image(image_bytes)
    original_size = (img.shape[1], img.shape[0])  # (width, height)

    img = to_grayscale(img)
    img = denoise(img)
    img = binarize(img)
    img = deskew(img)

    return img, original_size
```

---

## File Location
```
utils/image_utils.py
```

All functions above live in this single file. Import them in `services/ocr_service.py` (Phase 4).

---

## Tuning Parameters

| Parameter | Default | When to Adjust |
|---|---|---|
| Gaussian kernel size | `3` | Increase to `5` or `7` for very grainy images |
| Adaptive block size | `11` | Increase for images with large lighting variance |
| Adaptive constant C | `2` | Increase for dark images, decrease for light ones |
| Deskew min angle | `0.5°` | Decrease if slight tilts are not being corrected |

---

## Verification Checklist
- [ ] `decode_image()` raises `ValueError` for corrupt/non-image bytes
- [ ] `to_grayscale()` returns single-channel (H, W) array
- [ ] `binarize()` returns binary image with only 0 and 255 values
- [ ] `deskew()` returns unchanged image when angle < 0.5°
- [ ] `preprocess()` returns `(ndarray, (width, height))` tuple
