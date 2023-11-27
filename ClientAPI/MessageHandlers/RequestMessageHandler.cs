using Common.Communication;
using MessageBrokerApplication.Interfaces;

namespace ClientAPI.MessageHandlers
{
    public class RequestMessageHandler : IMessageHandler<RequestObject>
    {
        public async Task Handle(RequestObject message)
        {
            //add extra logic when consuming servers response
        }
    }
}
