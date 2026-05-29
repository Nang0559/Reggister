using FVN_REGISTER.Shared.Utils;
using Microsoft.Maui.Storage;


namespace FVN_REGISTER.Services
{
    using FVN_REGISTER.Contract.Utils;
    using Microsoft.Maui.Storage;

    public class SecureTokenService : ITokenStorage
    {
      

        public async Task SetTokenAsync(string token)
        {
            // Tự động mã hóa và lưu vào Keychain (iOS) hoặc Keystore (Android)
            await SecureStorage.Default.SetAsync(AuthConstants.TokenKey, token);
        }

        public async Task<string?> GetTokenAsync()
        {
            // Lấy token ra
            return await SecureStorage.Default.GetAsync(AuthConstants.TokenKey);
        }

        public async Task RemoveTokenAsync()
        {
            // Xóa token khi Logout
            SecureStorage.Default.Remove(AuthConstants.TokenKey);
            // Hoặc xóa tất cả nếu cần: SecureStorage.Default.RemoveAll();
            await Task.CompletedTask;
        }
        public void MarkJsReady()
        {
           
        }
    }
}
