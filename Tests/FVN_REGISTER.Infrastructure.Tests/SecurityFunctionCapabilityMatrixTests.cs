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
    public void AttendanceViewOwn_IsOwnCapabilitySeparateFromDepartmentAttendanceView()
    {
        Assert.Equal(2912, SecurityFunctionCodes.AttendanceViewOwn);
        Assert.NotEqual(SecurityFunctionCodes.AttendanceView, SecurityFunctionCodes.AttendanceViewOwn);
        Assert.NotEqual(SecurityFunctionCodes.AttendanceExport, SecurityFunctionCodes.AttendanceViewOwn);
        Assert.NotEqual(SecurityFunctionCodes.AttendanceCalculate, SecurityFunctionCodes.AttendanceViewOwn);
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
            SecurityFunctionCodes.AttendanceViewOwn,
            SecurityFunctionCodes.EquipmentCancel
        };

        Assert.Equal(codes.Length, codes.Distinct().Count());
    }
}