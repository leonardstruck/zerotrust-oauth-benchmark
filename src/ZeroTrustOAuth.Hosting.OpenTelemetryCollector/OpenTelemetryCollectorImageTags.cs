namespace Aspire.Hosting;

/// <summary>
/// Provides constants for configuring the OpenTelemetry Collector container image,
/// including the image name, tag, and registry.
/// </summary>
public static class OpenTelemetryCollectorImageTags
{
    /// <summary>
    /// The name of the OpenTelemetry Collector container image.
    /// </summary>
    public const string Image = "otel/opentelemetry-collector-contrib";
    /// <summary>
    /// The tag/version of the OpenTelemetry Collector container image.
    /// </summary>
    public const string Tag = "0.140.0";
    /// <summary>
    /// The registry where the OpenTelemetry Collector image is hosted.
    /// </summary>
    public const string Registry = "docker.io";
}