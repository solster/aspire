using Azure.Core;

namespace Aspire.Azure.CommunicationServices;

public sealed class AzureCommunicationEmailSettings
{
    public string? Endpoint { get; set; }

    public TokenCredential? Credential { get; set; }
}
