using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace DAL.DTO
{
    public class MistralOcrResultDto
    {
        [JsonPropertyName("choices")]
        public List<MistralChoice> Choices { get; set; } = new();

        [JsonIgnore]
        public double Confidence => Texts.Count > 0 ? 0.9 : 0.0;

        [JsonIgnore]
        public List<OcrText> Texts
        {
            get
            {
                if (Choices == null || Choices.Count == 0)
                    return [];

                return
                [
                    new OcrText
                    {
                        Text = Choices[0].Message.Content,
                        Confidence = 0.9
                    }
                ];
            }
        }
    }

    public class MistralChoice
    {
        [JsonPropertyName("message")]
        public MistralMessage Message { get; set; } = new();
    }

    public class MistralMessage
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    public class OcrText
    {
        public string Text { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }
}
