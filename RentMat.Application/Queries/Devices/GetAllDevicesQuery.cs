using MediatR;
using RentMat.Application.Common;
using RentMat.Application.DTOs.Device;
using RentMat.Core.Enums;

namespace RentMat.Application.Queries.Devices;

public record GetAllDevicesQuery(int Page = 1, int PageSize = 10, string? Search = null, DeviceStatus? Status = null)
    : IRequest<PagedResponse<DeviceResponseDto>>;
