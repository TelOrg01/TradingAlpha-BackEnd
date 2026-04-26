using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingAlpha.Application.Interfaces;
using TradingAlpha.Domain.Common;

namespace TradingAlpha.Api.Controllers;

/// <summary>
/// Subscription status endpoints.
/// Used by: Angular profile page (show plan status) and later the WPF desktop app (license check).
/// 
/// GET /api/subscription/current → returns plan details, isActive, daysRemaining
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    /// <summary>
    /// Gets the current subscription status for the logged-in user.
    /// Returns IsActive=false with PlanName="None" if no active subscription exists
    /// (not an error — the profile page always renders, just shows different content).
    /// 
    /// GET /api/subscription/current
    /// Response: { planId, planName, status, isActive, daysRemaining, expiryDate, startDate }
    /// </summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentSubscription()
    {
        var userId = GetUserIdFromClaims();
        if (userId is null)
        {
            return Unauthorized(new { status = 401, message = "Invalid token: user ID not found" });
        }

        var result = await _subscriptionService.GetCurrentSubscriptionAsync(userId.Value);
        return ToActionResult(result);
    }

    /// <summary>
    /// Extracts the user's GUID from the JWT "sub" claim.
    /// </summary>
    private Guid? GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }

    /// <summary>
    /// Maps Result&lt;T&gt; to the appropriate HTTP response.
    /// </summary>
    private IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return StatusCode(result.StatusCode, new
        {
            status = result.StatusCode,
            message = result.Error
        });
    }
}