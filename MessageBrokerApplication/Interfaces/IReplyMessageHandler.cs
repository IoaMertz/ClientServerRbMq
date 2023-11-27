using MessageBrokerApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBrokerApplication.Interfaces
{
    public interface IReplyMessageHandler<TMessage> where TMessage : Message
    {
        Task Handle(TMessage message, string replyQueue, string correlationId);

    }
}
