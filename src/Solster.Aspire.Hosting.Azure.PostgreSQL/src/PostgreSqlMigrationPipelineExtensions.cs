using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Pipelines;
using Azure.Core;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

// ReSharper disable once CheckNamespace
namespace Aspire.Hosting.Azure;

public static class PostgreSqlMigrationPipelineExtensions
{
    public static IResourceBuilder<AzurePostgresFlexibleServerDatabaseResource> WithMigration<TContext>(
        this IResourceBuilder<AzurePostgresFlexibleServerDatabaseResource> builder,
        IResourceBuilder<AzureUserAssignedIdentityResource> identityBuilder)
        where TContext : DbContext
    {
        var stepName = $"migration-{builder.Resource.Name}";

        builder.WithPipelineStepFactory(stepName, async context =>
        {
            var task = await context.ReportingStep.CreateTaskAsync($"Migrating database ({builder.Resource.Name})", context.CancellationToken);

            await using (task.ConfigureAwait(false))
            {
                try
                {
                    var connectionString =
                        await builder.Resource.ConnectionStringExpression.GetValueAsync(context.CancellationToken);

                    if (string.IsNullOrEmpty(connectionString))
                    {
                        await task.FailAsync("Connection string is empty");
                        return;
                    }

                    var clientId = await identityBuilder.Resource.ClientId.GetValueAsync(context.CancellationToken);

                    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                    {
                        ManagedIdentityClientId = clientId
                    });

                    context.Logger.LogInformation("Acquiring access token for database migration...");

                    var accessToken = await credential.GetTokenAsync(
                        new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]),
                        context.CancellationToken
                    );

                    var user = await identityBuilder.Resource.NameOutputReference.GetValueAsync(context.CancellationToken);

                    if (string.IsNullOrEmpty(user))
                    {
                        throw new InvalidOperationException(
                            "Could not determine the AAD principal name for the database connection. " +
                            "The identity name is empty.");
                    }

                    context.Logger.LogInformation("Using Azure PostgreSQL migration username from managed identity name: {user}", user);

                    var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
                    {
                        Username = user,
                        Password = accessToken.Token,
                        SslMode = SslMode.Require
                    };

                    var optionsBuilder = new DbContextOptionsBuilder<TContext>()
                        .UseNpgsql(connectionStringBuilder.ConnectionString, npgsql =>
                            npgsql.MigrationsAssembly(typeof(TContext).Assembly.GetName().Name));

                    await using var db = MigrationDbContextFactory<TContext>.Create(optionsBuilder.Options);

                    var applied = (await db.Database.GetAppliedMigrationsAsync(context.CancellationToken)).ToList();
                    context.Logger.LogInformation("Applied migrations ({count}): {migrations}", applied.Count, string.Join(", ", applied));

                    var pending = (await db.Database.GetPendingMigrationsAsync(context.CancellationToken)).ToList();
                    context.Logger.LogInformation("Pending migrations ({count}): {migrations}", pending.Count, string.Join(", ", pending));

                    await db.Database.MigrateAsync(context.CancellationToken);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    await task.FailAsync(e.Message);
                }
            }
        }, requiredBy: [WellKnownPipelineSteps.Deploy]);

        return builder;
    }
}
