using RentMat.Core.Enums;

namespace RentMat.Application.Queries;

public record GetAllDevicesQuery(int Page = 1, int PageSize = 10, string? Search = null, DeviceStatus? Status = null);