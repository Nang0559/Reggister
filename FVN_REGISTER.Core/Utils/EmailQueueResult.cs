namespace FVN_REGISTER.Core.Utils;

public class EmailQueueResult
{
    public bool IsSuccess { get; set; }
    public bool IsTemplateNotFound { get; set; }
    public bool IsSkipped { get; set; }
    public string? Message { get; set; }

    public static EmailQueueResult Ok(string? message = null)
        => new() { IsSuccess = true, Message = message };

    public static EmailQueueResult TemplateNotFound(string templateCode)
        => new()
        {
            IsSuccess = false,
            IsTemplateNotFound = true,
            Message = $"Template email '{templateCode}' chưa được khai báo hoặc chưa kích hoạt."
        };

    public static EmailQueueResult Skipped(string message)
        => new() { IsSuccess = true, IsSkipped = true, Message = message };

    public static EmailQueueResult Fail(string message)
        => new() { IsSuccess = false, Message = message };
}
