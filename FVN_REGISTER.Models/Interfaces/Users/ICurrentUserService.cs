using FVN_REGISTER.Contract.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Interfaces.Users
{
    public interface ICurrentUserService
    {
        // Trả về null nếu không tìm thấy Token hợp lệ
        CurrentUser? GetCurrentUser();

        // Các hàm check nhanh dựa trên Claims
        bool IsLoggedIn { get; }
        string? GetUserId(); // Thường trả về ID dưới dạng string từ ClaimTypes.NameIdentifier
    }
}
