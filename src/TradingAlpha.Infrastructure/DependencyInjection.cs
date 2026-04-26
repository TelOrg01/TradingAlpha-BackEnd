using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TradingAlpha.Application.Interfaces;
using TradingAlpha.Domain.Interfaces;
using TradingAlpha.Infrastructure.Authentication;
using TradingAlpha.Infrastructure.Data;
using TradingAlpha.Infrastructure.ExternalServices;
using TradingAlpha.Infrastructure.Repositories;
using TradingAlpha.Infrastructure.Services;

namespace TradingAlpha.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── 1. PostgreSQL + EF Core ──
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(3)
            ));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // ── 2. Repositories ──
        services.AddScoped<IUserRepository, UserRepository>();

        // ── 3. JWT Configuration ──
        // Read settings from appsettings.json "JwtSettings" section
        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        var jwtSettings = jwtSection.Get<JwtSettings>();

        // Fail fast with a clear message if config is missing
        if (jwtSettings is null || string.IsNullOrEmpty(jwtSettings.Secret))
        {
            throw new InvalidOperationException(
                "JwtSettings is missing or empty in appsettings.json. " +
                "Ensure the 'JwtSettings' section exists with a 'Secret' key " +
                "that is at least 32 characters long.");
        }

        // Register JwtSettings for IOptions<JwtSettings> injection
        services.Configure<JwtSettings>(jwtSection);

        // ── 4. Token Generator ──
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // ── 5. Password Hasher ──
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // ── 6. JWT Bearer Authentication ──
        services.AddAuthentication(options =>
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
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // Bind RazorpaySettings from appsettings.json (same pattern as JwtSettings)
        services.Configure<RazorpaySettings>(
            configuration.GetSection(RazorpaySettings.SectionName));

        // Register the Razorpay gateway wrapper
        services.AddScoped<IRazorpayService, RazorpayService>();

        return services;
    }
}