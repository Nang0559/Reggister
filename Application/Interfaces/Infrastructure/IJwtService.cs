

using FVN_REGISTER.Contract.Dtos.Authentication;
using System.Security.Claims;


namespace FVN_REGISTER.Application.Interfaces.Infrastructure
{
   
        public interface IJwtService
        {
          
            string GenerateToken(AuthResultDto user, TimeSpan? expires = null);

            ClaimsPrincipal? ValidateToken(string token);
        }
    
}
