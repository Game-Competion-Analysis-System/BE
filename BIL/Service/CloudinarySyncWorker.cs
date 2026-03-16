using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DAL.DTO;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BIL.Service
{
    public class CloudinarySyncWorker(IServiceProvider serviceProvider, ILogger<CloudinarySyncWorker> logger, IConfiguration config) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<CloudinarySyncWorker> _logger = logger;
        private readonly IConfiguration _config = config;
        private readonly Cloudinary _cloudinary = new(new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            ));
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
        private const int INTERVAL_SECONDS = 60; // 1 minute

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CloudinarySyncWorker started. Checking for new images every {Interval}s", INTERVAL_SECONDS);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<Swd392GameAiContext>();
                    var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();

                    // 1. Fetch Latest Image from Cloudinary - Worker uses "AirtestUpload" folder
                    string? imageUrl = null;
                    string? publicId = null;

                    var searchResult = await _cloudinary.Search()
                        .Expression("folder:\"AirtestUpload\" AND resource_type:image")
                        .SortBy("created_at", "desc")
                        .MaxResults(1)
                        .ExecuteAsync();

                    var searchLatest = searchResult.Resources?.FirstOrDefault();
                    if (searchLatest != null)
                    {
                        imageUrl = searchLatest.SecureUrl?.ToString();
                        publicId = searchLatest.PublicId;
                    }
                    else
                    {
                        var listParams = new ListResourcesByPrefixParams
                        {
                            Type = "upload",
                            Prefix = "AirtestUpload/",
                            MaxResults = 1,
                            Direction = "desc"
                        };
                        var listResources = await _cloudinary.ListResourcesAsync(listParams);
                        var listLatest = listResources.Resources?.FirstOrDefault();
                        if (listLatest != null)
                        {
                            imageUrl = listLatest.SecureUrl?.ToString();
                            publicId = listLatest.PublicId;
                        }
                    }

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        // 2. Check if already processed
                        var exists = await dbContext.Imageuploads.AnyAsync(u => u.Imageurl == imageUrl, stoppingToken);
                        if (!exists)
                        {
                            _logger.LogInformation("Found new image on Cloudinary: {PublicId}. Starting analysis...", publicId);
                            
                            // 3. Process the new image
                            await ProcessNewImageAsync(dbContext, httpClient, imageUrl);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in CloudinarySyncWorker");
                }

                await Task.Delay(TimeSpan.FromSeconds(INTERVAL_SECONDS), stoppingToken);
            }
        }

        private async Task ProcessNewImageAsync(Swd392GameAiContext context, HttpClient http, string imageUrl)
        {
            int systemUserId = 1; // System account

            // Step 1: Create ImageUpload
            var upload = new Imageupload
            {
                Userid = systemUserId,
                Imageurl = imageUrl,
                Uploadtime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                Status = "Pending"
            };
            context.Imageuploads.Add(upload);
            await context.SaveChangesAsync();

            try
            {
                // Step 2: Call AI OCR
                var ocr = await CallGroqOcrWithUrl(http, imageUrl);

                // Step 3: Create AIAnalysis
                var analysis = new Aianalysis
                {
                    Uploadid = upload.Uploadid,
                    Aimodelversion = "meta-llama/llama-4-scout-17b-16e-instruct",
                    Confidencescore = ocr.Confidence,
                    Processedtime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
                };
                context.Aianalyses.Add(analysis);
                await context.SaveChangesAsync();

                // Step 4: Parse AI Result
                var content = ocr.Choices?.FirstOrDefault()?.Message?.Content;
                if (string.IsNullOrEmpty(content)) throw new Exception("AI returned empty content");

                string jsonContent = content;
                var startIdx = content.IndexOf('{');
                var endIdx = content.LastIndexOf('}');
                if (startIdx != -1 && endIdx != -1 && endIdx > startIdx)
                    jsonContent = content.Substring(startIdx, endIdx - startIdx + 1);

                using var data = JsonDocument.Parse(jsonContent, new JsonDocumentOptions { AllowTrailingCommas = true });
                var root = data.RootElement;

                // Step 5: Extract Game, Server, Event
                string? gameName = null;
                if (root.TryGetProperty("game_name", out var gElem)) gameName = gElem.GetString();
                
                var targetGame = await context.Games.FirstOrDefaultAsync(g => g.Gamename == gameName);
                if (targetGame != null)
                    context.Aiextractedfields.Add(new Aiextractedfield { Analysisid = analysis.Analysisid, Rawtext = gameName, Fieldtype = "GameName", Confidence = 1.0 });

                Server? targetServer = null;
                string? sn = null;
                if (root.TryGetProperty("server_name", out var sElem)) sn = sElem.GetString();
                if (!string.IsNullOrEmpty(sn))
                {
                    targetServer = await context.Servers.FirstOrDefaultAsync(s => s.Servername == sn && (targetGame == null || s.Gameid == targetGame.Gameid));
                    if (targetServer == null)
                    {
                        targetServer = new Server { Servername = sn, Game = targetGame };
                        context.Servers.Add(targetServer);
                        await context.SaveChangesAsync();
                    }
                    context.Aiextractedfields.Add(new Aiextractedfield { Analysisid = analysis.Analysisid, Rawtext = sn, Fieldtype = "ServerName", Confidence = 0.95 });
                }

                string? en = null;
                if (root.TryGetProperty("event_name", out var eElem)) en = eElem.GetString();
                Event? targetEvent = await context.Events.FirstOrDefaultAsync(e => e.Eventname == en && (targetGame == null || e.Gameid == targetGame.Gameid));
                if (targetEvent == null && !string.IsNullOrEmpty(en))
                {
                    targetEvent = new Event { Eventname = en, Game = targetGame, Startdate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified) };
                    context.Events.Add(targetEvent);
                    await context.SaveChangesAsync();
                    context.Aiextractedfields.Add(new Aiextractedfield { Analysisid = analysis.Analysisid, Rawtext = en, Fieldtype = "EventName", Confidence = 0.95 });
                }

                // Step 6: Create Leaderboard
                var lb = new Leaderboard
                {
                    Title = $"Bảng xếp hạng {en ?? targetEvent?.Eventname ?? "mới"}",
                    Createdfromanalysisid = analysis.Analysisid,
                    Metrictype = "Score",
                    Event = targetEvent
                };
                context.Leaderboards.Add(lb);
                await context.SaveChangesAsync();

                // Step 7: Process Entries
                if (root.TryGetProperty("leaderboard", out var lbArray) && lbArray.ValueKind == JsonValueKind.Array)
                {
                    Dictionary<string, Guild> guildCache = [];
                    Dictionary<string, Player> playerCache = [];

                    foreach (var item in lbArray.EnumerateArray())
                    {
                        int rank = 0;
                        if (item.TryGetProperty("rank", out var rE)) { if (rE.ValueKind == JsonValueKind.Number) rE.TryGetInt32(out rank); else if (int.TryParse(rE.GetString() ?? "0", out var rS)) rank = rS; }
                        
                        string? pName = item.TryGetProperty("player_name", out var pE) ? pE.GetString() : null;
                        double score = 0;
                        if (item.TryGetProperty("score", out var scE)) { if (scE.ValueKind == JsonValueKind.Number) scE.TryGetDouble(out score); else if (double.TryParse(scE.GetString() ?? "0", out var scS)) score = scS; }
                        
                        string? guildNameStr = item.TryGetProperty("guild_name", out var gNE) ? gNE.GetString() : null;

                        if (string.IsNullOrEmpty(pName)) continue;

                        Guild? playerGuild = null;
                        if (!string.IsNullOrEmpty(guildNameStr))
                        {
                            if (!guildCache.TryGetValue(guildNameStr, out playerGuild))
                            {
                                playerGuild = await context.Guilds.FirstOrDefaultAsync(g => g.Guildname == guildNameStr && (targetServer == null || g.Serverid == targetServer.Serverid));
                                if (playerGuild == null)
                                {
                                    playerGuild = new Guild { Guildname = guildNameStr, Server = targetServer };
                                    context.Guilds.Add(playerGuild);
                                    await context.SaveChangesAsync();
                                }
                                guildCache[guildNameStr] = playerGuild;
                            }
                        }

                        if (!playerCache.TryGetValue(pName, out var player))
                        {
                            player = await context.Players.FirstOrDefaultAsync(p => p.Playername == pName && (targetGame == null || p.Gameid == targetGame.Gameid));
                            if (player == null)
                            {
                                player = new Player { Playername = pName, Game = targetGame, Server = targetServer, Guild = playerGuild };
                                context.Players.Add(player);
                                await context.SaveChangesAsync();
                            }
                            else 
                            {
                                if (player.Guildid == null && playerGuild != null) player.Guildid = playerGuild.Guildid;
                                if (player.Serverid == null && targetServer != null) player.Serverid = targetServer.Serverid;
                            }
                            playerCache[pName] = player;
                        }

                        context.Leaderboardentries.Add(new Leaderboardentry { Leaderboardid = lb.Leaderboardid, Playerid = player.Playerid, Rank = rank, Value = score });
                    }
                }

                await context.SaveChangesAsync();
                upload.Status = "Success";
                await context.SaveChangesAsync();
                _logger.LogInformation("Successfully processed image. Analysis ID: {Id}", analysis.Analysisid);
            }
            catch (Exception ex)
            {
                upload.Status = "Failed";
                await context.SaveChangesAsync();
                _logger.LogError(ex, "Failed to process image: {Url}", imageUrl);
            }
        }

        private async Task<MistralOcrResultDto> CallGroqOcrWithUrl(HttpClient http, string imageUrl)
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
                messages = new object[] { new { role = "user", content = new object[] { new { type = "text", text = promptText }, new { type = "image_url", image_url = new { url = imageUrl } } } } },
                temperature = 0.0,
                response_format = new { type = "json_object" }
            };

            http.DefaultRequestHeaders.Clear();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await http.PostAsJsonAsync("https://api.groq.com/openai/v1/chat/completions", requestBody);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) throw new Exception(raw);
            return JsonSerializer.Deserialize<MistralOcrResultDto>(raw, _jsonOptions) ?? throw new Exception("Failed to parse OCR result");
        }
    }
}
