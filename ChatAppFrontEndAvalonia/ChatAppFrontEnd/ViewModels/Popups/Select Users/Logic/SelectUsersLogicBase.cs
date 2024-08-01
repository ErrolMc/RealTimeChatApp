using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatAppFrontEnd.ViewModels.Logic
{
    public abstract class SelectUsersLogicBase
    {
        public abstract Task<(bool result, string message)> HandleConfirm(List<string> userIDs);
        public abstract void OnSuccess();
        public abstract int MaxAmount { get; }
        public abstract int AmountDifference { get; }
    }
}

