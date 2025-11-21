namespace Aspire.Hosting;

/// <summary>
///     Represents the configuration settings for Grafana Resources.
/// </summary>
[PublicAPI]
public class GrafanaSettings
{
    /// <summary>
    ///     Determines whether the health check functionality is enabled.
    /// </summary>
    public bool EnableHealthCheck { get; set; } = true;
}