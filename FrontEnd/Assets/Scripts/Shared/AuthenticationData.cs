using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace ChatApp.Shared.Authentication
{
    [Serializable]
    public class UserLoginData
    {
        [JsonProperty("A")] public string UserName { get; set; }
        [JsonProperty("B")] public string Password { get; set; }
    }

    public class UserLoginResponseData
    {
        [JsonProperty("A")] public bool Status { get; set; }
        [JsonProperty("B")] public string Message { get; set; }
    }
}