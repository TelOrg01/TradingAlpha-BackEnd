namespace TradingAlpha.Contracts.Auth;

/// <summary>
/// DTO received from the Angular frontend for user login.
/// Validated by LoginRequestValidator before reaching AuthService.
/// </summary>
public record LoginRequest(
    string Username,
    string Password
);