namespace Aspire.Hosting;

/// <summary>
///     Provides constants for Grafana container image configuration.
/// </summary>
public static class GrafanaContainerImageTags
{
    /// <summary>
    ///     Represents the default container image used for Grafana configurations.
    /// </summary>
    public const string Image = "grafana/grafana";

    /// <summary>
    ///     Represents the specific tag version for the Grafana container image.
    /// </summary>
    public const string Tag = "12.3";

    /// <summary>
    ///     Represents the default container registry used for Grafana container images.
    /// </summary>
    public const string Registry = "docker.io";
}