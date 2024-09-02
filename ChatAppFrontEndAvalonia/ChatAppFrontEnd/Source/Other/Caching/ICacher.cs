using System.Threading.Tasks;

namespace ChatAppFrontEnd.Source.Other.Caching
{
    public interface ICacher
    {
        Task<bool> SaveLoginToken(string token);
        Task<(bool, string)> GetLoginToken();
        Task<bool> ClearLoginToken();
        
        Task<bool> Setup();
    }
}
