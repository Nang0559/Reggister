using FVN_REGISTER.Application.Interfaces.Trips;
using FVN_REGISTER.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Trips;

public sealed class TripActualService : ITripActualService
{
    private readonly FVNWEBAPPContext _db;

    public TripActualService(FVNWEBAPPContext db)
    {
        _db = db;
    }

    public async Task EnsureCreatedFromApprovedRequestAsync(int tripRequestId, CancellationToken ct = default)
    {
        var request = await _db.TripRequests.AsNoTracking()
            .Where(x => x.Id == tripRequestId && x.IsActive == true)
            .Select(x => new
            {
                x.Id,
                x.EmployeeCode,
                x.StartDate,
                x.EndDate,
                x.RequestStatus
            })
            .SingleOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"Không tìm thấy TripRequestId={tripRequestId}.");

        if (request.RequestStatus != FVN_REGISTER.Core.Enums.ApprovalStatus.Approved)
            throw new InvalidOperationException("Chỉ Trip đã Approved mới được tạo TripActual.");

        await _db.Database.ExecuteSqlInterpolatedAsync($@"
IF NOT EXISTS (SELECT 1 FROM dbo.F03TripActual WHERE TripRequestId = {request.Id})
BEGIN
    INSERT dbo.F03TripActual
    (
        TripRequestId, EmployeeCode, ActualStartDate, ActualEndDate,
        Status, IsActive, CreatedBy, CreatedAt
    )
    VALUES
    (
        {request.Id}, {request.EmployeeCode}, {request.StartDate},
        {request.EndDate}, N'Scheduled', 1, 0, GETDATE()
    );
END", ct);
    }
}
