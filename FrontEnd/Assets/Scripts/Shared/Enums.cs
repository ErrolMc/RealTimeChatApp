
namespace ChatApp.Shared.Enums
{
    public enum GroupUpdateReason
    {
        DoesntMatter = 0,

        UserLeft = 1,
        ThisUserLeft = 2, // for updating ui locally

        UserKicked = 3,
        ThisUserKicked = 4, // for updating ui locally

        AddedUsersToGroup = 5,
        GroupDeleted = 6,
    }

    public static class EnumExtensions
    {
        public static bool IsReasonToDeleteLocalGroup(this GroupUpdateReason reason)
        {
            return reason is GroupUpdateReason.ThisUserKicked 
                          or GroupUpdateReason.ThisUserLeft
                          or GroupUpdateReason.GroupDeleted;
        }
    }
}