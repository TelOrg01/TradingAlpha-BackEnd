using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TradingAlpha.Application.Interfaces;
using TradingAlpha.Application.Services;

namespace TradingAlpha.Application;

/// <summary>
/// Registers Application layer services into the DI container.
/// 
/// Called from Program.cs as: builder.Services.AddApplication();
/// Keeps DI registration close to the layer it belongs to — 
/// each layer is responsible for registering its own services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register auth business logic
        services.AddScoped<IAuthService, AuthService>();

        // Register all FluentValidation validators from this assembly
        services.AddValidatorsFromAssemblyContaining<IAuthService>();

        services.AddScoped<IPaymentService, PaymentService>();

        services.AddScoped<ISubscriptionService, SubscriptionService>();

        return services;
    }
}