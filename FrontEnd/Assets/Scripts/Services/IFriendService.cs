using Cysharp.Threading.Tasks;

namespace ChatApp.Services
{
    public interface IFriendService
    {
        public UniTask<(bool, string)> AddFriend(string userName);
    }
}