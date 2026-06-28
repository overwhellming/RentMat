using Serilog;
using Serilog.Events;

namespace RentMat.API.Registrars;

internal static class SerilogRegistrar
{
    public static ConfigureHostBuilder RegisterSerilog(this ConfigureHostBuilder host)
    {
        host.UseSerilog((ctx, loggerConfig) =>
        {
            loggerConfig
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Seq(ctx.Configuration["Seq:DefaultConnection"] ?? "http://localhost:5341");
        });
        return host;
    }
}