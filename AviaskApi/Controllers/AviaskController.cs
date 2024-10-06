using System.Net;
using System.Web.Http;
using AviaskApi.Entities;
using AviaskApi.Services.Filterable;
using AviaskApi.Services.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace AviaskApi.Controllers;

public class AviaskController<TController> : ControllerBase
{
    protected readonly IFilterableService Filterable;
    protected readonly IJwtService Jwt;
    protected readonly ILogger<TController> Logger;

    public AviaskController(IJwtService jwt, ILogger<TController> logger, IFilterableService filterable)
    {
        Jwt = jwt;
        Logger = logger;
        Filterable = filterable;
    }

    protected async Task<AviaskUser?> TryCurrentUserAsync()
    {
        return await Jwt.CurrentUserAuthenticatedAsync(User);
    }

    protected async Task<AviaskUser> CurrentUserAsync()
    {
        var result = await Jwt.CurrentUserAuthenticatedAsync(User);

        //  We should avoid throwing exceptions during requests since they slow down the application
        //  This method should only be called in an authorized action, so hopefully the error is never thrown
        if (result is null)
        {
            var message = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("You need to be authenticated to access this.")
            };

            throw new HttpResponseException(message);
        }

        return result;
    }
}