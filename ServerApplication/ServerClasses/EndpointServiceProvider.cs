using Common.Attributes;
using Common.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication.ServerClasses
{
    public class EndpointServiceProvider
    {
        public Dictionary<string, string> Services { get; private set; }

        private readonly IServiceProvider _serviceProvider;

        public EndpointServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Services = new Dictionary<string, string>();

            var assemblies = LoadAllBinDirectoryAssemblies();
            foreach (var assembly in assemblies)
            {
                var services = assembly.GetTypes().Where(type => type.GetCustomAttribute<EndPointServiceAttribute>() != null);
                foreach (var service in services)
                {
                    Services.Add(service.Name, service.FullName + ", " + service.Assembly.GetName().Name);
                }
            }
        }

        private List<Assembly> LoadAllBinDirectoryAssemblies()
        {
            // Get the path to the bin directory
            string path = Directory.GetCurrentDirectory() + "\\bin\\Debug\\net6.0";

            // Get all the ".dll" files in the bin directory
            string[] files = Directory.GetFiles(path, searchPattern: "*.dll");
            List<Assembly> loadedAssemblies = new();

            // Iterate through the files and load each assembly
            foreach (string dll in files)
            {
                try
                {
                    Assembly loadedAssembly = Assembly.LoadFile(dll);
                    loadedAssemblies.Add(loadedAssembly);
                }
                catch (Exception)
                {
                    //throw new Exception("Could Not Load Referenced Assembly",loadEx);
                }
            }
            // Return the list of loaded assemblies
            return loadedAssemblies;
        }

        public Task<BaseService> GetServiceAsync(string serviceName)
        {
            BaseService service = null;
            var serviceType = Type.GetType(Services[serviceName]);
            if (serviceType == null)
            {
                throw new Exception("Service Type Not Found");
            }
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    service = (BaseService)scope.ServiceProvider.GetService(serviceType);
                }
                catch (Exception)
                {

                    throw;
                }
            }

            if (service == null)
            {
                throw new Exception("Service not found in container");
            }

            return Task.FromResult(service);
        }
    }
}
