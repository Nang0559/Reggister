

namespace FVN_REGISTER.Shared.Utils
{
    public interface ITokenStorage
    {
        Task SetTokenAsync(string token);
        Task<string?> GetTokenAsync();
        Task RemoveTokenAsync();
        void MarkJsReady();
    }
}
