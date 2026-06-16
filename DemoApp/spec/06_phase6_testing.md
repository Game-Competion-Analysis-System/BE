# Phase 6 — Testing & Documentation

## Goal
Write a complete pytest test suite covering the health endpoint, OCR API integration, and individual preprocessing functions. All tests must pass before the project is considered production-ready.

---

## Test Setup

### Install Test Dependencies
Already included in `requirements.txt`:
```
pytest==8.2.0
httpx==0.27.0
```

### Run All Tests
```bash
pytest tests/ -v
```

### Run a Single Test File
```bash
pytest tests/test_ocr.py -v
```

---

## Test File Structure

```
tests/
├── conftest.py              # Shared fixtures (test client, sample images)
├── test_health.py           # Health endpoint tests
├── test_ocr.py              # API integration tests for /api/v1/extract
└── test_preprocessing.py   # Unit tests for utils/image_utils.py
```

---

## `tests/conftest.py` — Shared Fixtures

```python
import io
import pytest
import numpy as np
import cv2
from httpx import AsyncClient, ASGITransport
from main import app


@pytest.fixture
def anyio_backend():
    return "asyncio"


@pytest.fixture
async def client():
    async with AsyncClient(
        transport=ASGITransport(app=app), base_url="http://test"
    ) as ac:
        yield ac


@pytest.fixture
def sample_image_bytes() -> bytes:
    """Generate a simple white image with black text using OpenCV."""
    img = np.ones((100, 400, 3), dtype=np.uint8) * 255  # white background
    cv2.putText(img, "Hello World", (10, 60),
                cv2.FONT_HERSHEY_SIMPLEX, 1.5, (0, 0, 0), 2)
    _, buffer = cv2.imencode(".jpg", img)
    return buffer.tobytes()


@pytest.fixture
def blank_image_bytes() -> bytes:
    """All-white image with no text."""
    img = np.ones((100, 400, 3), dtype=np.uint8) * 255
    _, buffer = cv2.imencode(".jpg", img)
    return buffer.tobytes()


@pytest.fixture
def corrupt_bytes() -> bytes:
    return b"this is not an image"
```

---

## `tests/test_health.py`

```python
import pytest

@pytest.mark.anyio
async def test_health_returns_ok(client):
    response = await client.get("/health")
    assert response.status_code == 200
    assert response.json()["status"] == "ok"

@pytest.mark.anyio
async def test_health_returns_version(client):
    response = await client.get("/health")
    assert "version" in response.json()
```

---

## `tests/test_ocr.py`

```python
import pytest

@pytest.mark.anyio
async def test_extract_valid_image_returns_200(client, sample_image_bytes):
    response = await client.post(
        "/api/v1/extract",
        files={"image": ("test.jpg", sample_image_bytes, "image/jpeg")},
    )
    assert response.status_code == 200
    body = response.json()
    assert body["success"] is True
    assert body["filename"] == "test.jpg"
    assert isinstance(body["text_blocks"], list)
    assert isinstance(body["full_text"], str)
    assert body["processing_time_ms"] > 0

@pytest.mark.anyio
async def test_extract_blank_image_returns_empty_blocks(client, blank_image_bytes):
    response = await client.post(
        "/api/v1/extract",
        files={"image": ("blank.jpg", blank_image_bytes, "image/jpeg")},
    )
    assert response.status_code == 200
    body = response.json()
    assert body["text_blocks"] == []
    assert body["full_text"] == ""

@pytest.mark.anyio
async def test_extract_unsupported_format_returns_400(client):
    response = await client.post(
        "/api/v1/extract",
        files={"image": ("doc.pdf", b"fake pdf content", "application/pdf")},
    )
    assert response.status_code == 400
    assert "Unsupported file type" in response.json()["detail"]

@pytest.mark.anyio
async def test_extract_corrupt_image_returns_422(client, corrupt_bytes):
    response = await client.post(
        "/api/v1/extract",
        files={"image": ("bad.jpg", corrupt_bytes, "image/jpeg")},
    )
    assert response.status_code == 422

@pytest.mark.anyio
async def test_extract_missing_file_returns_422(client):
    response = await client.post("/api/v1/extract")
    assert response.status_code == 422

@pytest.mark.anyio
async def test_response_has_image_size(client, sample_image_bytes):
    response = await client.post(
        "/api/v1/extract",
        files={"image": ("test.jpg", sample_image_bytes, "image/jpeg")},
    )
    body = response.json()
    assert body["image_size"]["width"] > 0
    assert body["image_size"]["height"] > 0

@pytest.mark.anyio
async def test_text_blocks_have_required_fields(client, sample_image_bytes):
    response = await client.post(
        "/api/v1/extract",
        files={"image": ("test.jpg", sample_image_bytes, "image/jpeg")},
    )
    for block in response.json()["text_blocks"]:
        assert "text" in block
        assert "confidence" in block
        assert "bounding_box" in block
        bb = block["bounding_box"]
        assert all(k in bb for k in ("x", "y", "width", "height"))
```

---

## `tests/test_preprocessing.py`

```python
import numpy as np
import pytest
from utils.image_utils import (
    decode_image, to_grayscale, denoise, binarize, deskew, preprocess
)


@pytest.fixture
def sample_bgr_image():
    img = np.ones((100, 200, 3), dtype=np.uint8) * 200
    return img


def test_decode_image_valid(sample_image_bytes):
    img = decode_image(sample_image_bytes)
    assert img is not None
    assert len(img.shape) == 3  # H x W x C

def test_decode_image_invalid_raises():
    with pytest.raises(ValueError):
        decode_image(b"not an image")

def test_to_grayscale_returns_2d(sample_bgr_image):
    gray = to_grayscale(sample_bgr_image)
    assert len(gray.shape) == 2  # H x W only

def test_denoise_returns_same_shape(sample_bgr_image):
    gray = to_grayscale(sample_bgr_image)
    denoised = denoise(gray)
    assert denoised.shape == gray.shape

def test_binarize_returns_binary_values(sample_bgr_image):
    gray = to_grayscale(sample_bgr_image)
    binary = binarize(gray)
    unique_values = np.unique(binary)
    assert set(unique_values).issubset({0, 255})

def test_deskew_no_change_on_straight_image(sample_bgr_image):
    gray = to_grayscale(sample_bgr_image)
    binary = binarize(gray)
    result = deskew(binary)
    assert result.shape == binary.shape

def test_preprocess_returns_tuple(sample_image_bytes):
    img, size = preprocess(sample_image_bytes)
    assert isinstance(img, np.ndarray)
    assert isinstance(size, tuple)
    assert len(size) == 2
    assert size[0] > 0 and size[1] > 0
```

---

## API Documentation

FastAPI auto-generates interactive docs — no extra work needed.

| URL | Interface |
|---|---|
| `http://localhost:8000/docs` | Swagger UI — try endpoints in browser |
| `http://localhost:8000/redoc` | ReDoc — clean reference docs |
| `http://localhost:8000/openapi.json` | Raw OpenAPI 3.0 schema |

---

## Verification Checklist
- [ ] `pytest tests/ -v` — all tests pass with no errors
- [ ] `test_extract_valid_image_returns_200` — `text_blocks` is non-empty for image with text
- [ ] `test_extract_blank_image_returns_empty_blocks` — empty list for blank image
- [ ] `test_extract_unsupported_format_returns_400` — correct error for PDF upload
- [ ] `test_extract_corrupt_image_returns_422` — correct error for corrupt bytes
- [ ] Swagger UI at `/docs` shows all endpoints with schemas
- [ ] Each `TextBlock` in response has `text`, `confidence`, and `bounding_box`
