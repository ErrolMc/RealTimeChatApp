using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class NetworkCallerService : INetworkCallerService
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<BackendPostResponse<TRespClass>> PerformBackendPostRequest<TReqClass, TRespClass>(string endpointName, TReqClass requestData)
            where TReqClass : class
            where TRespClass : class
        {
            try
            {
                string json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync($"{ServiceConfig.BackendUri}/api/{endpointName}", content);

                if (!response.IsSuccessStatusCode)
                {
                    return new BackendPostResponse<TRespClass>() { ConnectionSuccess = false, Message = response.ReasonPhrase, ResponseData = null };
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                TRespClass responseData = JsonConvert.DeserializeObject<TRespClass>(responseContent);
                return new BackendPostResponse<TRespClass>() { ConnectionSuccess = true, Message = "Request Success", ResponseData = responseData };
            }
            catch (Exception ex)
            {
                return new BackendPostResponse<TRespClass>() { ConnectionSuccess = false, Message = $"Exception: {ex.Message}", ResponseData = null };
            }
        }
    }
}

