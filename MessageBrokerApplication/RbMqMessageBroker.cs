using MessageBrokerApplication.Interfaces;
using MessageBrokerApplication.Models;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBrokerApplication
{
    public class RbMqMessageBroker : IMessageBroker
    {

        //private readonly List<string> correlationIdList = new List<string>();
        private readonly ConcurrentDictionary<string,
                TaskCompletionSource<string>> callbackMapper = new();

        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public RbMqMessageBroker(IServiceProvider serviceProvider, IServiceScopeFactory serviceScopeFactory)
        {
            _serviceProvider = serviceProvider;
            _serviceScopeFactory = serviceScopeFactory;
        }
        public string DeclareQueue(string QueueName)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            return channel.QueueDeclare(QueueName, false, false, false, null);

        }

        //public Task SendCommand<T>(T command) where T : Command
        //{
        //    // command : message , message:IRequest<bool>, _mediator.Send() expects a IRequest<bool>
        //    return _mediator.Send(command);
        //}

        //if we publish a message we want to add a corellationId.
        //if we publish a reply we want to take the arleady existing correlationId

        public Task<string> PublishRPC(Message message,
            string queue,
            string replyQueue = null,
            string correlationId = null)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    IBasicProperties props = channel.CreateBasicProperties();

                    var tcs = new TaskCompletionSource<string>();

                    if (correlationId == null)
                    {
                        correlationId = Guid.NewGuid().ToString();
                        callbackMapper.TryAdd(correlationId, tcs);
                    }

                    props.CorrelationId = correlationId;

                    props.ReplyTo = replyQueue;

                    var queueName = queue;

                    var bodyContent = JsonConvert.SerializeObject(message);

                    var body = Encoding.UTF8.GetBytes(bodyContent);

                    channel.BasicPublish(
                        string.Empty,
                        queueName,
                        props,
                        body
                        );

                    return tcs.Task;
                }
            }
        }


        public void Publish(Message message,
           string queue, string correlationId = null)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    IBasicProperties props = channel.CreateBasicProperties();

                    var tcs = new TaskCompletionSource<string>();

                    if (correlationId == null)
                    {
                        correlationId = Guid.NewGuid().ToString();
                        callbackMapper.TryAdd(correlationId, tcs);
                    }

                    props.CorrelationId = correlationId;

                    var queueName = queue;

                    var bodyContent = JsonConvert.SerializeObject(message);

                    var body = Encoding.UTF8.GetBytes(bodyContent);

                    channel.BasicPublish(
                        string.Empty,
                        queueName,
                        props,
                        body
                        );
                }
            }

        }


        //overload when we need to reply
        public void SubscribeReply<T, TH>(string subscribingQueue) where T : Message
            where TH : IReplyMessageHandler<T>
        {
            var factory = new ConnectionFactory() { HostName = "localhost", DispatchConsumersAsync = true };

            var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            var consumer = new AsyncEventingBasicConsumer(channel);

            var handlerType = typeof(TH);
            //

            var scope = _serviceScopeFactory.CreateScope();

            var handlerInstance = scope.ServiceProvider.GetRequiredService(typeof(IReplyMessageHandler<T>));

            var handlerConcreteType = typeof(IReplyMessageHandler<>).MakeGenericType(typeof(T));

            consumer.Received += async (model, ea) =>
            {

                var messageString = Encoding.UTF8.GetString(ea.Body.ToArray());

                var messageObject = JsonConvert.DeserializeObject<T>(messageString);

                await (Task)handlerConcreteType.GetMethod("Handle").Invoke(handlerInstance, new object[] {
                    messageObject, ea.BasicProperties.ReplyTo,ea.BasicProperties.CorrelationId });
            };

            channel.BasicConsume(
                consumer,
                subscribingQueue,
                autoAck: true
                );
        }


        public void SubscribeRPC<T, TH>(string subscribingQueue
             ) where T : Message
           where TH : IMessageHandler<T>
        {
            var factory = new ConnectionFactory() { HostName = "localhost", DispatchConsumersAsync = true };

            var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            var consumer = new AsyncEventingBasicConsumer(channel);

            var handlerType = typeof(TH);
            //

            var scope = _serviceScopeFactory.CreateScope();

            var handlerInstance = scope.ServiceProvider.GetRequiredService(typeof(IMessageHandler<T>));

            var handlerConcreteType = typeof(IMessageHandler<>).MakeGenericType(typeof(T));

            consumer.Received += async (model, ea) =>
            {

                // if we want to check for correlation and we cant find the Id then exit the method
                if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
                {
                    return;
                }

                var messageString = Encoding.UTF8.GetString(ea.Body.ToArray());

                var messageObject = JsonConvert.DeserializeObject<T>(messageString);

                await (Task)handlerConcreteType.GetMethod("Handle").Invoke(handlerInstance, new object[] { messageObject });

                tcs.SetResult(messageString);

            };

            channel.BasicConsume(
                consumer,
                subscribingQueue,
                autoAck: true
                );
        }
    }

}
