using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Shared.Constants;
using Newtonsoft.Json;

namespace ChatAppFrontEnd.Source.Utils
{
    public class FunctionPostResponse<T> where T : class
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T ResponseData { get; set; }
    }
    
    public static class NetworkHelper
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task<FunctionPostResponse<TRespClass>> PerformFunctionPostRequest<TReqClass, TRespClass>(string functionName, TReqClass requestData) 
            where TReqClass : class 
            where TRespClass : class
        {
            try
            {
                string json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(NetworkConstants.FUNCTIONS_URI + "api/" + functionName, content);

                if (!response.IsSuccessStatusCode)
                {
                    return new FunctionPostResponse<TRespClass>() { Success = false, Message = response.ReasonPhrase, ResponseData = null };
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                TRespClass responseData = JsonConvert.DeserializeObject<TRespClass>(responseContent);
                return new FunctionPostResponse<TRespClass>() { Success = true, Message = "Request Success", ResponseData = responseData };
            }
            catch (Exception ex)
            {
                return new FunctionPostResponse<TRespClass>() { Success = false, Message = $"Exception: {ex.Message}", ResponseData = null };
            }
        }
    }
    
}

