using System.Net.Http.Headers;
using System.Text.Json;
using GameCompetionAnalysisSystem.Models;

namespace GameCompetionAnalysisSystem.Services
{
    public class OcrService : IOcrService
    {
        private readonly HttpClient _httpClient;
        private readonly string _extractEndpoint;

        public OcrService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            var baseUrl = configuration["OcrApi:BaseUrl"]
                          ?? "https://hxvf123-demoocrserver.hf.space";
            _extractEndpoint = $"{baseUrl.TrimEnd('/')}/api/v1/extract";
        }

        public async Task<OcrResult> ExtractTextAsync(IFormFile file, string language = "eng")
        {
            using var form = new MultipartFormDataContent();

            // language field
            form.Add(new StringContent(language), "language");

            // image file
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(file.ContentType ?? "image/png");
            form.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync(_extractEndpoint, form);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<OcrResult>(json,
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new OcrResult { Success = false };
        }
    }
}
