using Azure.Communication.Email;
using Azure.Core.Extensions;
using Azure.Identity;
using Aspire.Azure.CommunicationServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

public static class AzureCommunicationEmailClientExtensions
{
    private const string DefaultConfigSectionName = "Aspire:Azure:Communication:Email";

    public static IHostApplicationBuilder AddAzureCommunicationEmailClient(
        this IHostApplicationBuilder builder,
        string connectionName,
        Action<AzureCommunicationEmailSettings>? configureSettings = null,
        Action<IAzureClientBuilder<EmailClient, EmailClientOptions>>? configureClientBuilder = null)
    {
        var settings = GetSettings(builder, connectionName, DefaultConfigSectionName, configureSettings);

        builder.Services.AddAzureClients(azureBuilder =>
        {
            azureBuilder.UseCredential(settings.Credential ?? CreateCredential(builder));

            var clientBuilder = azureBuilder
                .AddClient<EmailClient, EmailClientOptions>((options, credential) =>
                    new EmailClient(new Uri(settings.Endpoint!), credential, options))
                .ConfigureOptions(builder.Configuration.GetSection($"{DefaultConfigSectionName}:ClientOptions"));

            configureClientBuilder?.Invoke(clientBuilder);
        });

        return builder;
    }

    public static IHostApplicationBuilder AddKeyedAzureCommunicationEmailClient(
        this IHostApplicationBuilder builder,
        string name,
        Action<AzureCommunicationEmailSettings>? configureSettings = null,
        Action<IAzureClientBuilder<EmailClient, EmailClientOptions>>? configureClientBuilder = null)
    {
        var configSectionName = $"{DefaultConfigSectionName}:{name}";
        var settings = GetSettings(builder, name, configSectionName, configureSettings);

        builder.Services.AddAzureClients(azureBuilder =>
        {
            azureBuilder.UseCredential(settings.Credential ?? CreateCredential(builder));

            var clientBuilder = azureBuilder
                .AddClient<EmailClient, EmailClientOptions>((options, credential) =>
                    new EmailClient(new Uri(settings.Endpoint!), credential, options))
                .ConfigureOptions(builder.Configuration.GetSection($"{configSectionName}:ClientOptions"))
                .WithName(name);

            configureClientBuilder?.Invoke(clientBuilder);
        });

        builder.Services.AddKeyedSingleton(name, (sp, _) =>
            sp.GetRequiredService<IAzureClientFactory<EmailClient>>().CreateClient(name));

        return builder;
    }

    private static AzureCommunicationEmailSettings GetSettings(
        IHostApplicationBuilder builder,
        string connectionName,
        string configSectionName,
        Action<AzureCommunicationEmailSettings>? configureSettings)
    {
        var settings = new AzureCommunicationEmailSettings
        {
            Endpoint = builder.Configuration[$"{configSectionName}:Endpoint"]
        };

        settings.Endpoint = builder.Configuration.GetConnectionString(connectionName) ?? settings.Endpoint;
        configureSettings?.Invoke(settings);

        if (String.IsNullOrWhiteSpace(settings.Endpoint))
        {
            throw new InvalidOperationException($"Connection string '{connectionName}' is not configured.");
        }

        return settings;
    }

    private static DefaultAzureCredential CreateCredential(IHostApplicationBuilder builder)
    {
        var credentialOptions = new DefaultAzureCredentialOptions();
        var managedIdentityClientId = builder.Configuration["AZURE_CLIENT_ID"];

        if (!String.IsNullOrWhiteSpace(managedIdentityClientId))
        {
            credentialOptions.ManagedIdentityClientId = managedIdentityClientId;
        }

        return new DefaultAzureCredential(credentialOptions);
    }
}
