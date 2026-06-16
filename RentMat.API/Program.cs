using Microsoft.EntityFrameworkCore;
using RentMat.API.ExceptionHandling;
using RentMat.Application;
using RentMat.Infrastructure.Data;
using Serilog;
using ZiggyCreatures.Caching.Fusion;


var builder = WebApplication.CreateBuilder(args);



builder.Host.UseSerilog((ctx, loggerConfig) =>
{
    loggerConfig
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics", Serilog.Events.LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Seq(ctx.Configuration["Seq:DefaultConnection"] ?? "http://localhost:5341");
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddFusionCache()
    .WithDefaultEntryOptions(o =>
    {
        o.Duration = TimeSpan.FromMinutes(5);
        o.IsFailSafeEnabled = true;
        o.FailSafeMaxDuration = TimeSpan.FromHours(2);
        o.FactorySoftTimeout = TimeSpan.FromMilliseconds(200);
        o.FactoryHardTimeout = TimeSpan.FromSeconds(5);
    })
    .WithSystemTextJsonSerializer();

builder.Services.RegisterHandlers();
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "RentMat.API");
        options.RoutePrefix = string.Empty;
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

try
{
    Log.Information("Starting API");
    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}