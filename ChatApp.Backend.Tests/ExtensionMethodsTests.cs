using ChatApp.Backend.Services;
using ChatApp.Shared.Tables;

namespace ChatApp.Backend.Tests
{
    public class ExtensionMethodsTests
    {
        [Test]
        public void GetOwnerAndPutAtFront_MovesOwnerToStartOfList()
        {
            var users = new List<User>
            {
                new() { UserID = "user-2", Username = "Second" },
                new() { UserID = "user-1", Username = "Owner" }
            };

            var found = users.GetOwnerAndPutAtFront(out var owner, "user-1");

            Assert.That(found, Is.True);
            Assert.That(owner.UserID, Is.EqualTo("user-1"));
            Assert.That(users.Select(user => user.UserID), Is.EqualTo(new[] { "user-1", "user-2" }));
        }

        [Test]
        public void GetOwnerAndPutAtFront_ReturnsFalseWhenOwnerMissing()
        {
            var users = new List<User>
            {
                new() { UserID = "user-2", Username = "Second" }
            };

            var found = users.GetOwnerAndPutAtFront(out _, "missing-user");

            Assert.That(found, Is.False);
            Assert.That(users.Select(user => user.UserID), Is.EqualTo(new[] { "user-2" }));
        }

        [Test]
        public void GetGroupName_JoinsUsernamesWithCommaSeparator()
        {
            var users = new List<User>
            {
                new() { Username = "Alice" },
                new() { Username = "Bob" }
            };

            var groupName = users.GetGroupName();

            Assert.That(groupName, Is.EqualTo("Alice, Bob"));
        }
    }
}
