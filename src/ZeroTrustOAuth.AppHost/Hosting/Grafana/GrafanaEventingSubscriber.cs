using Aspire.Hosting.Eventing;
using Aspire.Hosting.Lifecycle;

using Microsoft.Extensions.Logging;

namespace ZeroTrustOAuth.AppHost.Hosting.Grafana;

[UsedImplicitly]
public partial class GrafanaEventingSubscriber(ResourceLoggerService loggerService, DistributedApplicationModel model)
    : IDistributedApplicationEventingSubscriber
{
    private const string OtelExporterOtlpEndpoint = "OTEL_EXPORTER_OTLP_ENDPOINT";


    public Task SubscribeAsync(IDistributedApplicationEventing eventing,
        DistributedApplicationExecutionContext executionContext,
        CancellationToken cancellationToken)
    {
        GrafanaStackResource grafana = model.Resources.OfType<GrafanaStackResource>().Single();

        eventing.Subscribe<ResourceEndpointsAllocatedEvent>((@event, _) =>
            OnResourceEndpointsAllocatedEvent(@event, grafana));
        return Task.CompletedTask;
    }

    private Task OnResourceEndpointsAllocatedEvent(ResourceEndpointsAllocatedEvent @event,
        GrafanaStackResource receiver)
    {
        if (@event.Resource is GrafanaStackResource)
        {
            return Task.CompletedTask;
        }

        ILogger logger = loggerService.GetLogger(@event.Resource);

        @event.Resource.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
        {
            if (!context.EnvironmentVariables.ContainsKey(OtelExporterOtlpEndpoint))
            {
                return;
            }

            LogForwardingTelemetryForResourceNameToTheCollector(logger, @event.Resource.Name);
            context.EnvironmentVariables[OtelExporterOtlpEndpoint] =
                receiver.GetEndpoint(GrafanaStackResource.OtlpEndpointName);
        }));

        return Task.CompletedTask;
    }

    [LoggerMessage(LogLevel.Debug, "Forwarding telemetry for {resourceName} to grafana.")]
    static partial void LogForwardingTelemetryForResourceNameToTheCollector(ILogger logger, string resourceName);
}