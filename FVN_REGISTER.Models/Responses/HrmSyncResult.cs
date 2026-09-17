

namespace FVN_REGISTER.Contract.Responses
{
    public class HrmSyncResult
    {
        public int TotalSource { get; set; }
        public int Added { get; set; }
        public int Updated { get; set; }
        public int Deactivated { get; set; }
        public int Unchanged { get; set; }
        public int Superseded { get; set; }
        public List<string> Errors { get; set; } = new();

        public bool Success => Errors.Count == 0;

        public string Summary =>
            $"Nguồn: {TotalSource} | Thêm mới: {Added} | Cập nhật: {Updated} | Vô hiệu: {Deactivated} | Không đổi: {Unchanged}";
    }
}
