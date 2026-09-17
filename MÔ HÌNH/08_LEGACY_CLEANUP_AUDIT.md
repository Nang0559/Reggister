# 08 — Legacy Architecture Cleanup Audit

## Scope

The leave-registration UI still contained references to the removed `Contract.ViewModels` architecture after the API/Application migration. The cleanup keeps the current boundary:

```text
Blazor Shared
    ↓
Contract DTOs / Application-facing client contracts
    ↓
API Controllers
    ↓
Application interfaces
    ↓
Infrastructure implementations
    ↓
EF / database
```

The Shared UI does not depend on legacy ViewModels or persistence entities.

## Cleaned leave module

- `FVN_REGISTER.Shared/Services/Leaves/ILeaveCreateClientService.cs`
- `FVN_REGISTER.Shared/Services/Leaves/LeaveCreateClientService.cs`
- `FVN_REGISTER.Shared/Services/Leaves/ILeaveHistorysClientService.cs`
- `FVN_REGISTER.Shared/Services/Leaves/LeaveHistorysClientService.cs`
- `FVN_REGISTER.Shared/Services/Leaves/ILeaveTypeClientService.cs`
- `FVN_REGISTER.Shared/Services/Leaves/LeaveTypeClientService.cs`
- `FVN_REGISTER.Shared/Pages/LeaveCreate.razor`
- `FVN_REGISTER.Shared/Pages/LeaveCreate.razor.cs`
- `FVN_REGISTER.Shared/Pages/LeaveHistory.razor` remains routed through the generic current `HistoryPage` flow.
- `FVN_REGISTER.Shared/Pages/LeaveTypes/LeaveTypeManagement.razor`
- `FVN_REGISTER.Shared/Components/Leaves/LeaveListSection.razor`
- `FVN_REGISTER.Shared/Dialogs/LeaveAddDialog.razor`
- `FVN_REGISTER.Shared/Dialogs/LeaveDetailDialog.razor`
- `FVN_REGISTER.API/Controllers/LeaveTypeManagementController.cs`

## New/current contracts

- `FVN_REGISTER.Models/Dtos/Leaves/LeaveCalendarEventDto.cs`
- `SystemMasterDataDto` now exposes typed `LeaveTypes` and `LeaveEvents`.
- `LeaveRequestUpsertDto` is the create command contract.
- `LeaveRequestDetailUpsertDto` is the per-day create contract.
- `LeaveSummaryDto` is the history/list projection.
- `LeaveTypeDto` / `LeaveTypeUpsertDto` are the leave-type management contracts.

## Approval boundary

The create UI only displays the approval hierarchy returned by the current Application/approval provider. It does not submit approver ViewModels or manually construct approval snapshots. Approval state remains owned by the server-side approval workflow.

## Verification status

The repository was edited directly on `main`. A local `dotnet build` has not been claimed because the execution environment cannot resolve `github.com` for a repository clone. The next verification step is a clean restore/build of the solution plus Blazor/API contract compilation and DI validation.
