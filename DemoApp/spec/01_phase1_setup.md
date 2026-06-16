# Phase 1 — Project Setup & Foundation

## Goal
Bootstrap a clean, runnable FastAPI skeleton with structured logging, environment config, and a health check endpoint. After this phase the server starts and responds — no business logic yet.

---

## Prerequisites

### System Requirements
- Python 3.11 or higher
- pip + venv
- Git

### Install Python Virtual Environment
```bash
cd D:/DemoApp
python -m venv venv

# Activate (Windows)
venv\Scripts\activate

# Activate (macOS/Linux)
source venv/bin/activate
```

---

## Dependencies

### `requirements.txt`
```
fastapi==0.111.0
uvicorn[standard]==0.29.0
python-multipart==0.0.9
opencv-python==4.9.0.80
pytesseract==0.3.10
Pillow==10.3.0
pydantic==2.7.0
python-dotenv==1.0.1
pytest==8.2.0
httpx==0.27.0
```

Install with:
```bash
pip install -r requirements.txt
```

---

## Environment Configuration

### `.env.example`
```
# Server
HOST=0.0.0.0
PORT=8000

# Tesseract binary path (Windows example)
TESSERACT_CMD=C:/Program Files/Tesseract-OCR/tesseract.exe

# Upload limits
MAX_FILE_SIZE_MB=10
```

Copy to `.env` and fill in values:
```bash
cp .env.example .env
```

---

## Files to Create

### `main.py`
```python
import logging
from fastapi import FastAPI
from dotenv import load_dotenv

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

@app.get("/health", tags=["Health"])
def health_check():
    return {"status": "ok", "version": "1.0.0"}
```

Run the server:
```bash
uvicorn main:app --reload --host 0.0.0.0 --port 8000
```

---

## Verification Checklist
- [ ] `pip install -r requirements.txt` completes without errors
- [ ] `uvicorn main:app --reload` starts without errors
- [ ] `GET http://localhost:8000/health` returns `{"status": "ok", "version": "1.0.0"}`
- [ ] `GET http://localhost:8000/docs` shows Swagger UI
