# Feature Operator Assignment Security

## Purpose

F03FeatureOperatorAssignments defines who is allowed to operate a concrete application resource after RBAC has granted the capability.

It is intentionally separate from:
- F03Functions / F03RoleFunctions: capability (what can be done).
- F03ManagedScopes: organizational data scope.
- F03ApprovalPolicies / F03Approvers: approval workflow.
- F03PublicFormAudiences: who may submit a public form.

## Supported resources

| Function | ResourceType | ResourceId |
|---|---|---|
| 2801 PublicInformation.Manage | PUBLIC_INFORMATION | F03PublicInformation.Id |
| 2802 Execution.Review | EXECUTION_REVIEW | NULL (module-level operator) |
| 2807 PublicForm.Manage | PUBLIC_FORM | F03PublicForms.Id |
| 2808 PublicForm.SubmissionView | PUBLIC_FORM | F03PublicForms.Id |
| 2809 PublicForm.Export | PUBLIC_FORM | F03PublicForms.Id |

## Authorization order

1. The user must have the function capability through RBAC.
2. If resource-specific operator assignments exist, the user's HRM EmployeeCode must be assigned to that resource.
3. Otherwise, a global assignment (ResourceId IS NULL) is checked.
4. If no assignment exists at either level, existing RBAC + scope behavior remains effective for backward compatibility.

Employee identity, department and position are resolved from HRM; the assignment table stores only EmployeeCode.

## Examples

### HR feedback
Assign E0005 and E0012 to 2802 / EXECUTION_REVIEW / NULL. Both employees can process HR feedback only if they also have Execution.Review.

### Public Information
Assign E0005 to 2801 / PUBLIC_INFORMATION / 15. Only E0005 can edit/publish/archive information record 15 when resource assignment is configured.

### Public Form
The audience answers who can register. Operator assignment answers who can manage the form/submissions.

Example: Audience = AllCompany; 2807 E0005 manages form 12; 2808 E0005 views submissions for form 12; 2809 E0012 exports submissions for form 12.

## Security Center
The Security Center exposes operator assignment with HRM employee lookup showing EmployeeCode — EmployeeName — DeptCode — PositionCode.
No employee name, department name, position name or email supplied by the client is trusted.