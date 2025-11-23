using System.Diagnostics.CodeAnalysis;

namespace ZeroTrustOAuth.AppHost.Hosting.OpenTofu;

[Experimental("ASPIRECONTAINERSHELLEXECUTION001")]
public class OpenTofuProvisionerResource : ContainerResource
{
    public OpenTofuProvisionerResource(string name) : base(name)
    {
        ShellExecution = true;
    }
};