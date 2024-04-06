using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Shared.Tables;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ChatApp.Services
{
    public interface IAuthenticationService
    {
        public bool IsLoggedIn { get; set; }
        public User CurrentUser { get; set; }
        public UniTask<(bool, string, User)> TryLogin(string username, string password);
        public UniTask<(bool, string)> TryRegister(string username, string password);
    }   
}

