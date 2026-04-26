using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TradingAlpha.Application.Interfaces;
using TradingAlpha.Contracts.Auth;
using TradingAlpha.Domain.Common;

namespace TradingAlpha.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var validation = await _registerValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Validation failed.",
                errors = validation.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage
                })
            });
        }

        var result = await _authService.RegisterAsync(request, ct);
        return ToActionResult(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var validation = await _loginValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Validation failed.",
                errors = validation.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage
                })
            });
        }

        var result = await _authService.LoginAsync(request, ct);
        return ToActionResult(result);
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("sub")
                       ?? User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { status = 401, message = "Invalid token." });
        }

        var result = await _authService.GetProfileAsync(userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Converts Result<T> to proper HTTP response.
    /// 
    /// Success → 200 OK with result.Value (the actual data, NOT the wrapper)
    /// Failure → appropriate status code with error message
    /// </summary>
    private IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);   // ← .Value unwraps the Result<T>
        }

        return StatusCode(result.StatusCode, new
        {
            status = result.StatusCode,
            message = result.Error
        });
    }
}