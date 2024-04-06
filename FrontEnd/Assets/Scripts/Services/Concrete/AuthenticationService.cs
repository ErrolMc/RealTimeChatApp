using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ChatApp.Services.Concrete
{
    public class AuthenticationService : IAuthenticationService
    {
        public async Task<(bool, string)> TryLogin()
        {
            await UniTask.Yield();
            
            return (false, "");
        }

        public async Task<(bool, string)> TryRegister()
        {
            await UniTask.Yield();

            return (false, "");
        }
    }
}

