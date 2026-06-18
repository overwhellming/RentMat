using FluentValidation.AspNetCore;
using RentMat.API.ExceptionHandling;
using RentMat.API.Registrars;
using RentMat.Application.Registrars;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.RegisterSerilog();

builder.Services.RegisterDatabase(builder.Configuration);
builder.Services.RegisterJwt(builder.Configuration);
builder.Services.RegisterSwagger();
builder.Services.RegisterFusionCache();
builder.Services.RegisterHandlers();
builder.Services.RegisterValidators();
builder.Services.RegisterServices();

builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();

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
    app.UseHttpsRedirection();

app.UseAuthentication();
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