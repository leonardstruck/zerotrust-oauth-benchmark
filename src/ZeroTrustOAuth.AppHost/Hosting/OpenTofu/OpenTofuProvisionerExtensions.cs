using System.Diagnostics.CodeAnalysis;

namespace ZeroTrustOAuth.AppHost.Hosting.OpenTofu;

[Experimental("ASPIRECONTAINERSHELLEXECUTION001")]
internal static class OpenTofuProvisionerExtensions
{
    private const string VariablePrefix = "TF_VAR_";
    private const string ContainerWorkingDirectory = "/workspace";

    public static IResourceBuilder<OpenTofuProvisionerResource> AddOpenTofuProvisioner(
        this IDistributedApplicationBuilder builder, [ResourceName] string name, string path)
    {
        return builder.AddResource(new OpenTofuProvisionerResource(name))
            .WithImage(OpenTofuProvisionerContainerImageTags.Image, OpenTofuProvisionerContainerImageTags.Tag)
            .WithImageRegistry(OpenTofuProvisionerContainerImageTags.Registry)
            .WithEnvironment("TF_IN_AUTOMATION", "true")
            .WithEnvironment("TF_INPUT", "false")
            .WithEnvironment("OTEL_TRACES_EXPORTER", "otlp")
            .WithBindMount(path, ContainerWorkingDirectory)
            .WithEntrypoint("/bin/sh")
            .WithArgs("cd", ContainerWorkingDirectory)
            .WithArgs("&&", "tofu init")
            .WithArgs("&&", "tofu plan")
            .WithArgs("&&", "tofu apply", "-auto-approve")
            .WithOtlpExporter();
    }

    extension<T>(IResourceBuilder<T> builder) where T : OpenTofuProvisionerResource
    {
        [PublicAPI]
        public IResourceBuilder<T> WithVariable(string name, string? value)
        {
            return builder.WithEnvironment(VariablePrefix + name, value);
        }

        [PublicAPI]
        public IResourceBuilder<T> WithVariable(string name,
            in ReferenceExpression.ExpressionInterpolatedStringHandler value)
        {
            return builder.WithEnvironment(VariablePrefix + name, value);
        }

        [PublicAPI]
        public IResourceBuilder<T> WithVariable(string name, ReferenceExpression value)
        {
            return builder.WithEnvironment(VariablePrefix + name, value);
        }

        [PublicAPI]
        public IResourceBuilder<T> WithVariable(string name, EndpointReference value)
        {
            return builder.WithEnvironment(VariablePrefix + name, value);
        }

        [PublicAPI]
        public IResourceBuilder<T> WithVariable(string name, ParameterResource value)
        {
            return builder.WithEnvironment(VariablePrefix + name, value);
        }
    }
}