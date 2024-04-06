using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ChatApp.Services
{
    public interface IAuthenticationService
    {
        public Task<(bool, string)> TryLogin();
        public Task<(bool, string)> TryRegister();
    }   
}

