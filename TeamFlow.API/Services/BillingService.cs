using Microsoft.EntityFrameworkCore;
using TeamFlow.API.Data;
using TeamFlow.API.DTOs.Billing;
using TeamFlow.API.DTOs.Plans;
using TeamFlow.API.Exceptions;
using TeamFlow.API.Models;
using TeamFlow.API.Models.Enums;
using TeamFlow.API.Services.Interfaces;
using TeamFlow.API.Services.Payments;

namespace TeamFlow.API.Services;

public class BillingService : IBillingService
{
    public const string Currency = "USD";

    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentGateway _paymentGateway;

    public BillingService(
        AppDbContext context,
        ICurrentUserService currentUser,
        IPaymentGateway paymentGateway)
    {
        _context = context;
        _currentUser = currentUser;
        _paymentGateway = paymentGateway;
    }

    public async Task<List<PlanResponseDto>> GetPlansAsync()
    {
        var plans = await _context.Plans
            .AsNoTracking()
            .OrderBy(p => p.Price)
            .ThenBy(p => p.Id)
            .ToListAsync();

        return plans.Select(ToDto).ToList();
    }

    public async Task<SubscriptionResponseDto> GetCurrentSubscriptionAsync()
    {
        var organizationId = _currentUser.OrganizationId;

        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId);

        if (subscription is null)
        {
            throw new NotFoundException("Subscription not found.");
        }

        var usage = await GetUsageAsync(organizationId);

        return new SubscriptionResponseDto
        {
            Plan = ToDto(subscription.Plan),
            Status = subscription.Status.ToString(),
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            Usage = usage
        };
    }

    public async Task<SubscriptionResponseDto> ChangePlanAsync(
        ChangePlanDto dto)
    {
        var organizationId = _currentUser.OrganizationId;

        var plan = await _context.Plans
            .FirstOrDefaultAsync(p => p.Id == dto.PlanId);

        if (plan is null)
        {
            throw new NotFoundException("Plan not found.");
        }

        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s =>
                s.OrganizationId == organizationId &&
                s.Status == SubscriptionStatus.Active);

        if (subscription is null)
        {
            throw new NotFoundException("Active subscription not found.");
        }

        if (subscription.PlanId == plan.Id)
        {
            throw new ConflictException("You are already on this plan.");
        }

        // A downgrade is only allowed when current usage fits the new limits.
        await EnsureUsageFitsAsync(organizationId, plan);

        Payment? payment = null;

        if (plan.Price > 0)
        {
            if (dto.Card is null)
            {
                throw new BadRequestException(
                    "Payment details are required for paid plans.");
            }

            var result = await _paymentGateway.ChargeAsync(
                new PaymentGatewayRequest(
                    plan.Price,
                    Currency,
                    $"TeamFlow {plan.Name} plan (1 month)",
                    dto.Card));

            payment = new Payment
            {
                OrganizationId = organizationId,
                SubscriptionId = subscription.Id,
                Amount = plan.Price,
                Currency = Currency,
                Status = result.Succeeded
                    ? PaymentStatus.Succeeded
                    : PaymentStatus.Failed,
                TransactionId = result.TransactionId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            if (!result.Succeeded)
            {
                // Keep a record of the failed attempt, but do not change the plan.
                await _context.SaveChangesAsync();

                throw new PaymentRequiredException(
                    result.FailureReason ?? "The payment was declined.");
            }
        }

        var previousPlanName = subscription.Plan.Name;
        var now = DateTime.UtcNow;

        subscription.PlanId = plan.Id;
        subscription.Plan = plan;
        subscription.Status = SubscriptionStatus.Active;
        subscription.StartDate = now;
        subscription.EndDate = plan.Price > 0 ? now.AddMonths(1) : null;

        _context.Notifications.Add(new Notification
        {
            UserId = _currentUser.UserId,
            Message = $"Your plan was changed from {previousPlanName} to {plan.Name}.",
            IsRead = false,
            CreatedAt = now
        });

        await _context.SaveChangesAsync();

        return await GetCurrentSubscriptionAsync();
    }

    public async Task<List<PaymentResponseDto>> GetPaymentsAsync()
    {
        var organizationId = _currentUser.OrganizationId;

        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.OrganizationId == organizationId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(100)
            .Select(p => new PaymentResponseDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status.ToString(),
                TransactionId = p.TransactionId,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    private async Task<UsageDto> GetUsageAsync(int organizationId)
    {
        var members = await _context.OrganizationMembers
            .CountAsync(m => m.OrganizationId == organizationId);

        var projects = await _context.Projects
            .CountAsync(p => p.OrganizationId == organizationId);

        var storage = await _context.Files
            .Where(f => f.Task.Project.OrganizationId == organizationId)
            .SumAsync(f => (long?)f.Size) ?? 0;

        return new UsageDto
        {
            Members = members,
            Projects = projects,
            StorageBytes = storage
        };
    }

    private async Task EnsureUsageFitsAsync(int organizationId, Plan plan)
    {
        var usage = await GetUsageAsync(organizationId);

        if (plan.MaxMembers != -1 && usage.Members > plan.MaxMembers)
        {
            throw new ConflictException(
                $"You have {usage.Members} members but the {plan.Name} plan allows {plan.MaxMembers}. Remove members first.");
        }

        if (plan.MaxProjects != -1 && usage.Projects > plan.MaxProjects)
        {
            throw new ConflictException(
                $"You have {usage.Projects} projects but the {plan.Name} plan allows {plan.MaxProjects}. Delete projects first.");
        }

        if (plan.MaxStorage != -1 && usage.StorageBytes > plan.MaxStorage)
        {
            throw new ConflictException(
                $"Your files use more storage than the {plan.Name} plan allows. Delete some files first.");
        }
    }

    private static PlanResponseDto ToDto(Plan plan)
    {
        return new PlanResponseDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Price = plan.Price,
            Currency = Currency,
            MaxMembers = plan.MaxMembers,
            MaxProjects = plan.MaxProjects,
            MaxStorage = plan.MaxStorage,
            IsFree = plan.Price <= 0
        };
    }
}
