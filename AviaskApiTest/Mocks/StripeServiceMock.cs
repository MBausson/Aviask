using System.Security.Cryptography;
using System.Text;
using AviaskApi.Configuration;
using AviaskApi.Entities;
using AviaskApi.Models.Result;
using AviaskApi.Services.StripeService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace AviaskApiTest.Mocks;

public class StripeServiceMock : IStripeService
{
    private readonly string _webhookEndpointId;

    public StripeServiceMock(IConfiguration configuration)
    {
        _webhookEndpointId = Env.Get("STRIPE_WEBHOOK_ENDPOINT_ID");
    }

    public async Task<string> GetCheckoutUrlAsync(Guid userId)
    {
        return "url";
    }

    public Task<Customer?> GetCustomerById(string customerId)
    {
        throw new NotImplementedException();
    }

    public async Task<Event?> GetStripeEventAsync(HttpRequest request)
    {
        var json = await new StreamReader(request.Body).ReadToEndAsync();
        var signature =
            ComputeSignature(_webhookEndpointId, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), "{}");

        try
        {
            return EventUtility.ConstructEvent(json, signature, _webhookEndpointId,
                throwOnApiVersionMismatch: false);
        }
        catch (StripeException)
        {
            return null;
        }
    }

    public async Task<SubscriptionInformations?> GetUserSubscription(AviaskUser user)
    {
        return new SubscriptionInformations(SubscriptionStatus.ACTIVE, DateTime.Now.AddDays(-1),
            DateTime.Now.AddDays(1));
    }

    public async Task StopRenewalSubscription(AviaskUser user)
    {
        user.IsPremium = false;
    }

    public async Task StopCurrentSubscription(AviaskUser user)
    {
        user.IsPremium = false;
    }

    //  See: https://chekkan.com/2023/08/17/unit-testing-stripe-webhook.html
    private static string ComputeSignature(
        string secret,
        string timestamp,
        string payload)
    {
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes($"{timestamp}.{payload}");

        using var cryptographer = new HMACSHA256(secretBytes);
        var hash = cryptographer.ComputeHash(payloadBytes);
        return BitConverter.ToString(hash)
            .Replace("-", string.Empty).ToLowerInvariant();
    }
}