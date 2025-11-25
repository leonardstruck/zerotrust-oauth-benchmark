using System.Diagnostics.Metrics;

namespace ZeroTrustOAuth.ServiceDefaults;

internal static class AuthMetrics
{
    public const string MeterName = "ZeroTrustOAuth.AuthTelemetry";

    private static readonly Meter Meter = new(MeterName);
    private static readonly Histogram<int> MissingScopesHistogram =
        Meter.CreateHistogram<int>("auth.scopes.missing.count");
    private static readonly Histogram<int> ExcessScopesHistogram =
        Meter.CreateHistogram<int>("auth.scopes.excess.count");
    private static readonly Counter<long> IntrospectionCallsCounter =
        Meter.CreateCounter<long>("oauth.introspection.calls");
    private static readonly Counter<long> TokenExchangeCallsCounter =
        Meter.CreateCounter<long>("oauth.token_exchange.calls");

    public static void RecordScopeCounts(int missingCount, int excessCount)
    {
        MissingScopesHistogram.Record(missingCount);
        ExcessScopesHistogram.Record(excessCount);
    }

    public static void IncrementIntrospection() => IntrospectionCallsCounter.Add(1);

    public static void IncrementTokenExchange() => TokenExchangeCallsCounter.Add(1);
}
