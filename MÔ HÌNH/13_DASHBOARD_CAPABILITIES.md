# Dashboard Capability Architecture

## Runtime

```
JWT
  -> Current User
  -> IAuthorizationService
  -> Effective Permission Snapshot
  -> DashboardOrchestrator
  -> allowed providers
  -> DashboardResponse
  -> Blazor Web
```

DashboardOrchestrator checks each provider's `RequiredFunctionCode` before invoking it. An unauthorized provider therefore must not execute its data query.

## Provider contract

Each `IModuleDashboardProvider` declares a required security function.

The current codebase contains dashboard providers for:

- Leave -> `SecurityFunctionCodes.LeaveView`
- OT -> `SecurityFunctionCodes.OTView`
- Trip
- Equipment

Trip and Equipment are therefore no longer described as merely future provider implementations. Their actual UI exposure and capability behavior still require runtime certification in Phase 10.

The dashboard itself requires `DashboardView`.

## Target capability areas

### Personal

- Leave
- OT
- Trip
- Equipment
- Notifications

### Workflow

- Leave approval
- OT approval
- Trip approval
- Equipment approval

### Department

- Employee/absence statistics
- OT status
- Pending approvals

### Administration

- HRM sync status
- Security Center
- System health

## Certification rule

Documentation does not treat a provider as production-ready merely because its class exists.

For each provider Phase 10 must verify:

1. RequiredFunctionCode.
2. Permission evaluation.
3. Data scope.
4. Query execution.
5. Dashboard response.
6. UI rendering.
7. Empty/error state.
8. Export/report relationship where applicable.

No provider may bypass the central authorization/data-scope contract by adding a module-specific authorization branch to `DashboardOrchestrator`.

See `MÔ HÌNH/23_PRODUCT_READINESS_PLAN.md` for the complete production readiness sequence.
