using System.Net;
using AviaskApi.Models.Result;
using AviaskApiTest.Abstractions;
using Xunit.Abstractions;

namespace AviaskApiTest.Specs.Controllers;

public class PaymentsControllerTest : FunctionalTest
{
    public PaymentsControllerTest(FunctionalTestWebApplicationFactory factory, ITestOutputHelper output) : base(
        factory, output)
    {
    }

    [Fact]
    public async Task Checkout_ReturnsUnauthorized()
    {
        var response = await HttpClient.PostAsJsonAsync("api/payments", new { });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_ReturnsAlreadyPremium()
    {
        await Authenticate(premium: true);

        var response = await HttpClient.PostAsJsonAsync("api/payments", new { });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();

        Assert.Equal("/payments/result?success=false", result);
    }

    [Fact]
    public async Task Checkout_ReturnscheckoutUrl()
    {
        await Authenticate();

        var response = await HttpClient.PostAsJsonAsync("api/payments", new { });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();

        Assert.Equal("url", result);
    }

    [Fact]
    public async Task StripeWebhook_FailsNoEvent()
    {
        var response = await HttpClient.PostAsJsonAsync("api/payments/webhook", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Current_ReturnsUnauthorized()
    {
        var response = await HttpClient.GetAsync("api/payments/current");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Current_ReturnsNotFound()
    {
        await Authenticate();

        var response = await HttpClient.GetAsync("api/payments/current");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Current_ReturnsSubscriptionInformation()
    {
        var expected =
            new SubscriptionInformations(SubscriptionStatus.ACTIVE, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
        await Authenticate(premium: true);

        var response = await HttpClient.GetAsync("api/payments/current");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<SubscriptionInformations>();
        Assert.Equal(expected.Status, result.Status);
        Assert.True(result.NextPayment > result.StartedAt);
    }
}