using Common.Communication;
using MessageBrokerApplication.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace ClientAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IMessageBroker _messageBroker;
        public HomeController(IMessageBroker messageBroker)
        {
            _messageBroker = messageBroker;

        }


        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Get([FromBody] RequestObject request)
        {
            var result = new ContentResult();
            var responseMessage = await _messageBroker.PublishRPC(request,"ServerQueue","ClientQueue");

            var responseObject = JsonConvert.DeserializeObject<RequestObject>(responseMessage);
            if(responseObject.ErrorMessage != null)
            {
                result.StatusCode = 500;
                result.Content = responseObject.ErrorMessage;   
            }
            return result;
        }
    }
    
}
