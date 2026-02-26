using ChatApp.Shared;
using ChatApp.Shared.Messages;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Other.Caching.Data;
using ChatAppFrontEnd.Source.Services;
using ChatAppFrontEnd.Source.Services.Concrete;
using Moq;

namespace ChatAppFrontend.Tests
{
    public class ChatServiceTests
    {
        [Test]
        public async Task SendMessage_WithUserSimple_WhenNetworkFails_ReturnsFalse()
        {
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            var authentication = CreateAuthenticationMock("u1");
            network
                .Setup(service => service.PerformBackendPostRequest<SendMessageRequestData, SendMessageResponseData>(
                    EndpointNames.SEND_MESSAGE,
                    It.IsAny<SendMessageRequestData>()))
                .ReturnsAsync(new BackendPostResponse<SendMessageResponseData>
                {
                    ConnectionSuccess = false,
                    Message = "network error",
                    ResponseData = null!
                });

            var service = new ChatService(authentication.Object, cache, network.Object);
            var recipient = new UserSimple { UserID = "u2", UserName = "Bob" };

            var result = await service.SendMessage(recipient, "hello");
            var threadId = SharedStaticMethods.CreateHashedDirectMessageID("u1", "u2");
            var cachedMessages = await cache.GetMessagesFromThread(threadId);

            Assert.That(result, Is.False);
            Assert.That(cachedMessages, Is.Empty);
        }

        [Test]
        public async Task SendMessage_WithGroupDMSimple_WhenBackendSucceeds_CachesMessageAndReturnsTrue()
        {
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            var authentication = CreateAuthenticationMock("u1");
            var backendMessage = new Message
            {
                ID = "m1",
                ThreadID = "group-1",
                FromUser = new UserSimple { UserID = "u1", UserName = "Alice" },
                MessageContents = "group hello",
                MessageType = (int)MessageType.GroupMessage,
                TimeStamp = 10
            };
            network
                .Setup(service => service.PerformBackendPostRequest<SendMessageRequestData, SendMessageResponseData>(
                    EndpointNames.SEND_MESSAGE,
                    It.IsAny<SendMessageRequestData>()))
                .ReturnsAsync(new BackendPostResponse<SendMessageResponseData>
                {
                    ConnectionSuccess = true,
                    Message = "Request Success",
                    ResponseData = new SendMessageResponseData
                    {
                        Success = true,
                        NotificationSuccess = true,
                        ResponseMessage = "ok",
                        Message = backendMessage
                    }
                });

            var service = new ChatService(authentication.Object, cache, network.Object);
            var group = new GroupDMSimple { GroupID = "group-1", Name = "Group", Owner = "u1" };

            var result = await service.SendMessage(group, "group hello");
            var cachedMessages = await cache.GetMessagesFromThread("group-1");

            Assert.That(result, Is.True);
            Assert.That(cachedMessages.Count, Is.EqualTo(1));
            Assert.That(cachedMessages[0].MessageID, Is.EqualTo("m1"));
        }

        [Test]
        public async Task GetMessages_WhenBackendRequestFails_ReturnsCachedMessagesOnly()
        {
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            var authentication = CreateAuthenticationMock("u1");
            await cache.CacheMessages(new List<Message>
            {
                new()
                {
                    ID = "cached-1",
                    ThreadID = "group-1",
                    FromUser = new UserSimple { UserID = "u2", UserName = "Bob" },
                    MessageContents = "cached message",
                    MessageType = (int)MessageType.GroupMessage,
                    TimeStamp = 5
                }
            });
            network
                .Setup(service => service.PerformBackendPostRequest<GetMessagesRequestData, GetMessagesResponseData>(
                    EndpointNames.GET_MESSAGES,
                    It.IsAny<GetMessagesRequestData>()))
                .ReturnsAsync(new BackendPostResponse<GetMessagesResponseData>
                {
                    ConnectionSuccess = false,
                    Message = "network error",
                    ResponseData = null!
                });

            var service = new ChatService(authentication.Object, cache, network.Object);
            var group = new GroupDMSimple { GroupID = "group-1", Name = "Group", Owner = "u1" };

            var messages = await service.GetMessages(group);

            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(messages[0].MessageID, Is.EqualTo("cached-1"));
        }

        [Test]
        public async Task GetMessages_WhenBackendReturnsNewMessages_CachesAndReturnsMessages()
        {
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            var authentication = CreateAuthenticationMock("u1");
            var backendMessage = new Message
            {
                ID = "new-1",
                ThreadID = "group-1",
                FromUser = new UserSimple { UserID = "u2", UserName = "Bob" },
                MessageContents = "new message",
                MessageType = (int)MessageType.GroupMessage,
                TimeStamp = 20
            };
            network
                .Setup(service => service.PerformBackendPostRequest<GetMessagesRequestData, GetMessagesResponseData>(
                    EndpointNames.GET_MESSAGES,
                    It.IsAny<GetMessagesRequestData>()))
                .ReturnsAsync(new BackendPostResponse<GetMessagesResponseData>
                {
                    ConnectionSuccess = true,
                    Message = "Request Success",
                    ResponseData = new GetMessagesResponseData
                    {
                        Success = true,
                        ResponseMessage = "ok",
                        Messages = new List<Message> { backendMessage }
                    }
                });

            var service = new ChatService(authentication.Object, cache, network.Object);
            var group = new GroupDMSimple { GroupID = "group-1", Name = "Group", Owner = "u1" };

            var messages = await service.GetMessages(group);
            var cachedMessages = await cache.GetMessagesFromThread("group-1");

            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(messages[0].MessageID, Is.EqualTo("new-1"));
            Assert.That(cachedMessages.Count, Is.EqualTo(1));
            Assert.That(cachedMessages[0].MessageID, Is.EqualTo("new-1"));
        }

        [Test]
        public async Task OnReceiveMessage_CachesMessageAndRaisesOnMessageReceivedEvent()
        {
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            var authentication = CreateAuthenticationMock("u1");
            var service = new ChatService(authentication.Object, cache, network.Object);
            var message = new Message
            {
                ID = "event-1",
                ThreadID = "group-1",
                FromUser = new UserSimple { UserID = "u2", UserName = "Bob" },
                MessageContents = "hello event",
                MessageType = (int)MessageType.GroupMessage,
                TimeStamp = 30
            };
            var eventTcs = new TaskCompletionSource<MessageCache>();
            service.OnMessageReceived += cachedMessage => eventTcs.TrySetResult(cachedMessage);

            service.OnReceiveMessage(message);
            var raisedMessage = await eventTcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
            var cachedMessages = await cache.GetMessagesFromThread("group-1");

            Assert.That(raisedMessage.MessageID, Is.EqualTo("event-1"));
            Assert.That(cachedMessages.Count, Is.EqualTo(1));
            Assert.That(cachedMessages[0].MessageID, Is.EqualTo("event-1"));
        }

        private static Mock<IAuthenticationService> CreateAuthenticationMock(string userId)
        {
            var authentication = new Mock<IAuthenticationService>();
            authentication.SetupGet(service => service.CurrentUser).Returns(new User { UserID = userId, Username = "current-user" });
            return authentication;
        }
    }
}
