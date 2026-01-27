using DAL.DTO;
using DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace DAL.Repository
{
    public class AIAnalysisRepository : IAIAnalysisRepository
    {
        private readonly PostgresContext _context;
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public AIAnalysisRepository(PostgresContext context, HttpClient http, IConfiguration config)
        {
            _context = context;
            _http = http;
            _config = config;
        }

        public async Task<Aianalysis> ProcessScreenshotAsync(IFormFile file)
        {
            var ocr = await CallMistralOcr(file);

            var upload = new Imageupload
            {
                Imageurl = file.FileName,
                Uploadtime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                Status = "Processed"
            };

            _context.Imageuploads.Add(upload);
            await _context.SaveChangesAsync();

            var analysis = new Aianalysis
            {
                Uploadid = upload.Uploadid,
                Aimodelversion = "pixtral-12b",
                Confidencescore = ocr.Confidence,
                Processedtime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
            };

            _context.Aianalyses.Add(analysis);
            await _context.SaveChangesAsync();

            foreach (var t in ocr.Texts)
            {
                _context.Aiextractedfields.Add(new Aiextractedfield
                {
                    Analysisid = analysis.Analysisid,
                    Rawtext = t.Text,
                    Fieldtype = "OCR",
                    Confidence = t.Confidence
                });
            }

            await _context.SaveChangesAsync();
            return analysis;
        }

        private async Task<MistralOcrResultDto> CallMistralOcr(IFormFile file)
        {
            var apiKey = _config["Mistral:ApiKey"] ?? throw new Exception("Missing Mistral API Key");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            if (ms.Length > 2 * 1024 * 1024)
                throw new Exception("Image too large. Max 2MB.");

            var base64Image = Convert.ToBase64String(ms.ToArray());

            var requestBody = new
            {
                model = "pixtral-12b",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = "Extract all visible text from this game screenshot" },
                            new
                            {
                                type = "image_url",
                                image_url = new
                                {
                                    url = $"data:image/png;base64,{base64Image}"
                                }
                            }
                        }
                    }
                }
            };

            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _http.PostAsJsonAsync(
                "https://api.mistral.ai/v1/chat/completions",
                requestBody
            );

            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(raw);
            return JsonSerializer.Deserialize<MistralOcrResultDto>(
                raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? throw new Exception("Failed to parse OCR result");

        }
    }
}
