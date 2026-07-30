using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RentMat.API.Common.Security;
using RentMat.Application.Commands.Devices;
using RentMat.Application.Common;
using RentMat.Application.DTOs.Device;
using RentMat.Application.Handlers.Devices;
using RentMat.Application.Queries.Devices;
using RentMat.Application.Queries.Users;

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
            .ProducesValidationProblem();

        group.MapGet("/{id:int}", GetById)
            .WithName("GetDeviceById")
            .WithSummary("Returns a device by id")
            .ProducesValidationProblem();
        
        group.MapPost("/create", Create)
            .RequireAuthorization(Policies.AdminOnly)
            .WithName("CreateDevice")
            .WithSummary("Creates a device")
            .ProducesValidationProblem()
            .ProducesProblem(404);
        
        group.MapPut("/{id:int}", Update)
            .RequireAuthorization(Policies.AdminOnly)
            .WithName("UpdateDevice")
            .WithSummary("Updates a device")
            .ProducesValidationProblem()
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404);

        group.MapPut("/{id:int}/retire", Retire)
            .RequireAuthorization(Policies.AdminOnly)
            .WithName("RetireDevice")
            .WithSummary("Changes a device status to retired")
            .ProducesValidationProblem()
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404);
    }

    private static async Task<Ok<PagedResponse<DeviceResponseDto>>> GetAll(
        [AsParameters] GetAllDevicesQuery query, 
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<DeviceResponseDto>> GetById(
        int id, 
        [FromServices]  GetDeviceByIdQueryHandler queryHandler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await queryHandler.Handle(new GetDeviceByIdQuery(id), cancellationToken));
    }
    
    private static async Task<CreatedAtRoute<DeviceResponseDto>> Create(
        DeviceCreateDto dto, 
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateDeviceCommand(dto.Name, dto.HourRentPrice, dto.CategoryId);
        var device = await sender.Send(command, cancellationToken);
        return TypedResults.CreatedAtRoute(device, routeName:"GetDeviceById", routeValues: new {id = device.Id});
    }

    private static async Task<NoContent> Update(
        int id, 
        DeviceUpdateDto dto, 
        [FromServices]  ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDeviceCommand(id, dto.Name, dto.HourRentPrice, dto.CategoryId);
        await sender.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> Retire(
        int id, 
        [FromServices] RetireDeviceCommandHandler commandHandler, 
        CancellationToken cancellationToken)
    {
        await commandHandler.Handle(new RetireDeviceCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}