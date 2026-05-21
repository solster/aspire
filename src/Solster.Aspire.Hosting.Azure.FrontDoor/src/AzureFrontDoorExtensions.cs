using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure.FrontDoor;

// ReSharper disable once CheckNamespace
namespace Aspire.Hosting.Azure;

public static class AzureFrontDoorExtensions
{
    /// <summary>Adds an Azure Front Door profile resource to the distributed application.</summary>
    public static IResourceBuilder<AzureFrontDoorResource> AddAzureFrontDoor(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name)
    {
        builder.AddAzureProvisioning();
        return builder.AddResource(new AzureFrontDoorResource(name));
    }

    /// <summary>References a pre-provisioned Azure Front Door profile in publish mode.</summary>
    public static IResourceBuilder<AzureFrontDoorResource> AsExisting(
        this IResourceBuilder<AzureFrontDoorResource> builder,
        string resourceName,
        string? resourceGroup = null)
    {
        builder.WithAnnotation(new ExistingAzureResourceAnnotation(resourceName, resourceGroup));

        return builder.PublishAsExisting(resourceName, resourceGroup);
    }

    extension<T>(IResourceBuilder<T> builder) where T : IResourceWithEnvironment
    {
        /// <summary>
        /// Injects the Azure Front Door profile ID into the consumer via <c>AzureFrontDoor__ProfileId</c>.
        /// </summary>
        public IResourceBuilder<T> WithFrontDoorProfileId(IResourceBuilder<AzureFrontDoorResource> frontDoor)
        {
            return builder.WithEnvironment("AzureFrontDoor__ProfileId", frontDoor.Resource.ProfileIdExpression);
        }
    }
}
