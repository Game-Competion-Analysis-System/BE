import io
import pytest
import numpy as np
import cv2
from unittest.mock import patch, MagicMock
from httpx import AsyncClient, ASGITransport
from main import app
from models.schemas import OCRResponse, TextBlock, BoundingBox, ImageSize


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


@pytest.fixture
def mock_ocr_response():
    """Canned OCR response for tests that don't need real Tesseract."""
    return OCRResponse(
        success=True,
        filename="test.jpg",
        image_size=ImageSize(width=400, height=100),
        full_text="Hello World",
        text_blocks=[
            TextBlock(
                text="Hello",
                confidence=98.0,
                bounding_box=BoundingBox(x=10, y=30, width=80, height=30),
            ),
            TextBlock(
                text="World",
                confidence=97.5,
                bounding_box=BoundingBox(x=100, y=30, width=80, height=30),
            ),
        ],
        language="eng",
        processing_time_ms=123.4,
    )


@pytest.fixture
def patched_ocr(mock_ocr_response):
    """Patch ocr_service.process to return canned response."""
    with patch("services.ocr_service.process", return_value=mock_ocr_response):
        yield mock_ocr_response


@pytest.fixture
def mock_blank_ocr_response():
    """Canned OCR response simulating a blank image."""
    return OCRResponse(
        success=True,
        filename="blank.jpg",
        image_size=ImageSize(width=400, height=100),
        full_text="",
        text_blocks=[],
        language="eng",
        processing_time_ms=50.0,
    )


@pytest.fixture
def patched_ocr_blank(mock_blank_ocr_response):
    with patch("services.ocr_service.process", return_value=mock_blank_ocr_response):
        yield mock_blank_ocr_response
