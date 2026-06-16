# Phase 2 — Image Ingestion Layer

## Goal
Implement the `POST /api/v1/extract` endpoint skeleton that receives image uploads, validates them (format + file size), and wires up Pydantic schemas. OCR processing is stubbed — full logic added in Phase 4.

---

## Supported Image Formats
| Format | MIME Type |
|---|---|
| JPEG | `image/jpeg` |
| PNG | `image/png` |
| BMP | `image/bmp` |
| TIFF | `image/tiff` |
| WEBP | `image/webp` |

---

## Pydantic Schemas

### `models/schemas.py`
```python
from pydantic import BaseModel

class BoundingBox(BaseModel):
    x: int
    y: int
    width: int
    height: int

class TextBlock(BaseModel):
    text: str
    confidence: float          # 0.0 – 100.0
    bounding_box: BoundingBox

class ImageSize(BaseModel):
    width: int
    height: int

class OCRResponse(BaseModel):
    success: bool
    filename: str
    image_size: ImageSize
    full_text: str
    text_blocks: list[TextBlock]
    processing_time_ms: float
```

---

## Endpoint

### `routers/ocr.py`
```python
import os
from fastapi import APIRouter, UploadFile, File, HTTPException
from models.schemas import OCRResponse

router = APIRouter()

ALLOWED_MIME_TYPES = {
    "image/jpeg", "image/png", "image/bmp", "image/tiff", "image/webp"
}
MAX_FILE_SIZE_MB = int(os.getenv("MAX_FILE_SIZE_MB", 10))
MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024


@router.post("/extract", response_model=OCRResponse, tags=["OCR"])
async def extract_text(image: UploadFile = File(...)):
    # 1. Validate MIME type
    if image.content_type not in ALLOWED_MIME_TYPES:
        raise HTTPException(
            status_code=400,
            detail=f"Unsupported file type '{image.content_type}'. "
                   f"Allowed: {', '.join(ALLOWED_MIME_TYPES)}"
        )

    # 2. Read bytes and check size
    contents = await image.read()
    if len(contents) > MAX_FILE_SIZE_BYTES:
        raise HTTPException(
            status_code=413,
            detail=f"File too large. Maximum allowed size is {MAX_FILE_SIZE_MB} MB."
        )

    # 3. TODO: Pass to OCR service (Phase 4)
    # result = ocr_service.process(contents, image.filename)
    # return result

    # Stub response for Phase 2
    return OCRResponse(
        success=True,
        filename=image.filename or "unknown",
        image_size={"width": 0, "height": 0},
        full_text="",
        text_blocks=[],
        processing_time_ms=0.0,
    )
```

### Register Router in `main.py`
```python
from routers.ocr import router as ocr_router

app.include_router(ocr_router, prefix="/api/v1")
```

---

## Request Format
```
POST /api/v1/extract
Content-Type: multipart/form-data

Body:
  image: <file>    (required)
```

Example with curl:
```bash
curl -X POST http://localhost:8000/api/v1/extract \
  -F "image=@/path/to/photo.jpg"
```

---

## Error Responses

| HTTP Code | Scenario |
|---|---|
| 400 | Unsupported image format |
| 413 | File exceeds size limit |
| 422 | Missing `image` field in request |

---

## Verification Checklist
- [ ] `POST /api/v1/extract` with a valid JPEG returns stub `OCRResponse`
- [ ] Uploading a `.txt` file returns `400` with descriptive message
- [ ] Uploading a file over 10 MB returns `413`
- [ ] Missing `image` field returns `422`
- [ ] Schemas visible in Swagger UI at `/docs`
