
namespace ChatApp.Shared.Constants
{
    public static class NetworkConstants
    {
        private const string FUNCTIONS_URI_LOCALHOST = "http://localhost:7182/";
        private const string SIGNALR_SERVER_URI_LOCALHOST = "https://localhost:7003/";
        private const string COSMOSDB_URI_LOCALHOST = "https://localhost:8081/";
        
        public static string FUNCTIONS_URI => FUNCTIONS_URI_LOCALHOST;
        public static string SIGNALR_URI => SIGNALR_SERVER_URI_LOCALHOST;
        public static string COSMOSDB_URI => COSMOSDB_URI_LOCALHOST;
    }
}