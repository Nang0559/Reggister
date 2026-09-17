using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Core.Config
{
    public class AppOptions
    {
        public string SiteUrl { get; set; } = string.Empty;

        /// <summary>Tạo đường dẫn tuyệt đối tới một route cụ thể</summary>
        public string BuildUrl(string path)
        {
            var base_ = SiteUrl.TrimEnd('/');
            var path_ = path.TrimStart('/');
            return $"{base_}/{path_}";
        }
    }
}
