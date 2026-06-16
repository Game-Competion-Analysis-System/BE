import logging
import os

from fastapi import APIRouter, UploadFile, File, HTTPException, Query

from models.schemas import OCRResponse
from services import ocr_service

logger = logging.getLogger(__name__)

router = APIRouter()

ALLOWED_MIME_TYPES = {
    "image/jpeg", "image/png", "image/bmp", "image/tiff", "image/webp"
}
try:
    MAX_FILE_SIZE_MB = int(os.getenv("MAX_FILE_SIZE_MB", 10))
except ValueError:
    MAX_FILE_SIZE_MB = 10
SUPPORTED_LANGUAGES = set(os.getenv("SUPPORTED_LANGUAGES", "eng,vie").split(","))
MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024
VALID_IMAGE_TYPES = {"jpeg", "png", "bmp", "tiff", "webp"}


def _detect_image_type(header: bytes) -> str | None:
    """Return a normalised type string from magic bytes, or None if unrecognised.

    Replicates the subset of imghdr.what() needed here.
    imghdr was removed from the stdlib in Python 3.13.
    """
    if header[:3] == b"\xff\xd8\xff":
        return "jpeg"
    if header[:8] == b"\x89PNG\r\n\x1a\n":
        return "png"
    if header[:2] in (b"BM",):
        return "bmp"
    if header[:4] in (b"II\x2a\x00", b"MM\x00\x2a"):
        return "tiff"
    if header[:4] == b"RIFF" and header[8:12] == b"WEBP":
        return "webp"
    return None


@router.post("/extract", response_model=OCRResponse, tags=["OCR"])
async def extract_text(
    image: UploadFile = File(...),
    language: str = Query(default="eng", description="Language code (e.g. eng, vie, jpn)"),
):
    # 1. Validate language
    if language not in SUPPORTED_LANGUAGES:
        raise HTTPException(
            status_code=400,
            detail=f"Unsupported language '{language}'. Supported: {sorted(SUPPORTED_LANGUAGES)}",
        )

    # 2. Validate MIME type
    if image.content_type not in ALLOWED_MIME_TYPES:
        raise HTTPException(
            status_code=400,
            detail=f"Unsupported file type '{image.content_type}'. "
                   f"Allowed: {', '.join(sorted(ALLOWED_MIME_TYPES))}"
        )

    # 3. Read and enforce size limit
    # UploadFile.read() is the correct API in FastAPI 0.111 / Starlette;
    # async iteration over UploadFile was removed in newer Starlette versions.
    contents = await image.read()
    if len(contents) > MAX_FILE_SIZE_BYTES:
        raise HTTPException(
            status_code=413,
            detail=f"File too large. Maximum allowed size is {MAX_FILE_SIZE_MB} MB."
        )

    # 4. Validate actual file signature (not just client-supplied MIME)
    # Use 422 (Unprocessable Entity) here because the MIME type was acceptable
    # but the actual bytes cannot be interpreted as a valid image.
    detected = _detect_image_type(contents[:12])
    if detected not in VALID_IMAGE_TYPES:
        raise HTTPException(
            status_code=422,
            detail="File content does not match a supported image format."
        )

    # 5. Run OCR
    try:
        result = ocr_service.process(contents, image.filename or "unknown", language=language)
    except ValueError as e:
        raise HTTPException(status_code=422, detail=str(e))
    except RuntimeError as e:
        raise HTTPException(status_code=503, detail=str(e))
    except Exception:
        logger.exception("Unexpected error during OCR processing")
        raise HTTPException(
            status_code=500,
            detail="An internal error occurred during OCR processing."
        )

    return result
