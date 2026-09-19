# Dashboard Capability Architecture

## Runtime

JWT -> Current User -> IAuthorizationService -> Effective Permission Snapshot -> DashboardOrchestrator -> allowed providers -> DashboardResponse -> Blazor Web.

DashboardOrchestrator now checks RequiredFunctionCode before invoking a provider. Unauthorized module providers therefore do not execute their data queries.

## Provider contract

Each IModuleDashboardProvider declares RequiredFunctionCode.

Current providers:
- Leave -> SecurityFunctionCodes.LeaveView
- OT -> SecurityFunctionCodes.OTView

The dashboard itself requires DashboardView.

## Target dashboard

Personal:
- Leave
- OT
- Trip
- Equipment
- Notifications

Workflow:
- Leave approval
- OT approval
- Trip approval
- Equipment approval

Department:
- employee/absence statistics
- OT status
- pending approvals

Administration:
- HRM sync status
- Security Center
- system health

Future Trip and Equipment dashboard providers should follow the same capability contract instead of adding module-specific branches to DashboardOrchestrator.
