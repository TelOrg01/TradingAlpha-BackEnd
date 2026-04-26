using TradingAlpha.Api.Middleware;
using TradingAlpha.Application;
using TradingAlpha.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ══════════════════════════════════════════════════════════════
// SERVICE REGISTRATION
// ══════════════════════════════════════════════════════════════

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

// OpenAPI document generation (required by Scalar)
builder.Services.AddOpenApi();

// CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? new[] { "http://localhost:4200" };

        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ══════════════════════════════════════════════════════════════
// MIDDLEWARE PIPELINE
// ══════════════════════════════════════════════════════════════

// 1. Exception handler
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. Scalar + OpenAPI (dev only)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("TradingAlpha API")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// 3. HTTPS
app.UseHttpsRedirection();

// 4. CORS
app.UseCors("AllowAngular");

// 5. Auth
app.UseAuthentication();
app.UseAuthorization();

// 6. Controllers
app.MapControllers();

app.Run();