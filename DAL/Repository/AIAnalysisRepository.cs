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
    public class AIAnalysisRepository(Swd392GameAiContext context, HttpClient http, IConfiguration config) : IAIAnalysisRepository
    {
        private readonly Swd392GameAiContext _context = context;
        private readonly HttpClient _http = http;
        private readonly IConfiguration _config = config;
        private readonly Cloudinary _cloudinary = new(new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            ));
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public async Task<Aianalysis> ProcessScreenshotAsync(IFormFile file, int userId, string gameName)
        {
            // 1. Upload to Cloudinary (Step 3-4) - Manual upload goes to "AI upload"
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = "AI upload"
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            
            if (uploadResult.Error != null)
                throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");

            var imageUrl = uploadResult.SecureUrl?.ToString() ?? throw new Exception("Cloudinary upload failed: No URL returned");
            return await ExecuteAnalysisFlow(imageUrl, userId, gameName);
        }

        public async Task<Aianalysis?> ProcessLatestImageFromCloudAsync(int userId, string gameName)
        {
            string? imageUrl = null;
            string? publicId = null;
            string? createdAt = null;

            // 1. Try SEARCH API first - Get from "HumanUpload" folder
            var searchResult = await _cloudinary.Search()
                .Expression("folder:\"HumanUpload\" AND resource_type:image")
                .SortBy("created_at", "desc")
                .MaxResults(1)
                .ExecuteAsync();

            var searchLatest = searchResult.Resources?.FirstOrDefault();
            if (searchLatest != null)
            {
                imageUrl = searchLatest.SecureUrl?.ToString();
                publicId = searchLatest.PublicId;
                createdAt = searchLatest.CreatedAt;
            }
            else
            {
                // 2. Fallback to ListResources
                Console.WriteLine("Cloudinary: Search API returned 0 results for 'HumanUpload'. Trying ListResources fallback...");
                var listParams = new ListResourcesByPrefixParams
                {
                    Type = "upload",
                    Prefix = "HumanUpload/",
                    MaxResults = 1,
                    Direction = "desc"
                };
                var listResources = await _cloudinary.ListResourcesAsync(listParams);
                var listLatest = listResources.Resources?.FirstOrDefault();
                if (listLatest != null)
                {
                    imageUrl = listLatest.SecureUrl?.ToString();
                    publicId = listLatest.PublicId;
                    createdAt = listLatest.CreatedAt;
                }
            }

            if (string.IsNullOrEmpty(imageUrl))
            {
                Console.WriteLine("Cloudinary: Truly no images found in 'game-analysis/' folder.");
                return null;
            }

            // CHECK IF ALREADY PROCESSED
            var exists = await _context.Imageuploads.AnyAsync(u => u.Imageurl == imageUrl);
            if (exists)
            {
                Console.WriteLine($"Cloudinary: Image {publicId} already processed. Skipping.");
                return null;
            }

            Console.WriteLine($"Cloudinary: Found latest image! PublicID: {publicId}, CreatedAt: {createdAt}, URL: {imageUrl}");

            // 3. Run analysis flow
            return await ExecuteAnalysisFlow(imageUrl, userId, gameName);
        }

        private async Task<Aianalysis> ExecuteAnalysisFlow(string imageUrl, int userId, string gameName)
        {
            // 2. Add ImageUpload with Status = "Pending"
            var upload = new Imageupload
            {
                Userid = userId,
                Imageurl = imageUrl,
                Uploadtime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                Status = "Pending"
            };
            _context.Imageuploads.Add(upload);
            await _context.SaveChangesAsync();

            Aianalysis? analysis = null;
            try 
            {
                // 3. Call AI OCR
                var ocr = await CallGroqOcrWithUrl(imageUrl);

                // 4. Add AIAnalysis
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
                
                // Robust JSON extraction
                string jsonContent = content;
                var startIdx = content.IndexOf('{');
                var endIdx = content.LastIndexOf('}');
                
                if (startIdx != -1 && endIdx != -1 && endIdx > startIdx)
                {
                    jsonContent = content.Substring(startIdx, endIdx - startIdx + 1);
                }

                try 
                {
                    using var data = JsonDocument.Parse(jsonContent, new JsonDocumentOptions { AllowTrailingCommas = true });
                    var root = data.RootElement;

                    // Cache for current request to avoid redundant queries
                    Dictionary<string, Game> gameCache = [];
                    Dictionary<string, Server> serverCache = [];
                    Dictionary<string, Guild> guildCache = [];
                    Dictionary<string, Player> playerCache = [];

                    // 1. Get Game from Parameter (User choice)
                    var targetGame = await _context.Games.FirstOrDefaultAsync(g => g.Gamename == gameName);
                    if (targetGame != null)
                    {
                        gameCache[gameName] = targetGame;
                        _context.Aiextractedfields.Add(new Aiextractedfield { Analysisid = analysis.Analysisid, Rawtext = gameName, Fieldtype = "GameName", Confidence = 1.0 });
                    }

                    // 2. Extract Server Name
                    Server? targetServer = null;
                    string? sn = null;
                    if (root.TryGetProperty("server_name", out var sElem)) sn = sElem.GetString();

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
                    string? en = null;
                    if (root.TryGetProperty("event_name", out var eElem)) en = eElem.GetString();

                    Event? targetEvent = await _context.Events.FirstOrDefaultAsync(e => e.Eventname == en && (targetGame == null || e.Gameid == targetGame.Gameid));
                    if (targetEvent == null && !string.IsNullOrEmpty(en))
                    {
                        targetEvent = new Event { Eventname = en, Game = targetGame, Startdate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified) };
                        _context.Events.Add(targetEvent);
                        await _context.SaveChangesAsync();
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
                            
                            string? guildNameStr = null;
                            if (item.TryGetProperty("guild_name", out var gNE)) guildNameStr = gNE.GetString();

                            if (string.IsNullOrEmpty(pName)) continue;

                            // Find or Create Guild
                            Guild? playerGuild = null;
                            if (!string.IsNullOrEmpty(guildNameStr))
                            {
                                if (!guildCache.TryGetValue(guildNameStr, out playerGuild))
                                {
                                    playerGuild = await _context.Guilds.FirstOrDefaultAsync(g => g.Guildname == guildNameStr && (targetServer == null || g.Serverid == targetServer.Serverid));
                                    if (playerGuild == null)
                                    {
                                        playerGuild = new Guild { Guildname = guildNameStr, Server = targetServer };
                                        _context.Guilds.Add(playerGuild);
                                        await _context.SaveChangesAsync();
                                    }
                                    guildCache[guildNameStr] = playerGuild;
                                }
                            }

                            // Find or Create Player
                            if (!playerCache.TryGetValue(pName, out var player))
                            {
                                player = await _context.Players.FirstOrDefaultAsync(p => p.Playername == pName && (targetGame == null || p.Gameid == targetGame.Gameid));
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
                                playerCache[pName] = player;
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
                if (upload != null)
                {
                    upload.Status = "Failed";
                    await _context.SaveChangesAsync();
                }
                Console.WriteLine($"Error processing AI result: {ex.Message}");
                throw;
            }

            return analysis;
        }

        private async Task<MistralOcrResultDto> CallGroqOcrWithUrl(string imageUrl)
        {
            var apiKey = _config["Groq:ApiKey"] ?? throw new Exception("Missing Groq API Key");

            var promptText = "Bạn là chuyên gia phân tích ảnh chụp màn hình bảng xếp hạng game VLTK Mobile và VLTK 2.0. " +
                "NHIỆM VỤ: Trích xuất thông tin chính xác từ ảnh theo định dạng JSON chuẩn. " +
                "YÊU CẦU DỮ LIỆU: " +
                "1. 'game_name': Phải là 'VLTK Mobile' hoặc 'VLTK 2.0'. " +
                "2. 'server_name': Tên máy chủ (ví dụ: Thái Sơn, S1...). " +
                "3. 'event_name': Tên sự kiện bảng xếp hạng (ví dụ: Công Thành Chiến, Võ Lâm Minh Chủ...). " +
                "4. 'leaderboard': Danh sách người chơi gồm: 'rank' (số nguyên), 'player_name' (tên chính xác), 'score' (điểm số hoặc lực chiến), 'guild_name' (tên bang hội nếu có). " +
                "QUAN TRỌNG: Chỉ trả về mã JSON duy nhất, không giải thích thêm. Nếu không thấy trường nào hãy để null. " +
                "Ví dụ định dạng trả về: { \"game_name\": \"...\", \"server_name\": \"...\", \"event_name\": \"...\", \"leaderboard\": [ { \"rank\": 1, \"player_name\": \"...\", \"score\": 100, \"guild_name\": \"...\" } ] }";

            var requestBody = new
            {
                model = "meta-llama/llama-4-scout-17b-16e-instruct", 
                messages = new object[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = promptText },
                            new
                            {
                                type = "image_url",
                                image_url = new { url = imageUrl }
                            }
                        }
                    }
                },
                temperature = 0.0,
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

        public async Task<(List<Aianalysis> Items, int TotalCount)> GetAllAsync(QueryParameters parameters, int? userId = null)
        {
            var query = _context.Aianalyses
                .Include(a => a.Aiextractedfields)
                .Include(a => a.Upload)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(a => a.Upload != null && a.Upload.Userid == userId.Value);
            }

            // Search (Search by AI model or extracted fields)
            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                var search = parameters.SearchTerm.ToLower();
                query = query.Where(a => 
                    (a.Aimodelversion != null && a.Aimodelversion.ToLower().Contains(search)) ||
                    a.Aiextractedfields.Any(f => f.Rawtext != null && f.Rawtext.ToLower().Contains(search)));
            }

            var totalCount = await query.CountAsync();

            // Sorting
            query = parameters.IsDescending 
                ? query.OrderByDescending(a => a.Processedtime) 
                : query.OrderBy(a => a.Processedtime);

            // Paging
            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (items, totalCount);
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
            if (analysis.Leaderboards.Count > 0)
            {
                foreach (var lb in analysis.Leaderboards)
                {
                    _context.Leaderboardentries.RemoveRange(lb.Leaderboardentries);
                }
                _context.Leaderboards.RemoveRange(analysis.Leaderboards);
            }

            // Delete Extracted Fields
            if (analysis.Aiextractedfields.Count > 0)
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

            var promptText = "Bạn là chuyên gia phân tích ảnh chụp màn hình bảng xếp hạng game VLTK Mobile và VLTK 2.0. " +
                "NHIỆM VỤ: Trích xuất thông tin chính xác từ ảnh theo định dạng JSON chuẩn. " +
                "YÊU CẦU DỮ LIỆU: " +
                "1. 'game_name': Phải là 'VLTK Mobile' hoặc 'VLTK 2.0'. " +
                "2. 'server_name': Tên máy chủ (ví dụ: Thái Sơn, S1...). " +
                "3. 'event_name': Tên sự kiện bảng xếp hạng (ví dụ: Công Thành Chiến, Võ Lâm Minh Chủ...). " +
                "4. 'leaderboard': Danh sách người chơi gồm: 'rank' (số nguyên), 'player_name' (tên chính xác), 'score' (điểm số hoặc lực chiến), 'guild_name' (tên bang hội nếu có). " +
                "QUAN TRỌNG: Chỉ trả về mã JSON duy nhất, không giải thích thêm. Nếu không thấy trường nào hãy để null. " +
                "Ví dụ định dạng trả về: { \"game_name\": \"...\", \"server_name\": \"...\", \"event_name\": \"...\", \"leaderboard\": [ { \"rank\": 1, \"player_name\": \"...\", \"score\": 100, \"guild_name\": \"...\" } ] }";

            var requestBody = new
            {
                model = "meta-llama/llama-4-scout-17b-16e-instruct", 
                messages = new object[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = promptText },
                            new
                            {
                                type = "image_url",
                                image_url = new { url = $"data:image/jpeg;base64,{base64Image}" }
                            }
                        }
                    }
                },
                temperature = 0.0,
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
