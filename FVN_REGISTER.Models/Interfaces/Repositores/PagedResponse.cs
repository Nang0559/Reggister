using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Interfaces.Repositores
{
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new(); // Đổi Data thành Items để tránh trùng tên với ApiResponse.Data
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNext => PageNumber < TotalPages;
        public bool HasPrevious => PageNumber > 1;
    }
}
