using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;

namespace ChatAppFrontEnd.Source.Services
{
    public interface ICachingService
    {
        Task<bool> Setup();
        
        // auth
        Task<bool> SaveLoginToken(string token);
        Task<(bool, string)> GetLoginToken();
        Task<bool> ClearLoginToken();
        
    }
}

