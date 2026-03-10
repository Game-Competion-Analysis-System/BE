using BIL.Service;
using DAL.Entities;
using DAL.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddHttpClient<IAIAnalysisService, AIAnalysisService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["Groq:ApiKey"];

    client.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Game Competition Analysis API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    []
                }
            });
});

builder.Services.AddDbContext<Swd392GameAiContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAIAnalysisRepository, AIAnalysisRepository>();
builder.Services.AddScoped<IAIAnalysisService, AIAnalysisService>();

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameService, GameService>();

builder.Services.AddScoped<DAL.Repository.ICompanyRepository, DAL.Repository.CompanyRepository>();
builder.Services.AddScoped<BIL.Service.ICompanyService, BIL.Service.CompanyService>();

builder.Services.AddScoped<DAL.Repository.IEventRepository, DAL.Repository.EventRepository>();
builder.Services.AddScoped<BIL.Service.IEventService, BIL.Service.EventService>();

builder.Services.AddScoped<DAL.Repository.IPlayerRepository, DAL.Repository.PlayerRepository>();
builder.Services.AddScoped<BIL.Service.IPlayerService, BIL.Service.PlayerService>();

builder.Services.AddScoped<DAL.Repository.IServerRepository, DAL.Repository.ServerRepository>();
builder.Services.AddScoped<BIL.Service.IServerService, BIL.Service.ServerService>();

builder.Services.AddScoped<DAL.Repository.IGuildRepository, DAL.Repository.GuildRepository>();
builder.Services.AddScoped<BIL.Service.IGuildService, BIL.Service.GuildService>();

builder.Services.AddScoped<DAL.Repository.IUserRepository, DAL.Repository.UserRepository>();
builder.Services.AddScoped<BIL.Service.IUserService, BIL.Service.UserService>();

// Register Auth Service
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Always enable Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Redirect root to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
