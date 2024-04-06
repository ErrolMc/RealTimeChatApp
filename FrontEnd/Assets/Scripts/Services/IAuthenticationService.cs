using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ChatApp.Services
{
    public interface IAuthenticationService
    {
        public UniTask<(bool, string)> TryLogin(string username, string password);
        public UniTask<(bool, string)> TryRegister(string username, string password);
    }   
}

