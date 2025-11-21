namespace Aspire.Hosting;

/// <summary>
///     Settings that control how the embedded OpenTelemetry Collector is exposed by the hosting components.
/// </summary>
public class OpenTelemetryCollectorSettings
{
    /// <summary>
    ///     When true, enables the OTLP gRPC receiver endpoint on the collector. Recommended when clients support gRPC.
    /// </summary>
    public bool EnableGrpcEndpoint { get; set; } = true;

    /// <summary>
    ///     When true, enables the OTLP HTTP/JSON receiver endpoint on the collector. Useful for environments where HTTP is
    ///     preferred or required.
    /// </summary>
    public bool EnableHttpEndpoint { get; set; } = true;

    /// <summary>
    ///     When true, exposes the collector configuration (for example, through mounted files or endpoints) to aid in
    ///     diagnostics and transparency.
    /// </summary>
    public bool ExposeCollectorConfiguration { get; set; }

    /// <summary>
    ///     When true, exposes a health check endpoint that can be probed by orchestrators and monitoring systems to verify
    ///     collector readiness and liveness.
    /// </summary>
    public bool EnableHealthCheck { get; set; } = true;

    /// <summary>
    ///     When true, configures an OTLP exporter from the collector to the Aspire dashboard so traces/metrics/logs can be
    ///     visualized.
    /// </summary>
    public bool EnableAspireDashboardOtlpExport { get; set; } = true;
}