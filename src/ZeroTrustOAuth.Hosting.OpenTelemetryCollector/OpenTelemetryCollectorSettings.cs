namespace Aspire.Hosting;

public record OpenTelemetryCollectorSettings(
    bool EnableGrpcEndpoint = true,
    bool EnableHttpEndpoint = true,
    bool EnableHealthCheck = true,
    bool EnableAspireOtlpExporter = true
);