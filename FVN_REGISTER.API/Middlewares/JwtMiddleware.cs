

using FVN_REGISTER.Application.Interfaces.Infrastructure;

namespace FVN_REGISTER.Infrastructure.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        //private readonly IJwtService _jwtService;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;

        }

        public async Task InvokeAsync(HttpContext context)
        {
            var jwtService = context.RequestServices.GetRequiredService<IJwtService>();
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                var principal = jwtService.ValidateToken(token);
                if (principal != null)
                {
                    context.User = principal;
                }
            }

            await _next(context);
        }
    }
}
