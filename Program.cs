using BIL.Service;
using DAL.Entities;
using DAL.Repository;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddHttpClient<IAIAnalysisService, AIAnalysisService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["Mistral:ApiKey"];

    client.BaseAddress = new Uri("https://api.mistral.ai/");
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", apiKey);
});

builder.Services.AddHttpClient<IAIAnalysisRepository, AIAnalysisRepository>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(10);
});
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PostgresContext>();
builder.Services.AddScoped<IAIAnalysisRepository, AIAnalysisRepository>();
builder.Services.AddScoped<IAIAnalysisService, AIAnalysisService>();

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameService, GameService>();

var app = builder.Build();

if (app.Environment.IsDevelopment()||true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
