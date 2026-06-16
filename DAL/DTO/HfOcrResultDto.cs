using System.Text.Json.Serialization;

namespace DAL.DTO
{
    public class HfOcrResultDto
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("filename")]
        public string? Filename { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("image_size")]
        public HfImageSize? ImageSize { get; set; }

        [JsonPropertyName("full_text")]
        public string? FullText { get; set; }

        [JsonPropertyName("text_blocks")]
        public List<HfTextBlock> TextBlocks { get; set; } = [];

        [JsonPropertyName("processing_time_ms")]
        public double ProcessingTimeMs { get; set; }
    }

    public class HfImageSize
    {
        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }

    public class HfTextBlock
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("bounding_box")]
        public HfBoundingBox? BoundingBox { get; set; }
    }

    public class HfBoundingBox
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }
}
