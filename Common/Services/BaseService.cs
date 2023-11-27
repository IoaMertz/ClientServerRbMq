using Common.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services
{
    public class BaseService
    {

        protected string SessionId { get; private set; }
        public SessionObject Session { get; protected set; }
        protected virtual bool SessionMustExist { get; set; } = true;

        public BaseService() { }

        public async Task InitializeServiceAsync(string sessionId, IMemoryCache cache)
        {
            SessionId = sessionId;
            if (SessionMustExist)
            {
                if (SessionId == null || !cache.TryGetValue(SessionId, out SessionObject session))
                {
                    throw new Exception("User is not logged in.");
                }
                Session = session;
            }
            await Task.CompletedTask;
        }
    }
}
