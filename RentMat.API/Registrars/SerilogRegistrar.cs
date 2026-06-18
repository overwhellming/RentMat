using Serilog;

namespace RentMat.API.Registrars;

internal static class SerilogRegistrar
{
    public static ConfigureHostBuilder RegisterSerilog(this ConfigureHostBuilder host)
    {
        host.UseSerilog((ctx, loggerConfig) =>
        {
            loggerConfig
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Seq(ctx.Configuration["Seq:DefaultConnection"] ?? "http://localhost:5341");
        });
        return host;
    }
}