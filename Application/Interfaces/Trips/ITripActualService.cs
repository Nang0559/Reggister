namespace FVN_REGISTER.Application.Interfaces.Trips;

public interface ITripActualService
{
    /// <summary>
    /// Creates the execution record once a trip request reaches Approved.
    /// Idempotent: repeated approval callbacks never create a duplicate actual.
    /// </summary>
    Task EnsureCreatedFromApprovedRequestAsync(int tripRequestId, CancellationToken ct = default);
}
