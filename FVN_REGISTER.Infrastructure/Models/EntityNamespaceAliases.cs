// Transitional namespace aliases for infrastructure code that still imports the pre-migration
// FVN_REGISTER.Infrastructure.Models.Entities.* namespaces. The canonical entity types live
// in FVN_REGISTER.Core.Entities.*; no duplicate entity models are introduced.

namespace FVN_REGISTER.Infrastructure.Models.Entities.Common
{
    using F03AppNotification = FVN_REGISTER.Core.Entities.Common.F03AppNotification;
    using F03Attachment = FVN_REGISTER.Core.Entities.Common.F03Attachment;
    using F03AuditLog = FVN_REGISTER.Core.Entities.Common.F03AuditLog;
    using F03BusinessRule = FVN_REGISTER.Core.Entities.Common.F03BusinessRule;
    using F03CompanyHoliday = FVN_REGISTER.Core.Entities.Common.F03CompanyHoliday;
    using F03EmailLog = FVN_REGISTER.Core.Entities.Common.F03EmailLog;
    using F03EmailProfile = FVN_REGISTER.Core.Entities.Common.F03EmailProfile;
    using F03EmailQueue = FVN_REGISTER.Core.Entities.Common.F03EmailQueue;
    using F03EmailTemplate = FVN_REGISTER.Core.Entities.Common.F03EmailTemplate;
    using F03EscalationLog = FVN_REGISTER.Core.Entities.Common.F03EscalationLog;
    using F03EscalationRule = FVN_REGISTER.Core.Entities.Common.F03EscalationRule;
    using F03UserLog = FVN_REGISTER.Core.Entities.Common.F03UserLog;
    using F03WorkYear = FVN_REGISTER.Core.Entities.Common.F03WorkYear;
    using F03ApprovalStepSnapshot = FVN_REGISTER.Core.Entities.Common.F03ApprovalStepSnapshot;
}

namespace FVN_REGISTER.Infrastructure.Models.Entities.Approvers
{
    using F03ApprovalStep = FVN_REGISTER.Core.Entities.Approvers.F03ApprovalStep;
    using F03Approver = FVN_REGISTER.Core.Entities.Approvers.F03Approver;
}

namespace FVN_REGISTER.Infrastructure.Models.Entities.HR
{
    using F03Department = FVN_REGISTER.Core.Entities.HR.F03Department;
    using F03Employee = FVN_REGISTER.Core.Entities.HR.F03Employee;
    using F03Gender = FVN_REGISTER.Core.Entities.HR.F03Gender;
    using F03Position = FVN_REGISTER.Core.Entities.HR.F03Position;
}

namespace FVN_REGISTER.Infrastructure.Models.Entities.Leaves
{
    using F03AttendanceStaging = FVN_REGISTER.Core.Entities.Leaves.F03AttendanceStaging;
    using F03LeaveBalance = FVN_REGISTER.Core.Entities.Leaves.F03LeaveBalance;
    using F03LeaveDay = FVN_REGISTER.Core.Entities.Leaves.F03LeaveDay;
    using F03leaveDay = FVN_REGISTER.Core.Entities.Leaves.F03LeaveDay;
    using F03LeaveDayDetail = FVN_REGISTER.Core.Entities.Leaves.F03LeaveDayDetail;
    using F03LeaveType = FVN_REGISTER.Core.Entities.Leaves.F03LeaveType;
    using F03StagingLeave = FVN_REGISTER.Core.Entities.Leaves.F03StagingLeave;
}

namespace FVN_REGISTER.Infrastructure.Models.Entities.OT
{
    using F03OTEmployee = FVN_REGISTER.Core.Entities.OT.F03OTEmployee;
    using F03OTLimitRule = FVN_REGISTER.Core.Entities.OT.F03OTLimitRule;
    using F03OTReasonCode = FVN_REGISTER.Core.Entities.OT.F03OTReasonCode;
    using F03OTRequest = FVN_REGISTER.Core.Entities.OT.F03OTRequest;
    using F03StagingOT = FVN_REGISTER.Core.Entities.OT.F03StagingOT;
}

namespace FVN_REGISTER.Infrastructure.Models.Entities.Security
{
    using F03Function = FVN_REGISTER.Core.Entities.Security.F03Function;
    using F03Permission = FVN_REGISTER.Core.Entities.Security.F03Permission;
    using F03User = FVN_REGISTER.Core.Entities.Security.F03User;
    using F03UserFunction = FVN_REGISTER.Core.Entities.Security.F03UserFunction;
    using F03UserSession = FVN_REGISTER.Core.Entities.Security.F03UserSession;
}

namespace FVN_REGISTER.Infrastructure.Models.Entities.Trips
{
    using F03StagingTrip = FVN_REGISTER.Core.Entities.Trips.F03StagingTrip;
}
