using Common.Communication;
using MessageBrokerApplication;
using MessageBrokerApplication.Interfaces;
using ServerApplication.EndPointServices;
using ServerApplication.MessageHandlers;
using ServerApplication.ServerClasses;

namespace ServerApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddTransient<TestEndPointService>();
            builder.Services.AddSingleton<IMessageBroker, RbMqMessageBroker>();
            builder.Services.AddTransient<IReplyMessageHandler<RequestObject>, RequestObjectHandler>();
            builder.Services.AddSingleton<EndpointServiceProvider>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            ConfigureMessageBroker(app);
            app.Run();
        }

        private static void ConfigureMessageBroker(WebApplication app)
        {
            var messageBroker = app.Services.GetRequiredService<IMessageBroker>();
            messageBroker.DeclareQueue("ServerQueue");
            messageBroker.SubscribeReply<RequestObject, RequestObjectHandler>("ServerQueue");

           

        }
    }
}