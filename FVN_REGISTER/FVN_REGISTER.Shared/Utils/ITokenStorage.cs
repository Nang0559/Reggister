namespace FVN_REGISTER.Shared.Utils
{
    public interface ITokenStorage
    {
        Task SetTokenAsync(string token);
        Task<string?> GetTokenAsync();

        Task SetRefreshTokenAsync(string refreshToken);
        Task<string?> GetRefreshTokenAsync();

        Task RemoveTokenAsync();
        Task RemoveRefreshTokenAsync();
        Task ClearAsync();

        void MarkJsReady();
    }
}
