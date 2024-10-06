using AviaskApi.Configuration;
using AviaskApi.Entities;
using AviaskApi.Models.Result;
using AviaskApi.Repositories;
using Stripe;
using Stripe.Checkout;

namespace AviaskApi.Services.StripeService;

public class StripeService : IStripeService
{
    private static string? _cachedPricedId;
    private readonly string _basePath;
    private readonly string _failurePath;
    private readonly ILogger<StripeService> _logger;
    private readonly string _productName = "AviaskPremium";
    private readonly IStripeClient _stripeClient;

    private readonly string _successPath;
    private readonly UserRepository _userRepository;
    private readonly string _webhookEndpointId;

    public StripeService(IStripeClient stripeClient,
        IAviaskRepository<AviaskUser, Guid> userRepository, ILogger<StripeService> logger)
    {
        _stripeClient = stripeClient;
        _userRepository = (UserRepository)userRepository;
        _webhookEndpointId = Env.Get("STRIPE_WEBHOOK_ENDPOINT_ID");
        _logger = logger;

        _successPath = Env.Get("STRIPE_URL_SUCCESS");
        _failurePath = Env.Get("STRIPE_URL_FAILURE");
        _basePath = Env.Get("STRIPE_URL_BASE");
    }

    public async Task<Customer> GetCustomerById(string customerId)
    {
        return await new CustomerService(_stripeClient).GetAsync(customerId);
    }

    public async Task<string> GetCheckoutUrlAsync(Guid userId)
    {
        var customer = await CreateCustomerIfNotExists(userId);
        if (customer is null) return _basePath + _failurePath;

        var session = await new SessionService(_stripeClient).CreateAsync(await getCheckoutOptionsAsync(customer));

        return session.Url;
    }

    public async Task<Event?> GetStripeEventAsync(HttpRequest request)
    {
        var json = await new StreamReader(request.Body).ReadToEndAsync();

        try
        {
            return EventUtility.ConstructEvent(json, request.Headers["Stripe-Signature"], _webhookEndpointId);
        }
        catch (StripeException)
        {
            return null;
        }
    }

    public async Task<SubscriptionInformations?> GetUserSubscription(AviaskUser user)
    {
        var customer = await GetCustomerByEmail(user.Email!);
        if (customer is null) return null;

        //  From user' subscriptions, take the latest
        var subscription = (await GetCustomerSubscriptions(customer)).MaxBy(s => s.Created);

        if (subscription is null)
            return new SubscriptionInformations(
                SubscriptionStatus.CANCELLED,
                DateTimeOffset.MaxValue,
                DateTimeOffset.MaxValue
            );

        //  If the subscription is cancelled (= there's a cancellation reason) or not active => cancelled
        return new SubscriptionInformations(
            subscription.CancellationDetails.Reason is not null || subscription.Status != "active"
                ? SubscriptionStatus.CANCELLED
                : SubscriptionStatus.ACTIVE, subscription.Created,
            subscription.CurrentPeriodEnd);
    }

    public async Task StopRenewalSubscription(AviaskUser user)
    {
        var customer = await GetCustomerByEmail(user.Email);
        if (customer is null) return;

        var subscriptions = await GetActiveSubscriptions(customer);

        var service = new SubscriptionService();

        foreach (var subscription in subscriptions)
        {
            await service.UpdateAsync(subscription.Id, new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = true
            });

            _logger.LogInformation($"Cancelled renewal subscription '{subscription.Id}' for user '{user.Id}'");
        }
    }

    public async Task StopCurrentSubscription(AviaskUser user)
    {
        var customer = await GetCustomerByEmail(user.Email);
        if (customer is null) return;

        var subscriptions = (await GetCustomerSubscriptions(customer)).ToArray();
        var service = new SubscriptionService();

        foreach (var subscription in subscriptions)
        {
            if (subscription.Status is not ("trialing" or "active")) continue;

            await service.CancelAsync(subscription.Id);

            _logger.LogInformation($"Cancelled immediately subscription '{subscription.Id} from user '{user.Id}'");
        }
    }

    /// <summary>
    ///     Caches the product price ID
    /// </summary>
    /// <returns>Stripe product ID</returns>
    /// <exception cref="InvalidOperationException">Thrown if the product/price can't be found</exception>
    private async Task<string> GetProductPriceIdAsync()
    {
        if (_cachedPricedId is not null) return _cachedPricedId;

        var productSearchOptions = new ProductSearchOptions
        {
            Query = $"name~\"{_productName}\""
        };

        //  Fetches product from its name
        var product = (await new ProductService().SearchAsync(productSearchOptions)).Data.FirstOrDefault();
        if (product is null)
            throw new InvalidOperationException($"Could not find product with the name ${_productName}");

        //  Fetches product price
        var priceSearchOptions = new PriceSearchOptions
        {
            Query = $"product:\"{product.Id}\""
        };

        var price = (await new PriceService().SearchAsync(priceSearchOptions)).Data.FirstOrDefault();
        if (price is null) throw new InvalidOperationException($"Could not find price for product ${product.Id}");

        _cachedPricedId = price.Id;

        return _cachedPricedId;
    }

    /// <summary>
    ///     Creates a stripe customer if it doesn't already exist
    /// </summary>
    /// <param name="userId">Aviask user ID</param>
    /// <returns>Existing customer or created</returns>
    private async Task<Customer?> CreateCustomerIfNotExists(Guid userId)
    {
        //  Gather user informations
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) return null;

        var customerFound = await GetCustomerByEmail(user.Email!);
        if (customerFound is not null) return customerFound;

        //  Create the stripe customer if he doesn't exist
        var createCustomerOptions = new CustomerCreateOptions
        {
            Email = user.Email,
            Name = user.UserName,
            Description = "Aviask user",
            Metadata = new Dictionary<string, string>
                { { "userId", user.Id.ToString() } }
        };

        var createdCustomer = await new CustomerService().CreateAsync(createCustomerOptions);

        return createdCustomer;
    }

    /// <summary>
    ///     Fetches a stripe customer via its email
    /// </summary>
    /// <returns>Stripe customer associated to the given email</returns>
    /// <remarks>If no customer is associated with the given email, null is returned</remarks>
    private async Task<Customer?> GetCustomerByEmail(string email)
    {
        var options = new CustomerListOptions
        {
            Email = email
        };

        var customers = await new CustomerService().ListAsync(options);

        return customers.Data.FirstOrDefault(c => c.Email == email);
    }

    /// <summary>
    ///     Fetches subscriptions related to a given customer
    /// </summary>
    /// <param name="customer">Stripe customer</param>
    /// <returns>Customer' subscriptions</returns>
    private async Task<IEnumerable<Subscription>> GetCustomerSubscriptions(Customer customer)
    {
        var options = new SubscriptionListOptions
        {
            Customer = customer.Id
        };

        var service = new SubscriptionService();

        return await service.ListAsync(options);
    }

    private async Task<IEnumerable<Subscription>> GetActiveSubscriptions(Customer customer)
    {
        return (await GetCustomerSubscriptions(customer))
            .Where(c => c.Status is "trialing" or "active")
            .ToArray();
    }

    private async Task<SessionCreateOptions> getCheckoutOptionsAsync(Customer customer)
    {
        return new SessionCreateOptions
        {
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = await GetProductPriceIdAsync(),
                    Quantity = 1
                }
            ],
            Mode = "subscription",
            PhoneNumberCollection = new SessionPhoneNumberCollectionOptions
            {
                Enabled = false
            },
            SuccessUrl = _basePath + _successPath,
            CancelUrl = _basePath + _failurePath,
            Customer = customer.Id
        };
    }
}