using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DAL.DTO;
using DAL.Entities;
using DAL.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace DAL.Repository
{
    public class AIAnalysisRepository : IAIAnalysisRepository
    {
        private readonly Swd392GameAiContext _context;
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly Cloudinary _cloudinary;
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public AIAnalysisRepository(Swd392GameAiContext context, HttpClient http, IConfiguration config)
        {
            _context = context;
            _http = http;
            _config = config;

            var acc = new Account(
                _config["Cloudinary:CloudName"],
                _config["Cloudinary:ApiKey"],
                _config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(acc);
        }

        public async Task<Aianalysis> ProcessScreenshotAsync(IFormFile file, int userId, int? eventId = null)
        {
            // 1. Upload to Cloudinary (Step 3-4)
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = "game-analysis"
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            
            if (uploadResult.Error != null)
                throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");

            var imageUrl = uploadResult.SecureUrl.ToString();

            // 2. Add ImageUpload with Status = "Pending" (Step 5)
            var upload = new Imageupload
            {
                Userid = userId,
                Eventid = eventId,
                Imageurl = imageUrl,
                Uploadtime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                Status = "Pending"
            };
            _context.Imageuploads.Add(upload);
            await _context.SaveChangesAsync(); // Step 6

            Aianalysis? analysis = null;
            try 
            {
                // 3. Call AI OCR (Step 7-9)
                var ocr = await CallGroqOcr(file);

                // 4. Add AIAnalysis (Step 10)
                analysis = new Aianalysis
                {
                    Uploadid = upload.Uploadid,
                    Aimodelversion = "meta-llama/llama-4-scout-17b-16e-instruct",
                    Confidencescore = ocr.Confidence,
                    Processedtime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
                };
                _context.Aianalyses.Add(analysis);
                await _context.SaveChangesAsync();

                // 5. Try to parse structured content if available
                var content = ocr.Choices?.FirstOrDefault()?.Message?.Content;
                if (string.IsNullOrEmpty(content))
                {
                    throw new Exception("AI returned empty content");
                }
                
                // Robust JSON extraction using a more flexible approach
                string jsonContent = content;
                var startIdx = content.IndexOf('{');
                var endIdx = content.LastIndexOf('}');
                
                if (startIdx != -1 && endIdx != -1 && endIdx > startIdx)
                {
                    jsonContent = content.Substring(startIdx, endIdx - startIdx + 1);
                }

                try 
                {
                    using var data = JsonDocument.Parse(jsonContent);
                    var root = data.RootElement;

                    // Cache for current request to avoid redundant queries
                    var gameCache = new Dictionary<string, Game>();
                    var serverCache = new Dictionary<string, Server>();
                    var guildCache = new Dictionary<string, Guild>();

                    // 1. Extract Game Name
                    Game? targetGame = null;
                    string? gn = null;
                    if (root.TryGetProperty("game_name", out var gElem) && gElem.ValueKind == JsonValueKind.String) gn = gElem.GetString();
                    
                    if (!string.IsNullOrEmpty(gn))
                    {
                        targetGame = await _context.Games.FirstOrDefaultAsync(g => g.Gamename == gn);
                        if (targetGame == null)
                        {
                            targetGame = new Game { Gamename = gn };
                            _context.Games.Add(targetGame);
                            await _context.SaveChangesAsync();
                        }
                        gameCache[gn] = targetGame;

                        _context.Aiextractedfields.Add(new Aiextractedfield { Analysisid = analysis.Analysisid, Rawtext = gn, Fieldtype = "GameName", Confidence = 0.99 });
                    }

                    // 2. Extract Server Name
                    Server? targetServer = null;
                    string? sn = null;
                    if (root.TryGetProperty("server_name", out var sElem) && sElem.ValueKind == JsonValueKind.String) sn = sElem.GetString();

                    if (!string.IsNullOrEmpty(sn))
                    {
                        targetServer = await _context.Servers.FirstOrDefaultAsync(s => s.Servername == sn && (targetGame == null || s.Gameid == targetGame.Gameid));
                        if (targetServer == null)
                        {
                            targetServer = new Server { Servername = sn, Game = targetGame };
                            _context.Servers.Add(targetServer);
                            await _context.SaveChangesAsync();
                        }
                        serverCache[sn] = targetServer;

                        _context.Aiextractedfields.Add(new Aiextractedfield { Analysisid = analysis.Analysisid, Rawtext = sn, Fieldtype = "ServerName", Confidence = 0.95 });
                    }

                    // 3. Extract Event Name
                    Event? targetEvent = null;
                    string? en = null;
                    if (eventId.HasValue)
                    {
                        targetEvent = await _context.Events.Include(e => e.Game).FirstOrDefaultAsync(e => e.Eventid == eventId.Value);
                        if (targetEvent != null && targetGame == null) targetGame = targetEvent.Game;
                    }
                    else if (root.TryGetProperty("event_name", out var eElem) && eElem.ValueKind == JsonValueKind.String)
                    {
                        en = eElem.GetString();
                    }

                    if (targetEvent == null && !string.IsNullOrEmpty(en))
                    {
                        targetEvent = await _context.Events.FirstOrDefaultAsync(e => e.Eventname == en && (targetGame == null || e.Gameid == targetGame.Gameid));
                        if (targetEvent == null)
                        {
                            targetEvent = new Event { Eventname = en, Game = targetGame, Startdate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified) };
                            _context.Events.Add(targetEvent);
                            await _context.SaveChangesAsync();
                        }

                        _context.Aiextractedfields.Add(new Aiextractedfield { Analysisid = analysis.Analysisid, Rawtext = en, Fieldtype = "EventName", Confidence = 0.95 });
                    }

                    // 4. Create Leaderboard
                    var lb = new Leaderboard
                    {
                        Title = $"Bảng xếp hạng {en ?? targetEvent?.Eventname ?? "mới"}",
                        Createdfromanalysisid = analysis.Analysisid,
                        Metrictype = "Score",
                        Event = targetEvent
                    };
                    _context.Leaderboards.Add(lb);
                    await _context.SaveChangesAsync();

                    // 5. Process Leaderboard Entries
                    if (root.TryGetProperty("leaderboard", out var lbArray) && lbArray.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in lbArray.EnumerateArray())
                        {
                            // Extract basic info
                            int rank = 0;
                            if (item.TryGetProperty("rank", out var rE)) { if (rE.ValueKind == JsonValueKind.Number) rE.TryGetInt32(out rank); else if (int.TryParse(rE.GetString() ?? "0", out var rS)) rank = rS; }
                            
                            string? pName = null;
                            if (item.TryGetProperty("player_name", out var pE)) pName = pE.GetString();
                            
                            double score = 0;
                            if (item.TryGetProperty("score", out var scE)) { if (scE.ValueKind == JsonValueKind.Number) scE.TryGetDouble(out score); else if (double.TryParse(scE.GetString() ?? "0", out var scS)) score = scS; }
                            
                            string? guildName = null;
                            if (item.TryGetProperty("guild_name", out var gNE)) guildName = gNE.GetString();

                            if (string.IsNullOrEmpty(pName)) continue;

                            // Find or Create Guild
                            Guild? playerGuild = null;
                            if (!string.IsNullOrEmpty(guildName))
                            {
                                if (!guildCache.TryGetValue(guildName, out playerGuild))
                                {
                                    playerGuild = await _context.Guilds.FirstOrDefaultAsync(g => g.Guildname == guildName && (targetServer == null || g.Serverid == targetServer.Serverid));
                                    if (playerGuild == null)
                                    {
                                        playerGuild = new Guild { Guildname = guildName, Server = targetServer };
                                        _context.Guilds.Add(playerGuild);
                                        await _context.SaveChangesAsync();
                                    }
                                    guildCache[guildName] = playerGuild;
                                }
                            }

                            // Find or Create Player
                            var player = await _context.Players.FirstOrDefaultAsync(p => p.Playername == pName && (targetGame == null || p.Gameid == targetGame.Gameid));
                            if (player == null)
                            {
                                player = new Player { Playername = pName, Game = targetGame, Server = targetServer, Guild = playerGuild };
                                _context.Players.Add(player);
                                await _context.SaveChangesAsync();
                            }
                            else 
                            {
                                if (player.Guildid == null && playerGuild != null) player.Guildid = playerGuild.Guildid;
                                if (player.Serverid == null && targetServer != null) player.Serverid = targetServer.Serverid;
                            }

                            // Add Entry
                            _context.Leaderboardentries.Add(new Leaderboardentry
                            {
                                Leaderboardid = lb.Leaderboardid,
                                Playerid = player.Playerid,
                                Rank = rank,
                                Value = score
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    upload.Status = "Success";
                    await _context.SaveChangesAsync();
                }
                catch (Exception jsonEx)
                {
                    Console.WriteLine($"JSON Parse Error: {jsonEx.Message}. Content: {jsonContent}");
                    throw new Exception($"Failed to parse AI result: {jsonEx.Message}");
                }
            }
            catch (Exception ex)
            {
                // Step 15-16: Failed
                if (upload != null)
                {
                    upload.Status = "Failed";
                    await _context.SaveChangesAsync();
                }

                // Log the error for debugging
                Console.WriteLine($"Error processing AI result: {ex.Message}");
                throw; // Step 17: Rethrow to let Controller return 400 Bad Request
            }

            return analysis;
        }

        public async Task<List<Aianalysis>> GetAllAsync(int? userId = null)
        {
            var query = _context.Aianalyses
                .Include(a => a.Aiextractedfields)
                .Include(a => a.Upload)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(a => a.Upload.Userid == userId.Value);
            }

            return await query.OrderByDescending(a => a.Processedtime).ToListAsync();
        }

        public async Task<Aianalysis?> GetByIdAsync(int id)
        {
            return await _context.Aianalyses.Include(a => a.Aiextractedfields).FirstOrDefaultAsync(a => a.Analysisid == id);
        }

        public async Task<Aianalysis?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Aianalyses
                .Include(a => a.Upload)
                .Include(a => a.Aiextractedfields)
                .Include(a => a.Leaderboards)
                    .ThenInclude(lb => lb.Leaderboardentries)
                        .ThenInclude(e => e.Player)
                            .ThenInclude(p => p.Guild)
                .Include(a => a.Leaderboards)
                    .ThenInclude(lb => lb.Event)
                        .ThenInclude(ev => ev.Game)
                .Include(a => a.Leaderboards)
                    .ThenInclude(lb => lb.Event)
                        .ThenInclude(ev => ev.Game)
                            .ThenInclude(g => g.Servers)
                .FirstOrDefaultAsync(a => a.Analysisid == id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var analysis = await _context.Aianalyses
                .Include(a => a.Aiextractedfields)
                .Include(a => a.Leaderboards)
                    .ThenInclude(lb => lb.Leaderboardentries)
                .Include(a => a.Upload)
                .FirstOrDefaultAsync(a => a.Analysisid == id);

            if (analysis == null) return false;

            // Delete Leaderboards and their entries
            if (analysis.Leaderboards.Any())
            {
                foreach (var lb in analysis.Leaderboards)
                {
                    _context.Leaderboardentries.RemoveRange(lb.Leaderboardentries);
                }
                _context.Leaderboards.RemoveRange(analysis.Leaderboards);
            }

            // Delete Extracted Fields
            if (analysis.Aiextractedfields.Any())
            {
                _context.Aiextractedfields.RemoveRange(analysis.Aiextractedfields);
            }

            // Delete Upload if exists
            if (analysis.Upload != null)
            {
                _context.Imageuploads.Remove(analysis.Upload);
            }

            _context.Aianalyses.Remove(analysis);
            
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<MistralOcrResultDto> CallGroqOcr(IFormFile file)
        {
            var apiKey = _config["Groq:ApiKey"] ?? throw new Exception("Missing Groq API Key");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            if (ms.Length > 2 * 1024 * 1024)
                throw new Exception("Image too large. Max 2MB.");

            var base64Image = Convert.ToBase64String(ms.ToArray());

            var promptText = "Phân tích ảnh chụp màn hình bảng xếp hạng game này. " +
                "Trích xuất thông tin: Tên Game (Game Name), Tên Máy Chủ (Server Name), Tên Sự Kiện (Event Name), Tên Bang Hội (Guild Name). " +
                "Đối với danh sách bảng xếp hạng, trích xuất: Hạng (Rank), Tên Người Chơi (Player Name), Điểm Số/Lực Chiến (Score), Bang Hội (Guild Name). " +
                "QUAN TRỌNG: Trả về JSON chuẩn với cấu trúc: " +
                "{ 'game_name': '...', 'server_name': '...', 'event_name': '...', 'leaderboard': [ { 'rank': 1, 'player_name': '...', 'score': 100, 'guild_name': '...' } ] }. " +
                "Nếu không thấy thông tin nào, hãy để null. Nếu thấy 'Bang Hội' (Guild) cho từng người chơi, hãy trích xuất chính xác. " +
                "Ví dụ tên bang hội: 'ĐẠI-VIỆT', 'TAE_TụNghĩa'.";

            var requestBody = new
            {
                model = "meta-llama/llama-4-scout-17b-16e-instruct", 
                messages = (object[])
                [
                    new
                    {
                        role = "user",
                        content = (object)new object[]
                        {
                            new { type = "text", text = promptText },
                            new
                            {
                                type = "image_url",
                                image_url = new
                                {
                                    url = $"data:image/jpeg;base64,{base64Image}"
                                }
                            }
                        }
                    }
                ],
                temperature = 0.1,
                response_format = new { type = "json_object" }
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
