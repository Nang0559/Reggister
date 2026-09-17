using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Requests.Leaves
{
    public class LeaveAttachmentUploadDto
    {
        public string FileName { get; set; } = "";
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "";
    }
}
