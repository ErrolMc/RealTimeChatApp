using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Other.Caching.Data;

namespace ChatAppFrontend.Tests
{
    public class MockCachingServiceTests
    {
        [Test]
        public async Task GetLoginToken_WhenNotSaved_ReturnsFailure()
        {
            var cache = new MockCachingService();

            var (success, token) = await cache.GetLoginToken();

            Assert.That(success, Is.False);
            Assert.That(token, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task GetThreadTimeStamp_WhenThreadMissing_ReturnsMinusOne()
        {
            var cache = new MockCachingService();

            var timeStamp = await cache.GetThreadTimeStamp("missing-thread");

            Assert.That(timeStamp, Is.EqualTo(-1));
        }

        [Test]
        public async Task SaveLoginToken_ThenGetLoginToken_ReturnsSuccess()
        {
            var cache = new MockCachingService();

            await cache.SaveLoginToken("token-123");
            var (success, token) = await cache.GetLoginToken();

            Assert.That(success, Is.True);
            Assert.That(token, Is.EqualTo("token-123"));
        }

        [Test]
        public async Task SaveIsLoggedIn_ThenOnDisconnect_ResetsState()
        {
            var cache = new MockCachingService();

            await cache.SaveIsLoggedIn(true);
            cache.OnDisconnect();
            var isLoggedIn = await cache.GetIsLoggedIn();

            Assert.That(isLoggedIn, Is.False);
        }

        [Test]
        public async Task AddThreads_ThenGetAllThreads_ReturnsStoredThreads()
        {
            var cache = new MockCachingService();
            await cache.AddThreads(new List<ThreadCache>
            {
                new() { ThreadID = "t1", Type = 1, TimeStamp = 10 }
            });

            var threads = await cache.GetAllThreads();

            Assert.That(threads.Count, Is.EqualTo(1));
            Assert.That(threads[0].ThreadID, Is.EqualTo("t1"));
            Assert.That(threads[0].TimeStamp, Is.EqualTo(10));
        }

        [Test]
        public async Task CacheMessages_ThenGetMessagesFromThread_ReturnsMessagesAndUpdatesTimestamp()
        {
            var cache = new MockCachingService();
            var user = new UserSimple { UserID = "u1", UserName = "Alice" };
            var messages = new List<Message>
            {
                new() { ID = "m1", ThreadID = "t1", FromUser = user, MessageContents = "one", MessageType = 0, TimeStamp = 5 },
                new() { ID = "m2", ThreadID = "t1", FromUser = user, MessageContents = "two", MessageType = 0, TimeStamp = 20 }
            };

            await cache.CacheMessages(messages);
            var cachedMessages = await cache.GetMessagesFromThread("t1");
            var threadTimeStamp = await cache.GetThreadTimeStamp("t1");

            Assert.That(cachedMessages.Select(message => message.MessageID), Is.EqualTo(new[] { "m1", "m2" }));
            Assert.That(threadTimeStamp, Is.EqualTo(20));
        }

        [Test]
        public async Task RemoveThreads_WhenRemoveMessagesFalse_LeavesMessages()
        {
            var cache = new MockCachingService();
            var user = new UserSimple { UserID = "u1", UserName = "Alice" };
            await cache.CacheMessages(new List<Message>
            {
                new() { ID = "m1", ThreadID = "t1", FromUser = user, MessageContents = "one", MessageType = 0, TimeStamp = 5 }
            });

            await cache.RemoveThreads(new List<string> { "t1" }, removeMessages: false);
            var cachedMessages = await cache.GetMessagesFromThread("t1");

            Assert.That(cachedMessages.Count, Is.EqualTo(1));
            Assert.That(await cache.GetThreadTimeStamp("t1"), Is.EqualTo(-1));
        }

        [Test]
        public async Task CacheFriends_ThenClearCache_ResetsFriendsAndVersion()
        {
            var cache = new MockCachingService();
            await cache.CacheFriends(new List<UserSimple> { new() { UserID = "u1", UserName = "Alice" } }, 7);

            await cache.ClearCache();
            var friends = await cache.GetFriends();
            var vNum = await cache.GetFriendsVNum();

            Assert.That(friends, Is.Empty);
            Assert.That(vNum, Is.EqualTo(-1));
        }
    }
}
