// Compatibility bridge while the Infrastructure project finishes its namespace migration.
// Canonical entity definitions remain in FVN_REGISTER.Core.Entities.*.

global using FVNWEBAPPContext = FVN_REGISTER.Infrastructure.Models.Data.FVNWEBAPPContext;
global using F03AppNotification = FVN_REGISTER.Core.Entities.Common.F03AppNotification;
global using F03Attachment = FVN_REGISTER.Core.Entities.Common.F03Attachment;
global using F03AuditLog = FVN_REGISTER.Core.Entities.Common.F03AuditLog;
global using F03BusinessRule = FVN_REGISTER.Core.Entities.Common.F03BusinessRule;
global using F03CompanyHoliday = FVN_REGISTER.Core.Entities.Common.F03CompanyHoliday;
global using F03EmailLog = FVN_REGISTER.Core.Entities.Common.F03EmailLog;
global using F03EmailProfile = FVN_REGISTER.Core.Entities.Common.F03EmailProfile;
global using F03EmailQueue = FVN_REGISTER.Core.Entities.Common.F03EmailQueue;
global using F03EmailTemplate = FVN_REGISTER.Core.Entities.Common.F03EmailTemplate;
global using F03EscalationLog = FVN_REGISTER.Core.Entities.Common.F03EscalationLog;
global using F03EscalationRule = FVN_REGISTER.Core.Entities.Common.F03EscalationRule;
global using F03UserLog = FVN_REGISTER.Core.Entities.Common.F03UserLog;
global using F03WorkYear = FVN_REGISTER.Core.Entities.Common.F03WorkYear;
global using F03ApprovalStepSnapshot = FVN_REGISTER.Core.Entities.Common.F03ApprovalStepSnapshot;
global using F03ApprovalStep = FVN_REGISTER.Core.Entities.Approvers.F03ApprovalStep;
global using F03Approver = FVN_REGISTER.Core.Entities.Approvers.F03Approver;
global using F03Department = FVN_REGISTER.Core.Entities.HR.F03Department;
global using F03Employee = FVN_REGISTER.Core.Entities.HR.F03Employee;
global using F03Gender = FVN_REGISTER.Core.Entities.HR.F03Gender;
global using F03Position = FVN_REGISTER.Core.Entities.HR.F03Position;
global using F03AttendanceStaging = FVN_REGISTER.Core.Entities.Leaves.F03AttendanceStaging;
global using F03LeaveBalance = FVN_REGISTER.Core.Entities.Leaves.F03LeaveBalance;
global using F03LeaveDay = FVN_REGISTER.Core.Entities.Leaves.F03LeaveDay;
global using F03leaveDay = FVN_REGISTER.Core.Entities.Leaves.F03LeaveDay;
global using F03LeaveDayDetail = FVN_REGISTER.Core.Entities.Leaves.F03LeaveDayDetail;
global using F03LeaveType = FVN_REGISTER.Core.Entities.Leaves.F03LeaveType;
global using F03StagingLeave = FVN_REGISTER.Core.Entities.Leaves.F03StagingLeave;
global using F03OTEmployee = FVN_REGISTER.Core.Entities.OT.F03OTEmployee;
global using F03OTLimitRule = FVN_REGISTER.Core.Entities.OT.F03OTLimitRule;
global using F03OTReasonCode = FVN_REGISTER.Core.Entities.OT.F03OTReasonCode;
global using F03OTRequest = FVN_REGISTER.Core.Entities.OT.F03OTRequest;
global using F03StagingOT = FVN_REGISTER.Core.Entities.OT.F03StagingOT;
global using F03Function = FVN_REGISTER.Core.Entities.Security.F03Function;
global using F03Permission = FVN_REGISTER.Core.Entities.Security.F03Permission;
global using F03User = FVN_REGISTER.Core.Entities.Security.F03User;
global using F03UserFunction = FVN_REGISTER.Core.Entities.Security.F03UserFunction;
global using F03UserSession = FVN_REGISTER.Core.Entities.Security.F03UserSession;
global using F03StagingTrip = FVN_REGISTER.Core.Entities.Trips.F03StagingTrip;
global using LeaveApprovalSubject = FVN_REGISTER.Application.Models.Subjects.LeaveRequestSubject;
global using IHistoryDispatcher = FVN_REGISTER.Application.Interfaces.Histories.IHistoryDispatcher;
global using IHistoryHandler = FVN_REGISTER.Application.Interfaces.Histories.IHistoryHandler;
global using IApprovalSubject = FVN_REGISTER.Application.Interfaces.Approvals.IApprovalSubject;
global using Microsoft.Extensions.DependencyInjection;

namespace FVN_REGISTER.Infrastructure.Models.Entities { }
namespace FVN_REGISTER.Infrastructure.Models.Entities.Common { }
namespace FVN_REGISTER.Infrastructure.Models.Entities.Approvers { }
namespace FVN_REGISTER.Infrastructure.Models.Entities.HR { }
namespace FVN_REGISTER.Infrastructure.Models.Entities.Leaves { }
namespace FVN_REGISTER.Infrastructure.Models.Entities.OT { }
namespace FVN_REGISTER.Infrastructure.Models.Entities.Security { }
namespace FVN_REGISTER.Infrastructure.Models.Entities.Trips { }
