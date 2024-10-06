using System.Web.Http.Description;
using AviaskApi.Entities;
using AviaskApi.Identity;
using AviaskApi.Models.Result;
using AviaskApi.Repositories;
using AviaskApi.Services.Filterable;
using AviaskApi.Services.Jwt;
using AviaskApi.Services.StripeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace AviaskApi.Controllers;

public class PaymentsController : AviaskController<PaymentsController>
{
    private readonly IStripeService _stripeService;
    private readonly UserRepository _userRepository;

    public PaymentsController(IStripeService stripeStripeService, IAviaskRepository<AviaskUser, Guid> userRepository,
        IJwtService jwt, ILogger<PaymentsController> logger,
        IFilterableService filterable) : base(jwt,
        logger, filterable)
    {
        _stripeService = stripeStripeService;
        _userRepository = (UserRepository)userRepository;
    }

    [HttpPost]
    [Route("/api/payments")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<IActionResult> Checkout()
    {
        //  Check if user already is premium
        var currentUser = await CurrentUserAsync();

        //  TODO: Use ENV var
        if (currentUser.IsPremium) return Ok("/payments/result?success=false");

        return Ok(await _stripeService.GetCheckoutUrlAsync(currentUser.Id));
    }

    [HttpPost]
    [Route("/api/payments/webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var stripeEvent = await _stripeService.GetStripeEventAsync(Request);
        if (stripeEvent is null) return BadRequest();

        switch (stripeEvent.Type)
        {
            case Events.CustomerSubscriptionCreated:
                await OnSubscriptionCreated((stripeEvent.Data.Object as Subscription)!);
                break;

            case Events.CustomerSubscriptionDeleted:
                await OnSubscriptionDeleted((stripeEvent.Data.Object as Subscription)!);
                break;
        }

        return Ok();
    }

    [HttpGet]
    [Route("/api/payments/current")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    [ResponseType(typeof(SubscriptionInformations))]
    public async Task<IActionResult> Current()
    {
        var currentUser = await CurrentUserAsync();

        if (!currentUser.IsPremium) return NotFound(new ApiErrorResponse("You are not premium"));

        return Ok(await _stripeService.GetUserSubscription(currentUser));
    }

    [HttpPost]
    [Route("/api/payments/current/cancel")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<IActionResult> CancelCurrentSubscription()
    {
        var currentUser = await CurrentUserAsync();

        if (!currentUser.IsPremium) return NotFound(new ApiErrorResponse("You are not premium"));

        await _stripeService.StopRenewalSubscription(currentUser);
        return Ok();
    }

    private async Task OnSubscriptionCreated(Subscription subscription)
    {
        var customer = await _stripeService.GetCustomerById(subscription.CustomerId);
        var user = await _userRepository.GetByEmailAsync(customer.Email ?? "");

        if (user is null)
        {
            Logger.LogCritical($"Could not retrieve AviaskUser linked to subscription '{subscription.Id}'");
            return;
        }

        user.IsPremium = true;
        await _userRepository.UpdateAsync(user);
    }

    private async Task OnSubscriptionDeleted(Subscription subscription)
    {
        var customer = await _stripeService.GetCustomerById(subscription.CustomerId);
        var user = await _userRepository.GetByEmailAsync(customer.Email ?? "");

        if (user is null)
        {
            Logger.LogCritical($"Could not retrieve AviaskUser linked to subscription '{subscription.Id}'");
            return;
        }

        user.IsPremium = false;
        await _userRepository.UpdateAsync(user);
    }
}
