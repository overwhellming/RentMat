using RentMat.Core.Enums;

namespace RentMat.Application.Queries;

public record GetAllBookingsQuery(int Page = 1, int PageSize = 10, string? Search = null, BookingStatus? Status = null);