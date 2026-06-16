---
title: Image OCR Server
emoji: 🔍
colorFrom: blue
colorTo: green
sdk: docker
pinned: false
license: mit
---

# Image OCR Server

Extract text from images using **EasyOCR** + **OpenCV**.

## Supported Languages
| Code | Language   |
|------|------------|
| eng  | English    |
| vie  | Vietnamese |
| jpn  | Japanese   |

## API Endpoints

### `POST /api/v1/extract`
Upload an image and get extracted text.

**Query Parameters:**
- `language` — `eng` (default), `vie`, or `jpn`

**Accepted formats:** JPEG, PNG, BMP, TIFF, WebP (max 10 MB)

### `GET /health`
Returns server status.

### `GET /docs`
Interactive Swagger UI — try the API directly in your browser.

## Example (curl)
```bash
curl -X POST "https://<your-space-url>/api/v1/extract?language=eng" \
  -F "image=@photo.jpg"
```

## Built With
- FastAPI
- EasyOCR
- OpenCV
