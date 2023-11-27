using Common.Attributes;
using Common.Communication;
using Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication.EndPointServices
{
    [EndPointService]
    public class TestEndPointService :BaseService
    {
        protected override bool SessionMustExist { get; set; } = false;
        public async Task<CommunicationData[]> TestMethod(CommunicationData[] communicationDatas)
        {
            var comm = communicationDatas[0].Value;
            communicationDatas[0].Name = "Server";
            return communicationDatas;
        }
    }
}
