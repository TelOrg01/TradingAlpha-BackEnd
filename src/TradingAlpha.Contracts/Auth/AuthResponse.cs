namespace TradingAlpha.Contracts.Auth;

/// <summary>
/// DTO returned to the Angular frontend after successful login/register.
/// Contains the JWT token that the frontend stores and sends in
/// the Authorization header for subsequent API calls.
/// </summary>
public record AuthResponse(
	string Token,
	string Username,
	string Name,
	string Role,
	DateTime ExpiresAt
);