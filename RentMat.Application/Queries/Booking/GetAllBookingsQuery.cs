using MediatR;
using RentMat.Application.Common;
using RentMat.Application.DTOs.RentalBooking;
using RentMat.Core.Enums;

namespace RentMat.Application.Queries.Booking;

public record GetAllBookingsQuery(int Page = 1, int PageSize = 10, string? Search = null, BookingStatus? Status = null)
    : IRequest<PagedResponse<BookingResponseDto>>;