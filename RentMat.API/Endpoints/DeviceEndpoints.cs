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
            .Produces<PagedResponse<DeviceResponseDto>>()
            .Produces(400);

        group.MapGet("/{id:int}", GetById)
            .WithName("GetDeviceById")
            .WithSummary("Returns a device by id")
            .Produces<DeviceResponseDto>()
            .Produces(400);

        group.MapPut("/{id:int}", Update)
            .RequireAuthorization(Policies.AdminOnly)
            .WithName("UpdateDevice")
            .WithSummary("Updates a device")
            .Produces(204)
            .Produces(400)
            .Produces(403)
            .Produces(404);

        group.MapPut("/{id:int}/retire", Retire)
            .RequireAuthorization(Policies.AdminOnly)
            .WithName("RetireDevice")
            .WithSummary("Changes a device status to retired")
            .Produces(204)
            .Produces(400)
            .Produces(403)
            .Produces(404);
    }

    private static async Task<IResult> GetAll([AsParameters] GetAllDevicesQuery query, GetAllDevicesHandler handler,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await handler.Handle(query, cancellationToken));
    }

    private static async Task<IResult> GetById(int id, GetDeviceByIdHandler handler,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await handler.Handle(id, cancellationToken));
    }

    private static async Task<IResult> Update(int id, DeviceUpdateDto dto, UpdateDeviceHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(id, dto, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> Retire(int id, RetireDeviceHandler handler, CancellationToken cancellationToken)
    {
        await handler.Handle(id, cancellationToken);
        return Results.NoContent();
    }
}