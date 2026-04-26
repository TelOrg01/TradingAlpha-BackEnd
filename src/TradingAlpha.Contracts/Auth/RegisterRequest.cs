namespace TradingAlpha.Contracts.Auth;

/// <summary>
/// DTO received from the Angular frontend for user registration.
/// Validated by RegisterRequestValidator before reaching AuthService.
/// </summary>
public record RegisterRequest(
    string Name,
    string Username,
    string Password,
    string ConfirmPassword
);