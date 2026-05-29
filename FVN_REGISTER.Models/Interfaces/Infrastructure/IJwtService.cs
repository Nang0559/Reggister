
using FVN_REGISTER.Contract.ViewModels;
using System.Security.Claims;


namespace FVN_REGISTER.Contract.Interfaces.Infrastructure
{
   
        public interface IJwtService
        {
          
            string GenerateToken(UserSessionDto user, TimeSpan? expires = null);

            ClaimsPrincipal? ValidateToken(string token);
        }
    
}
