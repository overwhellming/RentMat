using ZiggyCreatures.Caching.Fusion;

namespace RentMat.API.Registrars;

internal static class FusionCacheRegistrar
{
    public static IServiceCollection RegisterFusionCache(this IServiceCollection services)
    {
        services.AddFusionCache()
            .WithDefaultEntryOptions(o =>
            {
                o.Duration = TimeSpan.FromMinutes(5);
                o.IsFailSafeEnabled = true;
                o.FailSafeMaxDuration = TimeSpan.FromHours(2);
                o.FactorySoftTimeout = TimeSpan.FromMilliseconds(200);
                o.FactoryHardTimeout = TimeSpan.FromSeconds(5);
            })
            .WithSystemTextJsonSerializer();

        return services;
    }
}