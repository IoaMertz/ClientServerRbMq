using MessageBrokerApplication.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBrokerApplication
{
    public static class MessageBrokerDependancyResolver
    {
        public static IServiceCollection MessageBrokerInfrastructureRegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IMessageBroker, RbMqMessageBroker>();

            return services;

        }

    }
}
