using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Shared.Services.Users
{
    public interface IAuthClientService
    {
        /// <summary>
        /// Đăng nhập hệ thống và khởi tạo phiên làm việc
        /// </summary>
        Task<ApiResponse<AuthResultDto>> Login(string username, string password, CancellationToken ct = default);

        /// <summary>
        /// Đăng xuất và xóa sạch dữ liệu phiên làm việc trên Client
        /// </summary>
        Task Logout(CancellationToken ct = default);

        /// <summary>
        /// Lấy thông tin chi tiết của người dùng hiện tại từ Server
        /// </summary>
        Task<ApiResponse<AuthResultDto>> GetProfileAsync(CancellationToken ct = default);
        Task<ApiResponse<object>> UpdateProfileAsync(string email, string? avatarUrl, CancellationToken ct = default);

        /// <summary>
        /// Đổi mật khẩu người dùng
        /// </summary>
        Task<ApiResponse<object>> ChangePassword(string currentPassword, string newPassword, CancellationToken ct = default);
    }
}
