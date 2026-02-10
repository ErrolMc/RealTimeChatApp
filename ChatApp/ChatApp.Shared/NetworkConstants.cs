using System;

namespace ChatApp.Shared.Constants
{
    public static class NetworkConstants
    {
        private const string BACKEND_URI_LOCALHOST = "https://localhost:7071";
        private const string SIGNALR_SERVER_URI_LOCALHOST = "https://localhost:7003";
        private const string COSMOSDB_URI_LOCALHOST = "https://localhost:8081";
        private const string WEBAPP_URI_LOCALHOST = "https://127.0.0.1:8000";

        public static string WEBAPP_URI => WEBAPP_URI_LOCALHOST;
        public static string BACKEND_URI => Environment.GetEnvironmentVariable("services__backend__https__0") ?? BACKEND_URI_LOCALHOST;
        public static string SIGNALR_URI => Environment.GetEnvironmentVariable("services__signalr-server__https__0") ?? SIGNALR_SERVER_URI_LOCALHOST;
        public static string COSMOSDB_URI => COSMOSDB_URI_LOCALHOST;
    }
}