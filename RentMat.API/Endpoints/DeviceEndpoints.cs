using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RentMat.API.Common.Security;
using RentMat.Application.Common;
using RentMat.Application.DTOs.Device;
using RentMat.Application.Handlers.Devices;
using RentMat.Application.Queries;

namespace RentMat.API.Endpoints;

internal static class DeviceEndpoints
{
    public static void MapDeviceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/devices")
            .WithTags("Devices");

        group.MapGet("/", GetAll)
            .WithName("GetDevices")
            .WithSummary("Returns all devices")
            .ProducesProblem(400);

        group.MapGet("/{id:int}", GetById)
            .WithName("GetDeviceById")
            .WithSummary("Returns a device by id")
            .ProducesProblem(400);
        
        group.MapPost("/create", Create)
            .RequireAuthorization(Policies.AdminOnly)
            .WithName("CreateDevice")
            .WithSummary("Creates a device")
            .ProducesValidationProblem()
            .ProducesProblem(400)
            .ProducesProblem(404);
        
        group.MapPut("/{id:int}", Update)
            .RequireAuthorization(Policies.AdminOnly)
            .WithName("UpdateDevice")
            .WithSummary("Updates a device")
            .ProducesValidationProblem()
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404);

        group.MapPut("/{id:int}/retire", Retire)
            .RequireAuthorization(Policies.AdminOnly)
            .WithName("RetireDevice")
            .WithSummary("Changes a device status to retired")
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404);
    }

    private static async Task<Ok<PagedResponse<DeviceResponseDto>>> GetAll(
        [AsParameters] GetAllDevicesQuery query, 
        [FromServices]  GetAllDevicesHandler handler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await handler.Handle(query, cancellationToken));
    }

    private static async Task<Ok<DeviceResponseDto>> GetById(
        int id, 
        [FromServices]  GetDeviceByIdHandler handler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await handler.Handle(id, cancellationToken));
    }
    
    private static async Task<CreatedAtRoute<DeviceResponseDto>> Create(
        DeviceCreateDto dto, 
        [FromServices] CreateDeviceHandler handler,
        CancellationToken cancellationToken)
    {
        var device = await handler.Handle(dto, cancellationToken);
        return TypedResults.CreatedAtRoute(device, routeName:"GetDeviceById", routeValues: new {id = device.Id});
    }

    private static async Task<NoContent> Update(
        int id, 
        DeviceUpdateDto dto, 
        [FromServices]  UpdateDeviceHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(id, dto, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> Retire(
        int id, 
        [FromServices] RetireDeviceHandler handler, 
        CancellationToken cancellationToken)
    {
        await handler.Handle(id, cancellationToken);
        return TypedResults.NoContent();
    }
}