namespace TradingAlpha.Infrastructure.Authentication;

/// <summary>
/// POCO class that maps to the "JwtSettings" section in appsettings.json.
/// 
/// Bound via IOptions<JwtSettings> pattern — values are read from
/// configuration at startup, not hardcoded.
/// 
/// SECURITY: The Secret key must be at least 32 characters (256 bits)
/// for HMAC-SHA256 signing. In production, store it in environment
/// variables or a secret manager, NOT in appsettings.json.
/// </summary>
public class JwtSettings
{
    /// <summary>Configuration section name in appsettings.json</summary>
    public const string SectionName = "JwtSettings";

    /// <summary>HMAC-SHA256 signing key — min 32 characters</summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>Token issuer (typically your API URL)</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Token audience (typically your frontend URL)</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>Token expiry in minutes</summary>
    public int ExpiryMinutes { get; set; } = 60;
}