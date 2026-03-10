using DAL.DTO;
using DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace DAL.Repository
{
    public class AIAnalysisRepository(Swd392GameAiContext context, HttpClient http, IConfiguration config) : IAIAnalysisRepository
    {
        private readonly Swd392GameAiContext _context = context;
        private readonly HttpClient _http = http;
        private readonly IConfiguration _config = config;
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public async Task<Aianalysis> ProcessScreenshotAsync(IFormFile file)
        {
            var ocr = await CallGroqOcr(file);

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
                Aimodelversion = "meta-llama/llama-4-scout-17b-16e-instruct",
                Confidencescore = ocr.Confidence,
                Processedtime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
            };

            _context.Aianalyses.Add(analysis);
            await _context.SaveChangesAsync();

            // Try to parse structured content if available
            try 
            {
                var content = ocr.Choices?.FirstOrDefault()?.Message?.Content;
                if (string.IsNullOrEmpty(content))
                {
                    throw new Exception("AI returned empty content");
                }
                
                // Robust JSON extraction
                var jsonStartIndex = content.IndexOf('{');
                var jsonEndIndex = content.LastIndexOf('}');
                
                if (jsonStartIndex != -1 && jsonEndIndex != -1 && jsonEndIndex > jsonStartIndex)
                {
                    content = content.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);
                    
                    var data = JsonDocument.Parse(content);
                    var root = data.RootElement;

                    Game? targetGame = null;
                    if (root.TryGetProperty("game_name", out var gameNameElem) && gameNameElem.ValueKind != JsonValueKind.Null)
                    {
                        var gn = gameNameElem.GetString();
                        if (!string.IsNullOrEmpty(gn))
                        {
                            _context.Aiextractedfields.Add(new Aiextractedfield
                            {
                                Analysisid = analysis.Analysisid,
                                Rawtext = gn.Length > 500 ? string.Concat(gn.AsSpan(0, 497), "...") : gn,
                                Fieldtype = "GameName",
                                Confidence = 0.98
                            });

                            // Find in DB or Local context
                            targetGame = await _context.Games.FirstOrDefaultAsync(g => g.Gamename == gn) 
                                         ?? _context.Games.Local.FirstOrDefault(g => g.Gamename == gn);
                            
                            if (targetGame == null)
                            {
                                targetGame = new Game { Gamename = gn };
                                _context.Games.Add(targetGame);
                            }
                        }
                    }

                    // Create Leaderboard
                    var lb = new Leaderboard
                    {
                        Title = $"Leaderboard from Analysis #{analysis.Analysisid}",
                        Createdfromanalysisid = analysis.Analysisid,
                        Metrictype = "Score"
                    };
                    _context.Leaderboards.Add(lb);

                    if (root.TryGetProperty("leaderboard", out var leaderboard) && leaderboard.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in leaderboard.EnumerateArray())
                        {
                            int rank = 0;
                            if (item.TryGetProperty("rank", out var rElem) && rElem.ValueKind != JsonValueKind.Null)
                            {
                                if (rElem.ValueKind == JsonValueKind.Number) rElem.TryGetInt32(out rank);
                                else if (rElem.ValueKind == JsonValueKind.String && int.TryParse(rElem.GetString(), out var rStr)) rank = rStr;
                            }

                            string pName = "Unknown";
                            if (item.TryGetProperty("player_name", out var pElem) && pElem.ValueKind != JsonValueKind.Null)
                            {
                                pName = pElem.GetString() ?? "Unknown";
                            }

                            double score = 0;
                            if (item.TryGetProperty("score", out var sElem) && sElem.ValueKind != JsonValueKind.Null)
                            {
                                if (sElem.ValueKind == JsonValueKind.Number) sElem.TryGetDouble(out score);
                                else if (sElem.ValueKind == JsonValueKind.String && double.TryParse(sElem.GetString(), out var sStr)) score = sStr;
                            }

                            // Find or create player
                            Player? player = null;
                            if (!string.IsNullOrEmpty(pName) && pName != "Unknown")
                            {
                                // Check DB and Local
                                player = await _context.Players.FirstOrDefaultAsync(p => p.Playername == pName && p.Gameid == (targetGame != null ? targetGame.Gameid : null))
                                         ?? _context.Players.Local.FirstOrDefault(p => p.Playername == pName && (targetGame == null || p.Game == targetGame || p.Gameid == targetGame.Gameid));
                                
                                if (player == null)
                                {
                                    player = new Player 
                                    { 
                                        Playername = pName,
                                        Game = targetGame
                                    };
                                    _context.Players.Add(player);
                                }
                            }

                            // Save as extracted field for history
                            var playerInfo = $"Rank: {rank}, Player: {pName}, Score: {score}";
                            _context.Aiextractedfields.Add(new Aiextractedfield
                            {
                                Analysisid = analysis.Analysisid,
                                Rawtext = playerInfo.Length > 500 ? string.Concat(playerInfo.AsSpan(0, 497), "...") : playerInfo,
                                Fieldtype = "LeaderboardRow",
                                Confidence = 0.95
                            });

                            // Save to LeaderboardEntry
                            if (player != null)
                            {
                                _context.Leaderboardentries.Add(new Leaderboardentry
                                {
                                    Leaderboard = lb,
                                    Player = player,
                                    Rank = rank,
                                    Value = score
                                });
                            }
                        }
                    }
                    
                    // Final save for all entities in the try block
                    await _context.SaveChangesAsync();
                }
                else 
                {
                    // Fallback if no JSON found
                    foreach (var t in ocr.Texts)
                    {
                        var rawText = t.Text;
                        if (rawText.Length > 500)
                        {
                            rawText = string.Concat(rawText.AsSpan(0, 497), "...");
                        }

                        _context.Aiextractedfields.Add(new Aiextractedfield
                        {
                            Analysisid = analysis.Analysisid,
                            Rawtext = rawText,
                            Fieldtype = "OCR",
                            Confidence = t.Confidence
                        });
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error processing AI result: {ex.Message}");

                // Fallback to original logic if parsing fails
                foreach (var t in ocr.Texts)
                {
                    var rawText = t.Text;
                    if (rawText.Length > 500)
                    {
                        rawText = string.Concat(rawText.AsSpan(0, 497), "...");
                    }

                    _context.Aiextractedfields.Add(new Aiextractedfield
                    {
                        Analysisid = analysis.Analysisid,
                        Rawtext = rawText,
                        Fieldtype = "OCR",
                        Confidence = t.Confidence
                    });
                }
                await _context.SaveChangesAsync();
            }

            return analysis;
        }

        public async Task<List<Aianalysis>> GetAllAsync()
        {
            return await _context.Aianalyses.Include(a => a.Aiextractedfields).ToListAsync();
        }

        public async Task<Aianalysis?> GetByIdAsync(int id)
        {
            return await _context.Aianalyses.Include(a => a.Aiextractedfields).FirstOrDefaultAsync(a => a.Analysisid == id);
        }

        private async Task<MistralOcrResultDto> CallGroqOcr(IFormFile file)
        {
            var apiKey = _config["Groq:ApiKey"] ?? throw new Exception("Missing Groq API Key");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            if (ms.Length > 2 * 1024 * 1024)
                throw new Exception("Image too large. Max 2MB.");

            var base64Image = Convert.ToBase64String(ms.ToArray());

            var requestBody = new
            {
                model = "meta-llama/llama-4-scout-17b-16e-instruct", 
                messages = (object[])
                [
                    new
                    {
                        role = "system",
                        content = (object)"You are an expert game data analyzer. Extract structured information from game leaderboard screenshots. Respond only with a JSON object."
                    },
                    new
                    {
                        role = "user",
                        content = (object)new object[]
                        {
                            new { type = "text", text = "Extract game name, player names, ranks, and scores from this leaderboard. Return as JSON with structure: { 'game_name': '...', 'leaderboard': [ { 'rank': 1, 'player_name': '...', 'score': 100 } ] }" },
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
                ]
            };

            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _http.PostAsJsonAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                requestBody
            );

            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(raw);
            return JsonSerializer.Deserialize<MistralOcrResultDto>(
                raw,
                _jsonOptions
            ) ?? throw new Exception("Failed to parse OCR result");

        }
    }
}
