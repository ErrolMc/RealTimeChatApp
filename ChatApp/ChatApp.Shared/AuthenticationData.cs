using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using ChatApp.Shared.Tables;

namespace ChatApp.Shared.Authentication
{
    [Serializable]
    public class UserLoginData
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    [Serializable]
    public class UserLoginResponseData
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public User User { get; set; }
        public string LoginToken { get; set; }
    }

    [Serializable]
    public class AutoLoginData
    {
        public string Token { get; set; }
    }
}