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
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<DeviceResponseDto>> GetById(
        [AsParameters] int id, 
        [FromServices]  IMediator mediator,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await mediator.Send(new GetDeviceByIdQuery(id), cancellationToken));
    }
    
    private static async Task<CreatedAtRoute<DeviceResponseDto>> Create(
        [FromBody] DeviceCreateDto dto, 
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateDeviceCommand(dto.Name, dto.HourRentPrice, dto.CategoryId);
        var device = await mediator.Send(command, cancellationToken);
        return TypedResults.CreatedAtRoute(device, routeName:"GetDeviceById", routeValues: new {id = device.Id});
    }

    private static async Task<NoContent> Update(
        [AsParameters] int id, 
        [FromBody] DeviceUpdateDto dto, 
        [FromServices]  IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDeviceCommand(id, dto.Name, dto.HourRentPrice, dto.CategoryId);
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> Retire(
        [AsParameters] int id, 
        [FromServices] IMediator mediator, 
        CancellationToken cancellationToken)
    {
        await mediator.Send(new RetireDeviceCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}