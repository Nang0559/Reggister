using FVN_REGISTER.Application.Interfaces.Trips;
using FVN_REGISTER.Core.Entities.Trips;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Trips;

public sealed class TripActualService : ITripActualService
{
    private readonly IUnitOfWork _uow;

    public TripActualService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task EnsureCreatedFromApprovedRequestAsync(int tripRequestId, CancellationToken ct = default)
    {
        var request = await _uow.Repository<F03TripRequest>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == tripRequestId && x.IsActive == true, ct);

        if (request == null || request.RequestStatus != ApprovalStatus.Approved)
            return;

        var actualRepository = _uow.Repository<F03TripActual>();
        var exists = await actualRepository.Query()
            .AnyAsync(x => x.TripRequestId == tripRequestId && x.IsActive == true, ct);

        if (exists)
            return;

        var actual = new F03TripActual
        {
            TripRequestId = request.Id,
            TripCode = request.TripCode,
            EmployeeCode = request.EmployeeCode,
            ActualStartDate = request.StartDate,
            ActualEndDate = request.EndDate,
            Destination = request.Destination,
            Purpose = request.Purpose,
            CustomerOrPartner = request.CustomerOrPartner,
            TransportMethod = request.TransportMethod,
            CompanionEmployeeCodes = request.CompanionEmployeeCodes,
            Accommodation = request.Accommodation,
            Note = request.Note,
            ActualStatus = "Scheduled",
            ApprovedAt = DateTime.Now,
            CreatedBy = request.CreatedBy,
            LastModifiedSource = "TRIP_APPROVAL"
        };

        await actualRepository.AddAsync(actual, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
