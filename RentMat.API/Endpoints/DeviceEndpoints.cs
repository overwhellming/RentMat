using Microsoft.AspNetCore.Http.HttpResults;
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
            .Produces(400);

        group.MapGet("/{id:int}", GetById)
            .WithName("GetDeviceById")
            .WithSummary("Returns a device by id")
            .Produces(400);

        group.MapPut("/{id:int}", Update)
            .RequireAuthorization(Policies.AdminOnly)
            .WithName("UpdateDevice")
            .WithSummary("Updates a device")
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        group.MapPut("/{id:int}/retire", Retire)
            .RequireAuthorization(Policies.AdminOnly)
            .WithName("RetireDevice")
            .WithSummary("Changes a device status to retired")
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);
    }

    private static async Task<Ok<PagedResponse<DeviceResponseDto>>> GetAll([AsParameters] GetAllDevicesQuery query, GetAllDevicesHandler handler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await handler.Handle(query, cancellationToken));
    }

    private static async Task<Ok<DeviceResponseDto>> GetById(int id, GetDeviceByIdHandler handler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await handler.Handle(id, cancellationToken));
    }

    private static async Task<NoContent> Update(int id, DeviceUpdateDto dto, UpdateDeviceHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(id, dto, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> Retire(int id, RetireDeviceHandler handler, CancellationToken cancellationToken)
    {
        await handler.Handle(id, cancellationToken);
        return TypedResults.NoContent();
    }
}