using MessageBrokerApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBrokerApplication.Interfaces
{
    public interface IMessageBroker
    {
        string DeclareQueue(string QueueName);
        Task<string> PublishRPC(Message message,
            string queue, string replyQueue, string correlationId = null);
        void SubscribeRPC<T, TH>(string subscribingQueue)
            where T : Message
            where TH : IMessageHandler<T>;

        void SubscribeReply<T, TH>(string subscribingQueue) where T : Message
          where TH : IReplyMessageHandler<T>;

        void Publish(Message message,
           string queue, string correlationId = null);
    }
}
