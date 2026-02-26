using ChatAppFrontEnd.Source.Services.Concrete;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChatAppFrontEnd.Source.Services
{
    public static class ServiceConfig
    {
        public static string BackendUri { get; set; } = Environment.GetEnvironmentVariable("services__backend__https__0") ?? "https://localhost:7071";
        public static string SignalRUri { get; set; } = Environment.GetEnvironmentVariable("services__signalr-server__https__0") ?? "https://localhost:7003";
    }

    public class BackendPostResponse<T> where T : class
    {
        public bool ConnectionSuccess { get; set; }
        public string Message { get; set; }
        public T ResponseData { get; set; }
    }

    public interface INetworkCallerService
    {
        public Task<BackendPostResponse<TRespClass>> PerformBackendPostRequest<TReqClass, TRespClass>(string endpointName, TReqClass requestData)
            where TReqClass : class
            where TRespClass : class;
    }
}
