using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingAlpha.Application.Interfaces;
using TradingAlpha.Contracts.Payment;
using TradingAlpha.Domain.Common;

namespace TradingAlpha.Api.Controllers;

/// <summary>
/// Handles Razorpay payment lifecycle: create order and verify payment.
/// Both endpoints require authentication — only logged-in users can pay.
/// 
/// Follows the same thin-controller pattern as AuthController:
/// - Extracts userId from JWT claims
/// - Delegates to PaymentService
/// - Maps Result&lt;T&gt; to HTTP status codes via ToActionResult
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Creates a Razorpay order for the specified plan.
    /// Returns the order details needed to open the Razorpay checkout modal.
    /// 
    /// POST /api/payment/create-order
    /// Request:  { "planId": "pro" }
    /// Response: { orderId, amountInPaise, currency, gatewayKeyId, planName, userName, userEmail }
    /// </summary>
    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId is null)
        {
            return Unauthorized(new { status = 401, message = "Invalid token: user ID not found" });
        }

        var result = await _paymentService.CreateOrderAsync(request.PlanId, userId.Value);
        return ToActionResult(result);
    }

    /// <summary>
    /// Verifies a completed Razorpay payment and activates the subscription.
    /// Called by the frontend after the Razorpay checkout modal returns successfully.
    /// 
    /// POST /api/payment/verify
    /// Request:  { "orderId": "order_...", "paymentId": "pay_...", "signature": "..." }
    /// Response: { success, subscriptionId, planName, expiryDate, message }
    /// </summary>
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId is null)
        {
            return Unauthorized(new { status = 401, message = "Invalid token: user ID not found" });
        }

        var result = await _paymentService.VerifyPaymentAsync(request, userId.Value);
        return ToActionResult(result);
    }

    /// <summary>
    /// Extracts the user's GUID from the JWT "sub" claim.
    /// Same approach as AuthController.GetProfile().
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
    /// Same helper pattern used in AuthController.
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