using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Response.AuditLog
{
    public class AuditLogResponse
    {
        public int LogID { get; set; }
        public string Action { get; set; }
        public string TableName { get; set; }
        public string UserName { get; set; }
        public DateTime ActionDate { get; set; }
        public string Details { get; set; }
    }
}
