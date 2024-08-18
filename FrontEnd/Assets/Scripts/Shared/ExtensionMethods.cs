using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Tables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using User = ChatApp.Shared.Tables.User;
using ChatApp.Shared.TableDataSimple;

namespace ChatApp.Shared.ExtensionMethods
{
    public static class ExtensionMethods
    {
        public static List<UserSimple> ToUserSimpleList(this List<User> users)
        {
            return users.Select(user => user.ToUserSimple()).ToList();
        }

        public static List<GroupDMSimple> ToGroupDMSimpleList(this List<ChatThread> groupDMs)
        {
            return groupDMs.Select(groupDM => groupDM.ToGroupDMSimple()).ToList();
        }
    }
}
