using TradingAlpha.Contracts.Auth;
using TradingAlpha.Domain.Common;

namespace TradingAlpha.Application.Interfaces;

/// <summary>
/// Auth service contract.
/// 
/// Returns Result<T> instead of throwing exceptions for
/// expected failures (wrong password, duplicate username, etc.).
/// The controller checks Result.IsSuccess and returns
/// the appropriate HTTP status code.
/// </summary>
public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Result<UserProfileResponse>> GetProfileAsync(Guid userId, CancellationToken ct = default);
}