namespace TradingAlpha.Contracts.Auth;

/// <summary>
/// DTO returned for GET /api/auth/profile.
/// Used by the Angular ProfileComponent to display user details.
/// Intentionally excludes sensitive fields like PasswordHash.
/// </summary>
public record UserProfileResponse(
    Guid Id,
    string Username,
    string Name,
    string Role,
    string MemberSince
);