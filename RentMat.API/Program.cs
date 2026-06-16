using Microsoft.EntityFrameworkCore;
using RentMat.Application;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

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

app.Run();