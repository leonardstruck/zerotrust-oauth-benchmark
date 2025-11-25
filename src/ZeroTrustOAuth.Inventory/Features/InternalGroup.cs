using FastEndpoints;

namespace ZeroTrustOAuth.Inventory.Features;

public sealed class InternalGroup : Group
{
    public InternalGroup()
    {
        Configure("internal", ep =>
        {
            ep.Tags("Internal");
        });
    }
}