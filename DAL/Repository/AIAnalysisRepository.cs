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
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public async Task<Aianalysis> ProcessScreenshotAsync(IFormFile file, int userId)
        {
            var ocr = await CallGroqOcr(file);

            var upload = new  Imageupload
            {
                Userid = userId,
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

                    // Cache for current request to avoid redundant queries
                    var gameCache = new Dictionary<string, Game>();
                    var serverCache = new Dictionary<string, Server>();
                    var guildCache = new Dictionary<string, Guild>();
                    var playerCache = new Dictionary<string, Player>();

                    Game? targetGame = null;
                    if (root.TryGetProperty("game_name", out var gameNameElem) && gameNameElem.ValueKind != JsonValueKind.Null)
                    {
                        var gn = gameNameElem.GetString();
                        if (!string.IsNullOrEmpty(gn))
                        {
                            if (!gameCache.TryGetValue(gn, out targetGame))
                            {
                                targetGame = await _context.Games.FirstOrDefaultAsync(g => g.Gamename == gn);
                                if (targetGame == null)
                                {
                                    var normalizedGn = StringNormalizationHelper.Normalize(gn);
                                    var allGames = await _context.Games.ToListAsync();
                                    targetGame = allGames.FirstOrDefault(g => StringNormalizationHelper.Normalize(g.Gamename) == normalizedGn);
                                }
                                if (targetGame == null)
                                {
                                    targetGame = new Game { Gamename = gn };
                                    _context.Games.Add(targetGame);
                                }
                                gameCache[gn] = targetGame;
                            }

                            _context.Aiextractedfields.Add(new Aiextractedfield
                            {
                                Analysisid = analysis.Analysisid,
                                Rawtext = gn.Length > 500 ? string.Concat(gn.AsSpan(0, 497), "...") : gn,
                                Fieldtype = "GameName",
                                Confidence = 0.98
                            });
                        }
                    }

                    // Find or create Server
                    Server? targetServer = null;
                    if (root.TryGetProperty("server_name", out var serverNameElem) && serverNameElem.ValueKind != JsonValueKind.Null && targetGame != null)
                    {
                        var sn = serverNameElem.GetString();
                        if (!string.IsNullOrEmpty(sn))
                        {
                            if (!serverCache.TryGetValue(sn, out targetServer))
                            {
                                targetServer = await _context.Servers.FirstOrDefaultAsync(s => s.Servername == sn && s.Gameid == targetGame.Gameid);
                                if (targetServer == null)
                                {
                                    var normalizedSn = StringNormalizationHelper.Normalize(sn);
                                    var serversInGame = await _context.Servers.Where(s => s.Gameid == targetGame.Gameid).ToListAsync();
                                    targetServer = serversInGame.FirstOrDefault(s => StringNormalizationHelper.Normalize(s.Servername) == normalizedSn);
                                }
                                if (targetServer == null)
                                {
                                    targetServer = new Server { Servername = sn, Game = targetGame };
                                    _context.Servers.Add(targetServer);
                                }
                                serverCache[sn] = targetServer;
                            }

                            _context.Aiextractedfields.Add(new Aiextractedfield
                            {
                                Analysisid = analysis.Analysisid,
                                Rawtext = sn.Length > 500 ? string.Concat(sn.AsSpan(0, 497), "...") : sn,
                                Fieldtype = "ServerName",
                                Confidence = 0.95
                            });
                        }
                    }

                    // Find or create Event
                    Event? targetEvent = null;
                    if (root.TryGetProperty("event_name", out var eventNameElem) && eventNameElem.ValueKind != JsonValueKind.Null && targetGame != null)
                    {
                        var en = eventNameElem.GetString();
                        if (!string.IsNullOrEmpty(en))
                        {
                            targetEvent = await _context.Events.FirstOrDefaultAsync(e => e.Eventname == en && e.Gameid == targetGame.Gameid);
                            if (targetEvent == null)
                            {
                                var normalizedEn = StringNormalizationHelper.Normalize(en);
                                var eventsInGame = await _context.Events.Where(e => e.Gameid == targetGame.Gameid).ToListAsync();
                                targetEvent = eventsInGame.FirstOrDefault(e => StringNormalizationHelper.Normalize(e.Eventname) == normalizedEn);
                            }
                            if (targetEvent == null)
                            {
                                targetEvent = new Event { Eventname = en, Game = targetGame, Startdate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified) };
                                _context.Events.Add(targetEvent);
                            }

                            _context.Aiextractedfields.Add(new Aiextractedfield
                            {
                                Analysisid = analysis.Analysisid,
                                Rawtext = en.Length > 500 ? string.Concat(en.AsSpan(0, 497), "...") : en,
                                Fieldtype = "EventName",
                                Confidence = 0.95
                            });
                        }
                    }

                    // Create Leaderboard
                    var lb = new Leaderboard
                    {
                        Title = $"Leaderboard from Analysis #{analysis.Analysisid}",
                        Createdfromanalysisid = analysis.Analysisid,
                        Metrictype = "Score",
                        Event = targetEvent
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

                            // Optional Server/Guild per player
                            Server? playerServer = targetServer;
                            if (item.TryGetProperty("server_name", out var pSnElem) && pSnElem.ValueKind != JsonValueKind.Null && targetGame != null)
                            {
                                var psn = pSnElem.GetString();
                                if (!string.IsNullOrEmpty(psn))
                                {
                                    if (!serverCache.TryGetValue(psn, out playerServer))
                                    {
                                        playerServer = await _context.Servers.FirstOrDefaultAsync(s => s.Servername == psn && s.Gameid == targetGame.Gameid);
                                        if (playerServer == null)
                                        {
                                            var normalizedPsn = StringNormalizationHelper.Normalize(psn);
                                            var serversInGame = await _context.Servers.Where(s => s.Gameid == targetGame.Gameid).ToListAsync();
                                            playerServer = serversInGame.FirstOrDefault(s => StringNormalizationHelper.Normalize(s.Servername) == normalizedPsn);
                                        }
                                        if (playerServer == null)
                                        {
                                            playerServer = new Server { Servername = psn, Game = targetGame };
                                            _context.Servers.Add(playerServer);
                                        }
                                        serverCache[psn] = playerServer;
                                    }
                                }
                            }

                            Guild? playerGuild = null;
                            var guildNameProperty = item.TryGetProperty("guild_name", out var gElem) ? gElem 
                                                    : root.TryGetProperty("guild_name", out var globalGuildElem) ? globalGuildElem 
                                                    : (JsonElement?)null;

                            if (guildNameProperty.HasValue && guildNameProperty.Value.ValueKind != JsonValueKind.Null && playerServer != null)
                            {
                                var gn = guildNameProperty.Value.GetString();
                                if (!string.IsNullOrEmpty(gn))
                                {
                                    if (!guildCache.TryGetValue(gn, out playerGuild))
                                    {
                                        playerGuild = await _context.Guilds.FirstOrDefaultAsync(g => g.Guildname == gn && g.Serverid == playerServer.Serverid);
                                        if (playerGuild == null)
                                        {
                                            var normalizedGn = StringNormalizationHelper.Normalize(gn);
                                            var guildsInServer = await _context.Guilds.Where(g => g.Serverid == playerServer.Serverid).ToListAsync();
                                            playerGuild = guildsInServer.FirstOrDefault(g => StringNormalizationHelper.Normalize(g.Guildname) == normalizedGn);
                                        }
                                        if (playerGuild == null)
                                        {
                                            playerGuild = new Guild { Guildname = gn, Server = playerServer };
                                            _context.Guilds.Add(playerGuild);
                                        }
                                        guildCache[gn] = playerGuild;
                                    }
                                }
                            }
                            
                            // Find or create player
                            Player? player = null;
                            if (!string.IsNullOrEmpty(pName) && pName != "Unknown" && targetGame != null)
                            {
                                if (!playerCache.TryGetValue(pName, out player))
                                {
                                    player = await _context.Players.FirstOrDefaultAsync(p => p.Playername == pName && p.Gameid == targetGame.Gameid);
                                    if (player == null)
                                    {
                                        var normalizedPName = StringNormalizationHelper.Normalize(pName);
                                        var playersInGame = await _context.Players.Where(p => p.Gameid == targetGame.Gameid).ToListAsync();
                                        player = playersInGame.FirstOrDefault(p => StringNormalizationHelper.Normalize(p.Playername) == normalizedPName);
                                    }
                                    if (player == null)
                                    {
                                        player = new Player 
                                        { 
                                            Playername = pName,
                                            Game = targetGame,
                                            Server = playerServer,
                                            Guild = playerGuild
                                        };
                                        _context.Players.Add(player);
                                    }
                                    else 
                                    {
                                        // Update player's server/guild if they are now known
                                        if (player.Serverid == null && playerServer != null) player.Server = playerServer;
                                        if (player.Guildid == null && playerGuild != null) player.Guild = playerGuild;
                                    }
                                    playerCache[pName] = player;
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

            var promptText = "Extract game name, server name, guild name, event name, player names, ranks, and scores from this leaderboard screenshot. " +
                "IMPORTANT: Look for the column labeled 'Bang Hội' or 'Guild' and extract the guild name for each player entry. " +
                "Return as JSON with structure: { 'game_name': '...', 'server_name': '...', 'guild_name': '...', 'event_name': '...', " +
                "'leaderboard': [ { 'rank': 1, 'player_name': '...', 'score': 100, 'guild_name': '...', 'server_name': '...' } ] }. " +
                "If a player has a guild name (like 'ĐẠI-VIỆT' or 'TAE_TụNghĩa' in the image), make sure to include it in their entry. " +
                "Note: In Vietnamese game UI, 'Hạng' is rank, 'Tên' is player name, 'Bang Hội' is guild name, 'Lực Chiến' or 'Điểm' is score.";

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
                            new { type = "text", text = promptText },
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
