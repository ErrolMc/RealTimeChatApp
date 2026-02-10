using ChatApp.Shared.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User = ChatApp.Shared.Tables.User;

namespace ChatApp.Backend.Services
{
    public static class ExtensionMethods
    {
        public static bool GetOwnerAndPutAtFront(this List<User> users, out User owner, string ownerUserID)
        {
            owner = users.FirstOrDefault(user => user.UserID == ownerUserID);
            if (owner == null)
                return false;

            if (!users.Remove(owner))
                return false;

            users.Insert(0, owner);
            return true;
        }

        public static string GetGroupName(this List<User> users)
        {
            return string.Join(", ", users.Select(user => $"{user.Username}")).TrimEnd();
        }
    }
}
