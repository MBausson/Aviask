using System.Web.Http;
using Microsoft.AspNetCore.Diagnostics;

namespace AviaskApi.Configuration;

public static class Middlewares
{
    public static WebApplication UseHttpResponseExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(appBuilder =>
        {
            appBuilder.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature?.Error as HttpResponseException;

                if (exception is null) return;

                context.Response.StatusCode = (int)exception.Response.StatusCode;
                await exception.Response.Content.CopyToAsync(context.Response.BodyWriter.AsStream());
            });
        });

        return app;
    }
}