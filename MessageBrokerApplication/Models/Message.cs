using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBrokerApplication.Models
{
    public abstract class Message
    {
        public string MessageType { get; protected set; }
        public DateTime TimeStamp { get; protected set; }

        public Message()

        {
            MessageType = GetType().Name;
            TimeStamp = DateTime.Now;

        }
    }
}
