using FVN_REGISTER.Contract.Dtos.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FVN_REGISTER.API.Attributes
{
    public class AppAuthorizeAttribute : ActionFilterAttribute
    {
        public bool RequireLogin { get; set; } = true;
        public int[] RequireFunctions { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Trong Web API, chúng ta lấy thông tin user từ HttpContext.Items 
            // (Thường được set bởi JwtMiddleware hoặc SessionMiddleware)
            var user = context.HttpContext.Items["User"] as AuthResultDto;

            // 1. Kiểm tra đăng nhập
            if (RequireLogin && user == null)
            {
                // API không redirect, mà trả về 401
                context.Result = new JsonResult(new { message = "Unauthorized" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            // 2. Kiểm tra quyền (Function)
            if (RequireFunctions != null && RequireFunctions.Length > 0)
            {
                // Giả sử UserSessionDto của bạn có List<int> Functions
                var userFunctions = context.HttpContext.Items["UserFunctions"] as List<int>;

                var hasPermission = userFunctions != null &&
                                   RequireFunctions.Any(f => userFunctions.Contains(f));

                if (!hasPermission)
                {
                    // Trả về 403 Forbidden
                    context.Result = new JsonResult(new { message = "Forbidden: Bạn không có quyền thực hiện hành động này" })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
        