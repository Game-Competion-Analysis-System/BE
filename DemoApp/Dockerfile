FROM python:3.11-slim

# System deps for OpenCV and EasyOCR
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgl1 \
    libglib2.0-0 \
    libsm6 \
    libxext6 \
    libxrender-dev \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Install Python deps first (layer cache)
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Pre-download EasyOCR models so cold-starts are fast on HF Spaces
RUN python -c "import easyocr; easyocr.Reader(['en','vi'], gpu=False); easyocr.Reader(['ja','en'], gpu=False)"

# Copy source
COPY main.py .
COPY routers/ routers/
COPY services/ services/
COPY models/ models/
COPY utils/ utils/

# HF Spaces requires port 7860
ENV PORT=7860
EXPOSE 7860

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "7860"]
