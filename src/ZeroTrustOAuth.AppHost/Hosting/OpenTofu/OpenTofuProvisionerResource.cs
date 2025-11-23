namespace ZeroTrustOAuth.AppHost.Hosting.OpenTofu;

public class OpenTofuProvisionerResource(string name, string? entrypoint = null) : ContainerResource(name, entrypoint);