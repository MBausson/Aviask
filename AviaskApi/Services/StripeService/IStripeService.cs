using AviaskApi.Entities;
using AviaskApi.Models.Result;
using Stripe;

namespace AviaskApi.Services.StripeService;

public interface IStripeService
{
    /// <summary>
    ///     Fetches stripe customer via its id
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Stripe customer associated to the given ID</returns>
    public Task<Customer> GetCustomerById(string customerId);

    /// <summary>
    ///     Returns the callback URL associated with an attempt to checkout
    /// </summary>
    /// <returns>The checkout or the failure callback URL</returns>
    public Task<string> GetCheckoutUrlAsync(Guid userId);

    /// <summary>
    ///     Process an HTTP request to output the stripe event
    /// </summary>
    public Task<Event?> GetStripeEventAsync(HttpRequest request);

    /// <summary>
    ///     Fetches informations about a user' subscription
    /// </summary>
    /// <returns>Subscriptions informations about the given user</returns>
    public Task<SubscriptionInformations?> GetUserSubscription(AviaskUser user);

    /// <summary>
    ///     Stops the renewal of all user subscription
    /// </summary>
    public Task StopRenewalSubscription(AviaskUser user);

    /// <summary>
    ///     Stops immediately all user' subscriptions
    /// </summary>
    public Task StopCurrentSubscription(AviaskUser user);
}