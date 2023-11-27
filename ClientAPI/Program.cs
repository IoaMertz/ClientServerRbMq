using ClientAPI.MessageHandlers;
using Common.Communication;
using MessageBrokerApplication;
using MessageBrokerApplication.Interfaces;

namespace ClientAPI
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
            builder.Services.AddSingleton<IMessageBroker,RbMqMessageBroker>();
            builder.Services.AddTransient<IMessageHandler<RequestObject>,RequestMessageHandler>();


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

            ConfigureEventBus(app);

            app.Run();
        }
        private static void ConfigureEventBus(WebApplication app)
        {
            var messageBroker = app.Services.GetRequiredService<IMessageBroker>();
            messageBroker.DeclareQueue("ClientQueue");
            messageBroker.SubscribeRPC<RequestObject, RequestMessageHandler>("ClientQueue");
        }
    }
}