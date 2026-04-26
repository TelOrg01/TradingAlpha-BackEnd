using Microsoft.Extensions.Logging;
using TradingAlpha.Application.Common;
using TradingAlpha.Application.Interfaces;
using TradingAlpha.Contracts.Payment;
using TradingAlpha.Domain.Common;
using TradingAlpha.Domain.Entities;
using TradingAlpha.Domain.Interfaces;

namespace TradingAlpha.Application.Services;

/// <summary>
/// Orchestrates the payment lifecycle: create order → verify payment → activate subscription.
/// Follows the same patterns as AuthService:
/// - Uses Result&lt;T&gt; for expected failures (invalid plan, failed verification)
/// - Uses interfaces for all dependencies (testable, gateway-swappable)
/// - Single responsibility methods with clear steps
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IRazorpayService _razorpayService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IRazorpayService razorpayService,
        IPaymentRepository paymentRepository,
        ISubscriptionRepository subscriptionRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<PaymentService> logger)
    {
        _razorpayService = razorpayService;
        _paymentRepository = paymentRepository;
        _subscriptionRepository = subscriptionRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Step 1 of payment flow: Create a Razorpay order.
    /// 
    /// Flow: Validate planId → Lookup user → Resolve price server-side →
    ///       Call Razorpay API → Store Pending payment record → Return order details.
    /// </summary>
    public async Task<Result<CreateOrderResponse>> CreateOrderAsync(string planId, Guid userId)
    {
        // 1. Validate plan exists in our catalog
        var plan = PlanCatalog.GetPlan(planId);
        if (plan is null)
        {
            return Result<CreateOrderResponse>.Failure($"Invalid plan: '{planId}'");
        }

        // 2. Lookup user for prefill data (name for checkout modal)
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return Result<CreateOrderResponse>.NotFound("User not found");
        }

        // 3. Check if user already has an active subscription
        var existingSubscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId);
        if (existingSubscription is not null)
        {
            var existingPlan = PlanCatalog.GetPlan(existingSubscription.PlanId);

            // Block same plan purchase
            if (existingSubscription.PlanId.Equals(planId, StringComparison.OrdinalIgnoreCase))
            {
                return Result<CreateOrderResponse>.Failure(
                    $"You already have an active {existingSubscription.PlanName} subscription. " +
                    $"It expires on {existingSubscription.ExpiryDate:yyyy-MM-dd}.");
            }

            // Block lower tier purchase (e.g., user has Pro, tries to buy Starter)
            if (existingPlan is not null && plan.DurationInDays <= existingPlan.DurationInDays
                && plan.PriceInRupees <= existingPlan.PriceInRupees)
            {
                return Result<CreateOrderResponse>.Failure(
                    $"You already have a {existingSubscription.PlanName} plan which is equal or higher. " +
                    $"Downgrades are not supported.");
            }

            // Block if lifetime plan — no upgrades possible
            if (existingSubscription.PlanId.Equals("lifetime", StringComparison.OrdinalIgnoreCase))
            {
                return Result<CreateOrderResponse>.Failure(
                    "You already have a Lifetime subscription. No upgrades needed!");
            }
        }

        // 4. Create Razorpay order — price comes from PlanCatalog, NOT from the client
        var receipt = $"rcpt_{Guid.NewGuid():N}";
        var (orderId, amountInPaise) = await _razorpayService.CreateOrderAsync(
            plan.PriceInRupees,
            "INR",
            receipt);

        // 5. Store a Pending payment record (audit trail for every attempt)
        var paymentRecord = new PaymentRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = plan.PriceInRupees,
            Currency = "INR",
            Status = PaymentStatus.Pending,
            GatewayOrderId = orderId,
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(paymentRecord);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Order created: UserId={UserId}, PlanId={PlanId}, OrderId={OrderId}, Amount=₹{Amount}",
            userId, planId, orderId, plan.PriceInRupees);

        // 6. Return everything the frontend needs to open the checkout modal
        return Result<CreateOrderResponse>.Success(new CreateOrderResponse(
            OrderId: orderId,
            AmountInPaise: amountInPaise,
            Currency: "INR",
            GatewayKeyId: _razorpayService.GetKeyId(),
            PlanName: plan.Name,
            UserName: user.Name,
            UserEmail: string.Empty   // User entity doesn't have Email — Razorpay prefill is optional
        ));
    }

    /// <summary>
    /// Step 2 of payment flow: Verify payment and activate subscription.
    /// 
    /// Flow: Find Pending payment → Verify HMAC signature → Create Subscription →
    ///       Update payment record to Success → Return confirmation.
    /// 
    /// All DB operations happen in a single UnitOfWork transaction.
    /// If any step fails, nothing is committed.
    /// </summary>
    public async Task<Result<VerifyPaymentResponse>> VerifyPaymentAsync(
        VerifyPaymentRequest request,
        Guid userId)
    {
        // 1. Find the Pending payment record created during CreateOrderAsync
        var paymentRecord = await _paymentRepository.GetByOrderIdAsync(request.OrderId);
        if (paymentRecord is null)
        {
            return Result<VerifyPaymentResponse>.NotFound(
                $"No payment found for order: {request.OrderId}");
        }

        // 2. Ensure this payment belongs to the requesting user (prevent cross-user attacks)
        if (paymentRecord.UserId != userId)
        {
            _logger.LogWarning(
                "Payment ownership mismatch: OrderId={OrderId}, RecordUserId={RecordUser}, RequestUserId={RequestUser}",
                request.OrderId, paymentRecord.UserId, userId);

            return Result<VerifyPaymentResponse>.Failure("Payment does not belong to this user");
        }

        // 3. Prevent double-verification (idempotency)
        if (paymentRecord.Status == PaymentStatus.Success)
        {
            _logger.LogInformation(
                "Duplicate verification attempt: OrderId={OrderId}", request.OrderId);

            // Find the existing subscription and return it
            var existingSub = await _subscriptionRepository.GetByOrderIdAsync(request.OrderId);
            if (existingSub is not null)
            {
                return Result<VerifyPaymentResponse>.Success(new VerifyPaymentResponse(
                    Success: true,
                    SubscriptionId: existingSub.Id,
                    PlanName: existingSub.PlanName,
                    ExpiryDate: existingSub.ExpiryDate,
                    Message: "Payment was already verified"
                ));
            }
        }

        // 4. Verify HMAC-SHA256 signature — the critical security check
        var isValid = _razorpayService.VerifyPaymentSignature(
            request.OrderId,
            request.PaymentId,
            request.Signature);

        if (!isValid)
        {
            // Mark payment as Failed — signature mismatch means possible tampering
            paymentRecord.Status = PaymentStatus.Failed;
            paymentRecord.GatewayPaymentId = request.PaymentId;
            paymentRecord.GatewaySignature = request.Signature;
            await _unitOfWork.SaveChangesAsync();

            _logger.LogWarning(
                "Signature verification failed: OrderId={OrderId}, PaymentId={PaymentId}",
                request.OrderId, request.PaymentId);

            return Result<VerifyPaymentResponse>.Failure("Payment verification failed");
        }

        // 5. Resolve the plan to get duration (price was already locked at order time)
        var plan = PlanCatalog.GetPlan(GetPlanIdFromAmount(paymentRecord.Amount));
        if (plan is null)
        {
            _logger.LogError(
                "Could not resolve plan for amount: {Amount}", paymentRecord.Amount);
            return Result<VerifyPaymentResponse>.Failure("Could not determine subscription plan");
        }

        // 6. Create the Subscription record
        var now = DateTime.UtcNow;
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = plan.Id,
            PlanName = plan.Name,
            Amount = paymentRecord.Amount,
            Currency = paymentRecord.Currency,
            Status = SubscriptionStatus.Active,
            PaymentId = request.PaymentId,
            OrderId = request.OrderId,
            StartDate = now,
            ExpiryDate = now.AddDays(plan.DurationInDays),
            CreatedAt = now
        };

        await _subscriptionRepository.AddAsync(subscription);

        // 7. Update the payment record: Pending → Success, link to subscription
        paymentRecord.Status = PaymentStatus.Success;
        paymentRecord.GatewayPaymentId = request.PaymentId;
        paymentRecord.GatewaySignature = request.Signature;
        paymentRecord.SubscriptionId = subscription.Id;
        paymentRecord.RawResponse = System.Text.Json.JsonSerializer.Serialize(new
        {
            request.OrderId,
            request.PaymentId,
            request.Signature,
            VerifiedAt = now
        });

        // 8. Single SaveChanges — both Subscription insert and PaymentRecord update
        //    are committed atomically. If anything fails, both roll back.
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Payment verified and subscription activated: UserId={UserId}, PlanId={PlanId}, " +
            "SubscriptionId={SubId}, ExpiryDate={Expiry}",
            userId, plan.Id, subscription.Id, subscription.ExpiryDate);

        return Result<VerifyPaymentResponse>.Success(new VerifyPaymentResponse(
            Success: true,
            SubscriptionId: subscription.Id,
            PlanName: plan.Name,
            ExpiryDate: subscription.ExpiryDate,
            Message: "Subscription activated successfully"
        ));
    }

    /// <summary>
    /// Resolves a plan ID from the payment amount.
    /// Used during verification since we stored the amount at order time
    /// but need the plan details (duration) to create the subscription.
    /// </summary>
    private static string GetPlanIdFromAmount(decimal amount)
    {
        return amount switch
        {
            4m => "starter",
            5m => "pro",
            6m => "lifetime",
            _ => string.Empty
        };
    }
}