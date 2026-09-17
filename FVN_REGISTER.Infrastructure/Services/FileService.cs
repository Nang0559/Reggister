

using FVN_REGISTER.Application.Interfaces.Common;

namespace FVN_REGISTER.Infrastructure.Services

{
    public class FileService : IFileService
    {
        public void EnsureFolder(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path)) return;

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                // Log lỗi này lại vì nếu không tạo được thư mục, các hàm SaveFile phía sau sẽ crash
                Console.WriteLine($"[FileService Error]: Could not create path {path}. {ex.Message}");
                throw; // Re-throw để phía nghiệp vụ biết mà dừng lại
            }
        }
    }
}
