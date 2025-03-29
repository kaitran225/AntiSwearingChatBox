using System.Text;
using System.IO;
using AntiSwearingChatBox.AI.Services;
using AntiSwearingChatBox.AI.Interfaces;
using AntiSwearingChatBox.Server.Hubs;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AntiSwearingChatBox.Service.Services;

// Helper method to find the Service project directory
static string FindServiceProjectDirectory()
{
    // Start from current directory
    string? currentDir = Directory.GetCurrentDirectory();
    
    // Try to find solution root by traversing up
    while (currentDir != null && !File.Exists(Path.Combine(currentDir, "AntiSwearingChatBox.sln")))
    {
        currentDir = Directory.GetParent(currentDir)?.FullName;
    }
    
    // If found solution root, look for Service project
    if (currentDir != null)
    {
        string serviceDir = Path.Combine(currentDir, "AntiSwearingChatBox.Service");
        if (Directory.Exists(serviceDir))
        {
            return serviceDir;
        }
    }
    
    // Fallback to current directory
    return Directory.GetCurrentDirectory();
}

// Find the path to the Service project's appsettings.json
string serviceDirectory = FindServiceProjectDirectory();

// Create a new WebApplicationBuilder with configuration from Service project
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ApplicationName = typeof(Program).Assembly.GetName().Name,
    ContentRootPath = Directory.GetCurrentDirectory(),
    Args = args
});

// Add Service project's appsettings.json as first configuration source
builder.Configuration.AddJsonFile(Path.Combine(serviceDirectory, "appsettings.json"), optional: true, reloadOnChange: true);

// Then add local appsettings.json files for any environment-specific overrides
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add SignalR services
builder.Services.AddSignalR();

// Register profanity filter service
builder.Services.AddSingleton<IProfanityFilter, ProfanityFilterService>();

// Configure CORS to allow connections from any origin (for local network testing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});

// Configure JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings?.SecretKey ?? throw new InvalidOperationException("JWT SecretKey is not configured"))),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

// Map the ChatHub
app.MapHub<ChatHub>("/chatHub");

// Keep the sample weather endpoint for testing purposes
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Add a simple endpoint to check if the server is running
app.MapGet("/", () => "Chat Server is running!");

app.UseAuthentication();
app.UseAuthorization();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
