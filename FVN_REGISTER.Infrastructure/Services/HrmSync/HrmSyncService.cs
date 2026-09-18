using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.HrmSync;

public sealed class HrmSyncService : IHrmSyncService
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly object StateLock = new();
    private static HrmSyncRuntimeStatusDto _status = new();

    private readonly IHrmStagingImporterResolver _importers;
    private readonly IHrmSyncJobResolver _jobs;
    private readonly ILogger<HrmSyncService> _logger;

    public HrmSyncService(IHrmStagingImporterResolver importers, IHrmSyncJobResolver jobs, ILogger<HrmSyncService> logger)
    {
        _importers = importers;
        _jobs = jobs;
        _logger = logger;
    }

    public HrmSyncRuntimeStatusDto GetRuntimeStatus()
    {
        lock (StateLock) return CloneStatus(_status);
    }

    public Task<ServiceResult<HrmSyncRunResultDto>> RunAllAsync(string? triggeredBy = null, bool manual = true, CancellationToken ct = default)
        => ExecuteAsync(null, triggeredBy, manual, ct);

    public Task<ServiceResult<HrmSyncRunResultDto>> RunEntityAsync(string entityType, string? triggeredBy = null, CancellationToken ct = default)
        => string.IsNullOrWhiteSpace(entityType)
            ? Task.FromResult(ServiceResult<HrmSyncRunResultDto>.Fail("EntityType không được để trống."))
            : ExecuteAsync(entityType.Trim(), triggeredBy, true, ct);

    private async Task<ServiceResult<HrmSyncRunResultDto>> ExecuteAsync(string? entityType, string? triggeredBy, bool manual, CancellationToken ct)
    {
        if (!await Gate.WaitAsync(0, ct))
            return ServiceResult<HrmSyncRunResultDto>.Fail("Đang có một phiên đồng bộ HRM khác chạy. Vui lòng chờ phiên hiện tại hoàn tất.");

        var run = new HrmSyncRunResultDto
        {
            RunId = Guid.NewGuid(), Manual = manual, Running = true, StartedAt = DateTime.Now,
            TriggeredBy = string.IsNullOrWhiteSpace(triggeredBy) ? "SYSTEM" : triggeredBy
        };
        SetRunning(run);

        try
        {
            if (entityType == null)
            {
                foreach (var importer in _importers.GetAll())
                {
                    ct.ThrowIfCancellationRequested();
                    var count = await importer.ImportAsync(run.StartedAt.Date, ct);
                    _logger.LogInformation("[HRM-SYNC] Imported {Count} rows for {EntityType}.", count, importer.EntityType);
                }

                foreach (var job in _jobs.GetAll().OrderBy(x => x.SyncOrder).ThenBy(x => x.EntityType, StringComparer.OrdinalIgnoreCase))
                {
                    ct.ThrowIfCancellationRequested();
                    var result = await job.RunAsync(ct);
                    AddJobResult(run, job, result);
                    if (!result.Success && job.IsBlockingDependency)
                    {
                        run.Success = false;
                        run.Summary = $"Dừng tại job blocking '{job.EntityType}': {result.Summary}";
                        break;
                    }
                }
            }
            else
            {
                var importer = _importers.Resolve(entityType);
                var job = _jobs.Resolve(entityType);
                ct.ThrowIfCancellationRequested();
                var count = await importer.ImportAsync(run.StartedAt.Date, ct);
                _logger.LogInformation("[HRM-SYNC] Imported {Count} rows for {EntityType}.", count, entityType);
                var result = await job.RunAsync(ct);
                AddJobResult(run, job, result);
            }

            run.Success = run.Jobs.All(x => x.Success);
            run.Running = false;
            run.FinishedAt = DateTime.Now;
            if (string.IsNullOrWhiteSpace(run.Summary))
            {
                var ok = run.Jobs.Count(x => x.Success);
                run.Summary = $"Hoàn tất {run.Jobs.Count} job: {ok} thành công, {run.Jobs.Count - ok} lỗi.";
            }
            SetFinished(run);
            return ServiceResult<HrmSyncRunResultDto>.Ok(run);
        }
        catch (OperationCanceledException)
        {
            run.Running = false; run.Success = false; run.FinishedAt = DateTime.Now; run.Summary = "Phiên đồng bộ đã bị hủy.";
            SetFinished(run);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HRM-SYNC] Run failed. EntityType={EntityType}", entityType);
            run.Running = false; run.Success = false; run.FinishedAt = DateTime.Now; run.Summary = $"Đồng bộ thất bại: {ex.Message}";
            SetFinished(run);
            return ServiceResult<HrmSyncRunResultDto>.Ok(run);
        }
        finally { Gate.Release(); }
    }

    private static void AddJobResult(HrmSyncRunResultDto run, IHrmSyncJob job, HrmSyncResult result)
        => run.Jobs.Add(new HrmSyncJobRunDto
        {
            EntityType = job.EntityType, SyncOrder = job.SyncOrder, IsBlockingDependency = job.IsBlockingDependency,
            Success = result.Success, TotalSource = result.TotalSource, Added = result.Added, Updated = result.Updated,
            Deactivated = result.Deactivated, Unchanged = result.Unchanged, Superseded = result.Superseded,
            Summary = result.Summary, Errors = result.Errors.ToList()
        });

    private static void SetRunning(HrmSyncRunResultDto run)
    {
        lock (StateLock)
            _status = new HrmSyncRuntimeStatusDto { IsRunning = true, CurrentRunId = run.RunId, StartedAt = run.StartedAt, TriggeredBy = run.TriggeredBy };
    }

    private static void SetFinished(HrmSyncRunResultDto run)
    {
        lock (StateLock)
            _status = new HrmSyncRuntimeStatusDto { IsRunning = false, CurrentRunId = null, StartedAt = run.StartedAt, TriggeredBy = run.TriggeredBy, LastRun = CloneRun(run) };
    }

    private static HrmSyncRuntimeStatusDto CloneStatus(HrmSyncRuntimeStatusDto source)
        => new() { IsRunning = source.IsRunning, CurrentRunId = source.CurrentRunId, StartedAt = source.StartedAt, TriggeredBy = source.TriggeredBy, LastRun = source.LastRun == null ? null : CloneRun(source.LastRun) };

    private static HrmSyncRunResultDto CloneRun(HrmSyncRunResultDto source)
        => new()
        {
            RunId = source.RunId, Success = source.Success, Manual = source.Manual, Running = source.Running,
            StartedAt = source.StartedAt, FinishedAt = source.FinishedAt, TriggeredBy = source.TriggeredBy, Summary = source.Summary,
            Jobs = source.Jobs.Select(x => new HrmSyncJobRunDto
            {
                EntityType = x.EntityType, SyncOrder = x.SyncOrder, IsBlockingDependency = x.IsBlockingDependency,
                Success = x.Success, TotalSource = x.TotalSource, Added = x.Added, Updated = x.Updated,
                Deactivated = x.Deactivated, Unchanged = x.Unchanged, Superseded = x.Superseded,
                Summary = x.Summary, Errors = x.Errors.ToList()
            }).ToList()
        };
}
