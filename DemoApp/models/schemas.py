from pydantic import BaseModel, Field

class BoundingBox(BaseModel):
    x: int = Field(ge=0)
    y: int = Field(ge=0)
    width: int = Field(ge=0)
    height: int = Field(ge=0)

class TextBlock(BaseModel):
    text: str
    confidence: float = Field(ge=0.0, le=100.0)
    bounding_box: BoundingBox

class ImageSize(BaseModel):
    width: int
    height: int

class OCRResponse(BaseModel):
    success: bool
    filename: str
    language: str = "eng"
    image_size: ImageSize
    full_text: str
    text_blocks: list[TextBlock]
    processing_time_ms: float
