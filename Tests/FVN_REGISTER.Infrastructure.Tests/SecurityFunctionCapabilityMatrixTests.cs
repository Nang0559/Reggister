using FVN_REGISTER.Core.Constants;
using Xunit;

namespace FVN_REGISTER.Infrastructure.Tests;

public sealed class SecurityFunctionCapabilityMatrixTests
{
    [Fact]
    public void AttendanceCalculate_IsSeparateFromAttendanceViewAndExport()
    {
        Assert.NotEqual(SecurityFunctionCodes.AttendanceView, SecurityFunctionCodes.AttendanceCalculate);
        Assert.NotEqual(SecurityFunctionCodes.AttendanceExport, SecurityFunctionCodes.AttendanceCalculate);
    }

    [Fact]
    public void EquipmentCancel_IsSeparateCapability()
    {
        Assert.NotEqual(SecurityFunctionCodes.EquipmentEdit, SecurityFunctionCodes.EquipmentCancel);
        Assert.NotEqual(SecurityFunctionCodes.EquipmentApprove, SecurityFunctionCodes.EquipmentCancel);
    }

    [Fact]
    public void AdministrativeCapabilities_AreInDedicated3000Range()
    {
        Assert.Equal(3001, SecurityFunctionCodes.DepartmentView);
        Assert.Equal(3021, SecurityFunctionCodes.LeaveTypeView);
        Assert.Equal(3031, SecurityFunctionCodes.ApproverView);
        Assert.Equal(3041, SecurityFunctionCodes.WorkCalendarView);
        Assert.Equal(3051, SecurityFunctionCodes.DepartmentStatusView);
        Assert.Equal(3061, SecurityFunctionCodes.OTLimitManage);
        Assert.Equal(3071, SecurityFunctionCodes.ApprovalPolicyManage);
        Assert.Equal(3072, SecurityFunctionCodes.HrmUserRoleRuleManage);
        Assert.Equal(3081, SecurityFunctionCodes.EmailQueueManage);
        Assert.Equal(3082, SecurityFunctionCodes.EmailTemplateManage);
    }

    [Fact]
    public void HrAndItRoles_AreCanonicalAndLegacyApproverIsObsolete()
    {
        Assert.Equal(7, (int)UserRole.HR);
        Assert.Equal(8, (int)UserRole.IT);
        Assert.Equal(7, UserPermissionCodes.HR);
        Assert.Equal(8, UserPermissionCodes.IT);
        Assert.Equal(4, (int)UserRole.Approver);
    }

    [Fact]
    public void EquipmentActionCapabilities_AreDistinct()
    {
        var codes = new[]
        {
            SecurityFunctionCodes.EquipmentAssign,
            SecurityFunctionCodes.EquipmentTransfer,
            SecurityFunctionCodes.EquipmentReturn,
            SecurityFunctionCodes.EquipmentLiquidate,
            SecurityFunctionCodes.EquipmentQR,
            SecurityFunctionCodes.EquipmentHistory
        };

        Assert.Equal(6, codes.Distinct().Count());
        Assert.All(codes, code => Assert.InRange(code, 2309, 2314));
    }

    [Fact]
    public void CalendarView_IsSeparateFromWorkCalendarManage()
    {
        Assert.NotEqual(
            SecurityFunctionCodes.CalendarView,
            SecurityFunctionCodes.WorkCalendarManage);
    }

    [Fact]
    public void NewCapabilities_AreUnique()
    {
        var codes = new[]
        {
            SecurityFunctionCodes.DepartmentView,
            SecurityFunctionCodes.EmployeeView,
            SecurityFunctionCodes.LeaveTypeView,
            SecurityFunctionCodes.ApproverView,
            SecurityFunctionCodes.WorkCalendarView,
            SecurityFunctionCodes.DepartmentStatusView,
            SecurityFunctionCodes.OTLimitManage,
            SecurityFunctionCodes.ApprovalPolicyManage,
            SecurityFunctionCodes.HrmUserRoleRuleManage,
            SecurityFunctionCodes.EmailQueueManage,
            SecurityFunctionCodes.EmailTemplateManage,
            SecurityFunctionCodes.AttendanceCalculate,
            SecurityFunctionCodes.EquipmentCancel
        };

        Assert.Equal(codes.Length, codes.Distinct().Count());
    }
}