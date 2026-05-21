using Aspire.Hosting.ApplicationModel;
using Azure.Provisioning;
using Azure.Provisioning.Communication;
using Azure.Provisioning.Primitives;

namespace Aspire.Hosting.Azure.CommunicationServices;

public sealed class AzureCommunicationServicesResource : AzureProvisioningResource, IResourceWithEndpoints
{
    private readonly BicepOutputReference _hostNameOutput;

    public AzureCommunicationServicesResource(string name) : base(name, BuildInfrastructure)
        => _hostNameOutput = new BicepOutputReference("hostName", this);

    public ReferenceExpression EndpointExpression =>
        ReferenceExpression.Create($"https://{_hostNameOutput}");

    public override ProvisionableResource AddAsExistingResource(AzureResourceInfrastructure infra)
    {
        var cs = CommunicationService.FromExisting(
            this.GetBicepIdentifier(),
            CommunicationService.ResourceVersions.V2023_04_01);

        TryApplyExistingResourceAnnotation(this, infra, cs);
        infra.Add(cs);
        infra.Add(new ProvisioningOutput("hostName", typeof(string)) { Value = cs.HostName });
        return cs;
    }

    private static void BuildInfrastructure(AzureResourceInfrastructure infra)
    {
        var cs = CreateExistingOrNewProvisionableResource(
            infra,
            (identifier, name) =>
            {
                var existing = CommunicationService.FromExisting(
                    identifier,
                    CommunicationService.ResourceVersions.V2023_04_01);
                existing.Name = name;
                return existing;
            },
            infrastructure => new CommunicationService(
                infrastructure.AspireResource.GetBicepIdentifier(),
                CommunicationService.ResourceVersions.V2023_04_01));

        infra.Add(new ProvisioningOutput("hostName", typeof(string)) { Value = cs.HostName });
    }
}
