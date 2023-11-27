using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class SessionObject
    {
        public string SessionId { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime LastAccess { get; set; }
        public Int64 UserId { get; set; }

    }
}
