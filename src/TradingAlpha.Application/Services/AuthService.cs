using TradingAlpha.Application.Interfaces;
using TradingAlpha.Contracts.Auth;
using TradingAlpha.Domain.Common;
using TradingAlpha.Domain.Entities;
using TradingAlpha.Domain.Interfaces;

namespace TradingAlpha.Application.Services;

/// <summary>
/// AuthService — Core authentication business logic.
/// 
/// Uses Result pattern instead of exceptions for expected failures.
/// This means:
///   - Debugger won't break on wrong password
///   - No expensive stack trace capture for business errors
///   - Controller explicitly handles success/failure paths
///   - Unexpected exceptions (DB down, etc.) still throw and get
///     caught by ExceptionHandlingMiddleware
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtGenerator;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IUserRepository userRepo,
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtGenerator,
        IPasswordHasher passwordHasher)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
        _jwtGenerator = jwtGenerator;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request, CancellationToken ct = default)
    {
        // 1. Check if username already exists
        var username = request.Username.Trim().ToLower();
        var exists = await _userRepo.ExistsByUsernameAsync(username, ct);
        if (exists)
        {
            return Result<AuthResponse>.Failure("Username is already taken.");
        }

        // 2. Create the user entity with hashed password
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Name = request.Name.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Persist to database
        await _userRepo.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // 4. Generate JWT and return success
        var (token, expiresAt) = _jwtGenerator.GenerateToken(user);

        var response = new AuthResponse(
            Token: token,
            Username: user.Username,
            Name: user.Name,
            Role: user.Role.ToString(),
            ExpiresAt: expiresAt
        );

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request, CancellationToken ct = default)
    {
        // 1. Find user by username
        var user = await _userRepo.GetByUsernameAsync(
            request.Username.Trim().ToLower(), ct);

        if (user is null)
        {
            // Don't reveal whether username exists or not (security)
            return Result<AuthResponse>.Failure("Invalid username or password.");
        }

        // 2. Verify password against stored hash
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure("Invalid username or password.");
        }

        // 3. Generate JWT and return success
        var (token, expiresAt) = _jwtGenerator.GenerateToken(user);

        var response = new AuthResponse(
            Token: token,
            Username: user.Username,
            Name: user.Name,
            Role: user.Role.ToString(),
            ExpiresAt: expiresAt
        );

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result<UserProfileResponse>> GetProfileAsync(
        Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user is null)
        {
            return Result<UserProfileResponse>.NotFound("User not found.");
        }

        var response = new UserProfileResponse(
            Id: user.Id,
            Username: user.Username,
            Name: user.Name,
            Role: user.Role.ToString(),
            MemberSince: user.CreatedAt.ToString("MMMM yyyy")
        );

        return Result<UserProfileResponse>.Success(response);
    }
}