# Phase 5 — Response Formatting & Error Handling

## Goal
Finalize the API response contract, implement consistent error handling across all failure modes, and add global middleware for request logging and processing time tracking.

---

## Final Response Schema

All successful responses follow this structure:

```json
{
  "success": true,
  "filename": "photo.jpg",
  "image_size": {
    "width": 1920,
    "height": 1080
  },
  "full_text": "Invoice #1042\nDate: 2024-03-15\nTotal: $250.00",
  "text_blocks": [
    {
      "text": "Invoice",
      "confidence": 98.5,
      "bounding_box": {
        "x": 120,
        "y": 45,
        "width": 180,
        "height": 32
      }
    },
    {
      "text": "#1042",
      "confidence": 95.1,
      "bounding_box": {
        "x": 310,
        "y": 45,
        "width": 100,
        "height": 32
      }
    }
  ],
  "processing_time_ms": 312.4
}
```

---

## Error Response Schema

All error responses follow FastAPI's default format:

```json
{
  "detail": "Human-readable error message here"
}
```

---

## Error Code Reference

| HTTP Code | Trigger | Message Example |
|---|---|---|
| `400` | Unsupported file format | `"Unsupported file type 'application/pdf'. Allowed: image/jpeg, image/png..."` |
| `413` | File exceeds size limit | `"File too large. Maximum allowed size is 10 MB."` |
| `422` | Corrupt / unreadable image | `"Could not decode image. File may be corrupt or unsupported."` |
| `422` | Missing `image` field | FastAPI auto-generates from Pydantic validation |
| `500` | Unexpected server error | `"An internal error occurred. Please try again."` |

---

## Global Exception Handler

Add to `main.py` to catch any unhandled exceptions and return a clean 500:

```python
from fastapi import Request
from fastapi.responses import JSONResponse

@app.exception_handler(Exception)
async def global_exception_handler(request: Request, exc: Exception):
    logger.exception("Unhandled error on %s %s", request.method, request.url.path)
    return JSONResponse(
        status_code=500,
        content={"detail": "An internal error occurred. Please try again."},
    )
```

---

## Request Logging Middleware

Add to `main.py` to log every request with method, path, status code, and duration:

```python
import time
from starlette.middleware.base import BaseHTTPMiddleware

class LoggingMiddleware(BaseHTTPMiddleware):
    async def dispatch(self, request: Request, call_next):
        start = time.perf_counter()
        response = await call_next(request)
        duration_ms = round((time.perf_counter() - start) * 1000, 2)
        logger.info(
            "%s %s → %d (%.1f ms)",
            request.method,
            request.url.path,
            response.status_code,
            duration_ms,
        )
        return response

app.add_middleware(LoggingMiddleware)
```

---

## Final `main.py` Structure

```python
import logging
import time
from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse
from starlette.middleware.base import BaseHTTPMiddleware
from dotenv import load_dotenv
from routers.ocr import router as ocr_router

load_dotenv()

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
)
logger = logging.getLogger(__name__)

app = FastAPI(
    title="Image OCR Server",
    description="Extract text from images using OpenCV + Tesseract",
    version="1.0.0",
)

# Middleware
class LoggingMiddleware(BaseHTTPMiddleware):
    async def dispatch(self, request: Request, call_next):
        start = time.perf_counter()
        response = await call_next(request)
        duration_ms = round((time.perf_counter() - start) * 1000, 2)
        logger.info("%s %s → %d (%.1f ms)",
                    request.method, request.url.path,
                    response.status_code, duration_ms)
        return response

app.add_middleware(LoggingMiddleware)

# Global error handler
@app.exception_handler(Exception)
async def global_exception_handler(request: Request, exc: Exception):
    logger.exception("Unhandled error on %s %s", request.method, request.url.path)
    return JSONResponse(status_code=500, content={"detail": "An internal error occurred."})

# Routes
app.include_router(ocr_router, prefix="/api/v1")

@app.get("/health", tags=["Health"])
def health_check():
    return {"status": "ok", "version": "1.0.0"}
```

---

## Verification Checklist
- [ ] Valid image upload returns full `OCRResponse` JSON with all fields present
- [ ] `.txt` file upload returns `400` with descriptive message
- [ ] Oversized file returns `413`
- [ ] Corrupt image bytes returns `422`
- [ ] Every request is logged with method, path, status, and duration
- [ ] Deliberately raising an unhandled exception returns `500` with safe message (no stack trace exposed)
