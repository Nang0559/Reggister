namespace FVN_REGISTER.Application.Interfaces.Trips;

public interface ITripActualService
{
    Task EnsureCreatedFromApprovedRequestAsync(int tripRequestId, CancellationToken ct = default);
}
