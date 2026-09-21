using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Services.Execution;

public static class ExecutionNotificationModuleMapper
{
    public static RequestModule ToRequestModule(string moduleCode)
    {
        switch (moduleCode?.Trim().ToUpperInvariant())
        {
            case "OT":
                return RequestModule.Overtime;
            case "LEAVE":
                return RequestModule.Leave;
            case "TRIP":
                return RequestModule.Trip;
            case "EQUIPMENT":
                return RequestModule.Equipment;
            case "ATTENDANCE":
                return RequestModule.Attendance;
            default:
                throw new InvalidOperationException(
                    $"Không có mapping Notification Module cho Execution ModuleCode '{moduleCode}'.");
        }
    }
}
