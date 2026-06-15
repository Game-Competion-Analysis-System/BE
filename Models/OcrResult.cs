using System.Text.Json.Serialization;

namespace GameCompetionAnalysisSystem.Models
{
    public class OcrResult
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("filename")]
        public string? Filename { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("image_size")]
        public ImageSize? ImageSize { get; set; }

        [JsonPropertyName("full_text")]
        public string? FullText { get; set; }

        [JsonPropertyName("text_blocks")]
        public List<TextBlock>? TextBlocks { get; set; }

        [JsonPropertyName("processing_time_ms")]
        public double ProcessingTimeMs { get; set; }
    }

    public class ImageSize
    {
        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }

    public class TextBlock
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("bounding_box")]
        public BoundingBox? BoundingBox { get; set; }
    }

    public class BoundingBox
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
