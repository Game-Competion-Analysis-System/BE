# Image OCR Server — Project Overview

## Goal
Build a local FastAPI Python backend that:
1. Accepts image uploads via HTTP
2. Preprocesses images using OpenCV to maximize OCR accuracy
3. Runs Tesseract OCR to extract text
4. Returns structured JSON containing extracted text blocks, bounding boxes, and confidence scores

No external AI APIs — fully local and open-source stack.

---

## Tech Stack

| Layer | Technology | Version |
|---|---|---|
| Web Framework | FastAPI | ^0.111 |
| ASGI Server | Uvicorn | ^0.29 |
| Image Processing | OpenCV (`cv2`) | ^4.9 |
| OCR Engine | Tesseract (system) + pytesseract | 5.x / ^0.3 |
| Image Loading | Pillow | ^10.x |
| Data Validation | Pydantic v2 | ^2.x |
| File Upload | python-multipart | ^0.0.9 |
| Environment Config | python-dotenv | ^1.x |
| Testing | pytest + httpx | ^8.x / ^0.27 |

---

## Project Folder Structure

```
D:/DemoApp/
├── main.py                     # App entry point, mounts all routers
├── requirements.txt            # All Python dependencies
├── .env.example                # Environment variable template
│
├── routers/
│   └── ocr.py                  # POST /api/v1/extract endpoint
│
├── services/
│   └── ocr_service.py          # Core OCR business logic
│
├── models/
│   └── schemas.py              # Pydantic request/response models
│
├── utils/
│   └── image_utils.py          # OpenCV preprocessing helpers
│
├── tests/
│   ├── test_health.py          # Health check tests
│   ├── test_ocr.py             # API integration tests
│   └── test_preprocessing.py  # Unit tests for image utils
│
└── spec/                       # This folder — detailed phase docs
    ├── 00_overview.md
    ├── 01_phase1_setup.md
    ├── 02_phase2_ingestion.md
    ├── 03_phase3_preprocessing.md
    ├── 04_phase4_ocr.md
    ├── 05_phase5_response.md
    └── 06_phase6_testing.md
```

---

## API Summary

| Method | Endpoint | Description |
|---|---|---|
| GET | `/health` | Server health check |
| POST | `/api/v1/extract` | Upload image, receive OCR results |
| GET | `/docs` | Swagger UI (auto-generated) |
| GET | `/redoc` | ReDoc API docs (auto-generated) |

---

## Development Phases

| Phase | Focus | Key Output |
|---|---|---|
| 1 | Project Setup | Runnable skeleton, folder structure, health check |
| 2 | Image Ingestion | Upload endpoint, file validation, Pydantic schemas |
| 3 | Preprocessing | OpenCV pipeline (denoise, threshold, deskew) |
| 4 | OCR Extraction | Tesseract integration, text blocks with positions |
| 5 | Response & Errors | Structured JSON response, error handling |
| 6 | Testing & Docs | pytest suite, Swagger UI |
