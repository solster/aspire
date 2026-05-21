using Aspire.Azure.FrontDoor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

public static class AzureFrontDoorExtensions
{
    public static IHostApplicationBuilder AddAzureFrontDoor(
        this IHostApplicationBuilder builder,
        Action<AzureFrontDoorSettings>? configureSettings = null)
    {
        var settings = new AzureFrontDoorSettings
        {
            ProfileId = builder.Configuration["AzureFrontDoor:ProfileId"]
        };

        configureSettings?.Invoke(settings);

        builder.Services.AddSingleton(settings);

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto |
                ForwardedHeaders.XForwardedHost;

            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return builder;
    }

    public static WebApplication UseFrontDoor(this WebApplication app)
    {
        var settings = app.Services.GetService<AzureFrontDoorSettings>();
        var frontDoorId = settings?.ProfileId ?? app.Configuration["AzureFrontDoor:ProfileId"];

        if (String.IsNullOrEmpty(frontDoorId))
        {
            return app;
        }

        app.Use(async (context, next) =>
        {
            if (!context.Request.Headers.TryGetValue("X-Azure-FDID", out var fdid) ||
                !String.Equals(fdid, frontDoorId, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            await next();
        });

        return app;
    }
}
