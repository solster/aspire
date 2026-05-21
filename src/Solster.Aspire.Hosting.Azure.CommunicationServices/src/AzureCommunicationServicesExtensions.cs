using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure.CommunicationServices;

// ReSharper disable once CheckNamespace
namespace Aspire.Hosting.Azure;

public static class AzureCommunicationServicesExtensions
{
    // Contributor (b24988ac-...): broad management-plane role; no ACS-specific sending role exists.
    // Grants full control of the ACS resource including deletion. Least-privilege alternative
    // would be a custom role, but Contributor is the standard Aspire pattern for ACS.
    private static readonly RoleDefinition ContributorRole =
        new("b24988ac-6180-42a0-ab88-20f7382dd24c", "Contributor");

    /// <summary>Adds an Azure Communication Services resource to the distributed application.</summary>
    public static IResourceBuilder<AzureCommunicationServicesResource> AddAzureCommunicationServices(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name)
    {
        builder.AddAzureProvisioning();
        return builder.AddResource(new AzureCommunicationServicesResource(name));
    }

    /// <summary>References a pre-provisioned ACS instance in publish mode.</summary>
    public static IResourceBuilder<AzureCommunicationServicesResource> AsExisting(
        this IResourceBuilder<AzureCommunicationServicesResource> builder,
        string resourceName,
        string? resourceGroup = null)
    {
        builder.WithAnnotation(new ExistingAzureResourceAnnotation(resourceName, resourceGroup));

        return builder.PublishAsExisting(resourceName, resourceGroup);
    }

    extension<T>(IResourceBuilder<T> builder) where T : IResourceWithEnvironment
    {
        /// <summary>
        /// Injects the ACS endpoint URL into the consumer via <c>ConnectionStrings__acs</c>.
        /// Uses <see cref="AzureCommunicationServicesResource.EndpointExpression"/> which resolves
        /// to the ACS host URL from the Bicep output.
        /// </summary>
        public IResourceBuilder<T> WithAcsEndpoint(IResourceBuilder<AzureCommunicationServicesResource> acs)
        {
            return builder.WithEnvironment("ConnectionStrings__acs", acs.Resource.EndpointExpression);
        }

        /// <summary>Grants the Contributor role on ACS to the resource's managed identity.</summary>
        public IResourceBuilder<T> WithRoleAssignments(IResourceBuilder<AzureCommunicationServicesResource> target)
        {
            builder.WithAnnotation(new RoleAssignmentAnnotation(
                target.Resource,
                new HashSet<RoleDefinition> { ContributorRole }));

            return builder;
        }
    }
}
