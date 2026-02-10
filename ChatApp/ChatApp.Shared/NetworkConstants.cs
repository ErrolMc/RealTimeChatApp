
namespace ChatApp.Shared.Constants
{
    public static class NetworkConstants
    {
        private const string FUNCTIONS_URI_LOCALHOST = "https://localhost:7071";
        private const string SIGNALR_SERVER_URI_LOCALHOST = "https://localhost:7003";
        private const string COSMOSDB_URI_LOCALHOST = "https://localhost:8081";
        private const string WEBAPP_URI_LOCALHOST = "https://127.0.0.1:8000";
        
        public static string WEBAPP_URI => WEBAPP_URI_LOCALHOST;
        public static string FUNCTIONS_URI => FUNCTIONS_URI_LOCALHOST;
        public static string SIGNALR_URI => SIGNALR_SERVER_URI_LOCALHOST;
        public static string COSMOSDB_URI => COSMOSDB_URI_LOCALHOST;
    }
}