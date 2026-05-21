using Aspire.Hosting.ApplicationModel;
using Azure.Provisioning;
using Azure.Provisioning.Cdn;
using Azure.Provisioning.Primitives;

namespace Aspire.Hosting.Azure.FrontDoor;

public sealed class AzureFrontDoorResource : AzureProvisioningResource
{
    private readonly BicepOutputReference _profileIdOutput;

    public AzureFrontDoorResource(string name) : base(name, BuildInfrastructure)
        => _profileIdOutput = new BicepOutputReference("profileId", this);

    public ReferenceExpression ProfileIdExpression =>
        ReferenceExpression.Create($"{_profileIdOutput}");

    public override ProvisionableResource AddAsExistingResource(AzureResourceInfrastructure infra)
    {
        var profile = CdnProfile.FromExisting(
            this.GetBicepIdentifier(),
            CdnProfile.ResourceVersions.V2025_06_01);

        TryApplyExistingResourceAnnotation(this, infra, profile);
        infra.Add(profile);
        infra.Add(new ProvisioningOutput("profileId", typeof(string)) { Value = profile.FrontDoorId });
        return profile;
    }

    private static void BuildInfrastructure(AzureResourceInfrastructure infra)
    {
        var profile = CreateExistingOrNewProvisionableResource(
            infra,
            (identifier, name) =>
            {
                var existing = CdnProfile.FromExisting(
                    identifier,
                    CdnProfile.ResourceVersions.V2025_06_01);
                existing.Name = name;
                return existing;
            },
            infrastructure => new CdnProfile(
                infrastructure.AspireResource.GetBicepIdentifier(),
                CdnProfile.ResourceVersions.V2025_06_01)
            {
                SkuName = CdnSkuName.StandardAzureFrontDoor
            });

        infra.Add(new ProvisioningOutput("profileId", typeof(string)) { Value = profile.FrontDoorId });
    }
}
