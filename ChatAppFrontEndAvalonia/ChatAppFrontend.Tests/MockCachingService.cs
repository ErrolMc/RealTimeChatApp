using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Other.Caching.Data;
using ChatAppFrontEnd.Source.Services;

namespace ChatAppFrontend.Tests
{
    public class MockCachingService : ICachingService
    {
        private string? _loginToken;
        private bool _isLoggedIn;
        private int _friendsVNum = -1;

        private readonly Dictionary<string, ThreadCache> _threads = new();
        private readonly Dictionary<string, MessageCache> _messages = new();
        private List<UserSimple> _friends = new();

        public Task<bool> Setup() => Task.FromResult(true);

        public Task<bool> SaveLoginToken(string token)
        {
            _loginToken = token;
            return Task.FromResult(true);
        }

        public Task<(bool, string)> GetLoginToken()
        {
            if (_loginToken is null)
                return Task.FromResult((false, string.Empty));

            return Task.FromResult((true, _loginToken));
        }

        public Task<bool> SaveIsLoggedIn(bool isLoggedIn)
        {
            _isLoggedIn = isLoggedIn;
            return Task.FromResult(true);
        }

        public Task<bool> GetIsLoggedIn() => Task.FromResult(_isLoggedIn);

        public Task<bool> ClearCache()
        {
            _loginToken = null;
            _isLoggedIn = false;
            _friendsVNum = -1;
            _threads.Clear();
            _messages.Clear();
            _friends = new List<UserSimple>();
            return Task.FromResult(true);
        }

        public void OnDisconnect()
        {
            _isLoggedIn = false;
        }

        public Task<List<ThreadCache>> GetAllThreads()
        {
            return Task.FromResult(_threads.Values.Select(thread => new ThreadCache
            {
                ThreadID = thread.ThreadID,
                Type = thread.Type,
                TimeStamp = thread.TimeStamp
            }).ToList());
        }

        public Task<bool> RemoveThreads(List<string> threadIDs, bool removeMessages = true)
        {
            var threadIdSet = threadIDs.ToHashSet();
            foreach (var threadID in threadIdSet)
            {
                _threads.Remove(threadID);
            }

            if (removeMessages)
            {
                var messageIdsToRemove = _messages.Values
                    .Where(message => threadIdSet.Contains(message.ThreadID))
                    .Select(message => message.MessageID)
                    .ToList();

                foreach (var messageID in messageIdsToRemove)
                {
                    _messages.Remove(messageID);
                }
            }

            return Task.FromResult(true);
        }

        public Task<bool> AddThreads(List<ThreadCache> threadIDs)
        {
            foreach (var thread in threadIDs)
            {
                if (_threads.ContainsKey(thread.ThreadID))
                    continue;

                _threads[thread.ThreadID] = new ThreadCache
                {
                    ThreadID = thread.ThreadID,
                    Type = thread.Type,
                    TimeStamp = thread.TimeStamp
                };
            }

            return Task.FromResult(true);
        }

        public Task<long> GetThreadTimeStamp(string threadID)
        {
            if (_threads.TryGetValue(threadID, out var thread))
                return Task.FromResult(thread.TimeStamp);

            return Task.FromResult(-1L);
        }

        public Task<List<MessageCache>> GetMessagesFromThread(string threadID)
        {
            var messages = _messages.Values
                .Where(message => message.ThreadID == threadID)
                .OrderBy(message => message.TimeStamp)
                .Select(message => new MessageCache
                {
                    MessageID = message.MessageID,
                    ThreadID = message.ThreadID,
                    FromUser = new UserSimple { UserID = message.FromUser.UserID, UserName = message.FromUser.UserName },
                    Message = message.Message,
                    TimeStamp = message.TimeStamp
                })
                .ToList();
            return Task.FromResult(messages);
        }

        public Task<bool> CacheMessages(List<Message> messages)
        {
            foreach (var message in messages)
            {
                var messageCache = message.ToMessageCache();
                _messages[messageCache.MessageID] = new MessageCache
                {
                    MessageID = messageCache.MessageID,
                    ThreadID = messageCache.ThreadID,
                    FromUser = new UserSimple { UserID = messageCache.FromUser.UserID, UserName = messageCache.FromUser.UserName },
                    Message = messageCache.Message,
                    TimeStamp = messageCache.TimeStamp
                };

                if (_threads.TryGetValue(message.ThreadID, out var thread))
                {
                    if (message.TimeStamp > thread.TimeStamp)
                        thread.TimeStamp = message.TimeStamp;
                }
                else
                {
                    _threads[message.ThreadID] = new ThreadCache
                    {
                        ThreadID = message.ThreadID,
                        Type = message.MessageType,
                        TimeStamp = message.TimeStamp
                    };
                }
            }

            return Task.FromResult(true);
        }

        public Task<List<UserSimple>> GetFriends()
        {
            return Task.FromResult(_friends.Select(friend => new UserSimple
            {
                UserID = friend.UserID,
                UserName = friend.UserName
            }).ToList());
        }

        public Task<int> GetFriendsVNum() => Task.FromResult(_friendsVNum);

        public Task<bool> CacheFriends(List<UserSimple> friends, int vNum)
        {
            _friends = friends.Select(friend => new UserSimple
            {
                UserID = friend.UserID,
                UserName = friend.UserName
            }).ToList();
            _friendsVNum = vNum;
            return Task.FromResult(true);
        }
    }
}
