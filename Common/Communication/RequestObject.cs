using MessageBrokerApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Communication
{
    public class RequestObject : Message
    {
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public string SessionId { get; set; }
        public CommunicationData[] RequestData { get; set; }
        public string ErrorMessage { get; set; } = null;
    }
}
