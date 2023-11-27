using Common.Communication;
using Common.Services;
using MessageBrokerApplication.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using ServerApplication.ServerClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication.MessageHandlers
{
    public class RequestObjectHandler : IReplyMessageHandler<RequestObject>
    {
        private readonly IMemoryCache _cache;
        private readonly IMessageBroker _messageBroker;
        private readonly IServiceProvider _serviceProvider;
        private readonly EndpointServiceProvider _serviceTypeProvider;
        public RequestObjectHandler(IServiceProvider serviceProvider, IMessageBroker messageBroker, EndpointServiceProvider serviceTypeProvider)
        {
            _serviceProvider = serviceProvider;
            _messageBroker = messageBroker;
            _serviceTypeProvider = serviceTypeProvider;
        }
        public async Task Handle(RequestObject message, string replyQueue, string correlationId)
        {
            var result = new CommunicationData[1];
            BaseService service = null;
            message.ErrorMessage = null;
            try
            {
                    service = await _serviceTypeProvider.GetServiceAsync(message.ServiceName);

            }
            catch (Exception ex)
            {
                message.ErrorMessage = "Service Name Not Valid "+ex.Message;
                _messageBroker.Publish(message, replyQueue, correlationId);
                return;
            }

            try
            {
                await service.InitializeServiceAsync(message.SessionId, _cache);
                var method = service.GetType().GetMethod(message.MethodName)
                    ?? throw new Exception("Cannot get the method from the desired service");
                var resultObj = method.Invoke(service, new object[] { message.RequestData });
                if (resultObj != null)
                {
                    result = await (Task<CommunicationData[]>)resultObj;
                    message.RequestData = result;
                }
                else
                {
                    message.ErrorMessage = "Error in Methode Invocation";
                }
                _messageBroker.Publish(message, replyQueue, correlationId);
            }
            catch (Exception ex)
            {
                var responseError = ex.Message;
                message.ErrorMessage = responseError;
                _messageBroker.Publish(message, replyQueue, correlationId);
            }
        }
    }
}
