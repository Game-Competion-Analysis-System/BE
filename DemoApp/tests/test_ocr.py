import pytest


@pytest.mark.anyio
async def test_extract_valid_image_returns_200(client, sample_image_bytes, patched_ocr):
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
async def test_extract_blank_image_returns_empty_blocks(client, blank_image_bytes, patched_ocr_blank):
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
async def test_response_has_image_size(client, sample_image_bytes, patched_ocr):
    response = await client.post(
        "/api/v1/extract",
        files={"image": ("test.jpg", sample_image_bytes, "image/jpeg")},
    )
    assert response.status_code == 200
    body = response.json()
    assert body["image_size"]["width"] > 0
    assert body["image_size"]["height"] > 0


@pytest.mark.anyio
async def test_text_blocks_have_required_fields(client, sample_image_bytes, patched_ocr):
    response = await client.post(
        "/api/v1/extract",
        files={"image": ("test.jpg", sample_image_bytes, "image/jpeg")},
    )
    assert response.status_code == 200
    for block in response.json()["text_blocks"]:
        assert "text" in block
        assert "confidence" in block
        assert "bounding_box" in block
        bb = block["bounding_box"]
        assert all(k in bb for k in ("x", "y", "width", "height"))


@pytest.mark.anyio
async def test_extract_oversized_file_returns_413(client):
    # Generate a payload just over 10 MB
    big_payload = b"x" * (10 * 1024 * 1024 + 1)
    response = await client.post(
        "/api/v1/extract",
        files={"image": ("big.jpg", big_payload, "image/jpeg")},
    )
    assert response.status_code == 413


@pytest.mark.anyio
async def test_extract_default_language_is_eng(client, sample_image_bytes, patched_ocr):
    response = await client.post(
        "/api/v1/extract",
        files={"image": ("test.jpg", sample_image_bytes, "image/jpeg")},
    )
    assert response.status_code == 200
    assert response.json()["language"] == "eng"


@pytest.mark.anyio
async def test_extract_vietnamese_language_returns_200(client, sample_image_bytes):
    vie_response = __import__("models.schemas", fromlist=["OCRResponse"]).OCRResponse(
        success=True,
        filename="test.jpg",
        language="vie",
        image_size=__import__("models.schemas", fromlist=["ImageSize"]).ImageSize(width=400, height=100),
        full_text="Xin chào",
        text_blocks=[],
        processing_time_ms=100.0,
    )
    from unittest.mock import patch
    with patch("services.ocr_service.process", return_value=vie_response):
        response = await client.post(
            "/api/v1/extract?language=vie",
            files={"image": ("test.jpg", sample_image_bytes, "image/jpeg")},
        )
    assert response.status_code == 200
    assert response.json()["language"] == "vie"


@pytest.mark.anyio
async def test_extract_unsupported_language_returns_400(client, sample_image_bytes):
    response = await client.post(
        "/api/v1/extract?language=xyz",
        files={"image": ("test.jpg", sample_image_bytes, "image/jpeg")},
    )
    assert response.status_code == 400
    assert "xyz" in response.json()["detail"]
