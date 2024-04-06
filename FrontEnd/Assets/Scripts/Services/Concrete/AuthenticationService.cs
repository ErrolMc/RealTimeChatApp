using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ChatApp.Services.Concrete
{
    public class AuthenticationService : IAuthenticationService
    {
        public async Task<(bool, string)> TryLogin()
        {
            await Task.Delay(1000);
            
            return (false, "");
        }

        public async Task<(bool, string)> TryRegister()
        {
            await Task.Delay(1000);

            return (false, "");
        }
    }
}

