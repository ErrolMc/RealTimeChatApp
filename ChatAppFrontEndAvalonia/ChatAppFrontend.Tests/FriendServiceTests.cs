using System.Linq;
using ChatApp.Shared;
using ChatApp.Shared.Friends;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Services;
using ChatAppFrontEnd.Source.Services.Concrete;
using Moq;

namespace ChatAppFrontend.Tests
{
    public class FriendServiceTests
    {
        [Test]
        public async Task SendFriendRequest_WhenUserNameIsCurrentUser_ReturnsFailureWithoutNetworkCall()
        {
            var authentication = CreateAuthenticationMock("u1", "alice");
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            var service = new FriendService(authentication.Object, cache, network.Object);

            var (success, message, user) = await service.SendFriendRequest("alice");

            Assert.That(success, Is.False);
            Assert.That(message, Is.EqualTo("Cant send a friend request to yourself!"));
            Assert.That(user, Is.Null);
            network.Verify(service => service.PerformBackendPostRequest<FriendRequestNotification, FriendRequestNotificationResponseData>(
                EndpointNames.SEND_FRIEND_REQUEST,
                It.IsAny<FriendRequestNotification>()), Times.Never);
        }

        [Test]
        public async Task SendFriendRequest_WhenNetworkFails_ReturnsRequestFailed()
        {
            var authentication = CreateAuthenticationMock("u1", "alice");
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            network
                .Setup(service => service.PerformBackendPostRequest<FriendRequestNotification, FriendRequestNotificationResponseData>(
                    EndpointNames.SEND_FRIEND_REQUEST,
                    It.IsAny<FriendRequestNotification>()))
                .ReturnsAsync(new BackendPostResponse<FriendRequestNotificationResponseData>
                {
                    ConnectionSuccess = false,
                    Message = "offline",
                    ResponseData = null!
                });

            var service = new FriendService(authentication.Object, cache, network.Object);

            var (success, message, user) = await service.SendFriendRequest("bob");

            Assert.That(success, Is.False);
            Assert.That(message, Is.EqualTo("Request failed"));
            Assert.That(user, Is.Null);
        }

        [Test]
        public async Task RespondToFriendRequest_WhenNetworkFails_ReturnsRequestFailed()
        {
            var authentication = CreateAuthenticationMock("u1", "alice");
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            network
                .Setup(service => service.PerformBackendPostRequest<RespondToFriendRequestData, RespondToFriendRequestResponseData>(
                    EndpointNames.RESPOND_TO_FRIEND_REQUEST,
                    It.IsAny<RespondToFriendRequestData>()))
                .ReturnsAsync(new BackendPostResponse<RespondToFriendRequestResponseData>
                {
                    ConnectionSuccess = false,
                    Message = "offline",
                    ResponseData = null!
                });

            var service = new FriendService(authentication.Object, cache, network.Object);

            var (success, message) = await service.RespondToFriendRequest("u2", "u1", true);

            Assert.That(success, Is.False);
            Assert.That(message, Is.EqualTo("Request failed"));
        }

        [Test]
        public async Task UnFriend_WhenSuccess_RemovesLocalFriendAndRaisesEvent()
        {
            var authentication = CreateAuthenticationMock("u1", "alice");
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            network
                .Setup(service => service.PerformBackendPostRequest<UnfriendNotification, GenericResponseData>(
                    EndpointNames.REMOVE_FRIEND,
                    It.IsAny<UnfriendNotification>()))
                .ReturnsAsync(new BackendPostResponse<GenericResponseData>
                {
                    ConnectionSuccess = true,
                    Message = "ok",
                    ResponseData = new GenericResponseData { Success = true, Message = "Removed" }
                });

            var service = new FriendService(authentication.Object, cache, network.Object);
            var friend = new UserSimple { UserID = "u2", UserName = "bob" };
            service.Friends.Add(friend);
            var directMessageThread = SharedStaticMethods.CreateHashedDirectMessageID("u1", "u2");
            await cache.AddThreads(new()
            {
                new() { ThreadID = directMessageThread, Type = 0, TimeStamp = 0 }
            });

            var unfriendedNotification = new UnfriendNotification();
            service.OnUnfriended += notification => unfriendedNotification = notification;

            var (success, message) = await service.UnFriend("u2");
            var cachedThreads = await cache.GetAllThreads();

            Assert.That(success, Is.True);
            Assert.That(message, Is.EqualTo("Removed"));
            Assert.That(service.Friends.Any(existing => existing.UserID == "u2"), Is.False);
            Assert.That(unfriendedNotification.FromUserID, Is.EqualTo("u2"));
            Assert.That(cachedThreads.Any(thread => thread.ThreadID == directMessageThread), Is.False);
        }

        [Test]
        public async Task GetFriendRequests_WhenSuccessful_UpdatesLocalCollections()
        {
            var authentication = CreateAuthenticationMock("u1", "alice");
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            var incoming = new List<UserSimple> { new() { UserID = "u2", UserName = "bob" } };
            var outgoing = new List<UserSimple> { new() { UserID = "u3", UserName = "charlie" } };
            network
                .Setup(service => service.PerformBackendPostRequest<UserSimple, GetFriendRequestsResponseData>(
                    EndpointNames.GET_FRIEND_REQUESTS,
                    It.IsAny<UserSimple>()))
                .ReturnsAsync(new BackendPostResponse<GetFriendRequestsResponseData>
                {
                    ConnectionSuccess = true,
                    Message = "ok",
                    ResponseData = new GetFriendRequestsResponseData
                    {
                        Success = true,
                        Message = "updated",
                        FriendRequests = incoming,
                        OutgoingFriendRequests = outgoing
                    }
                });

            var service = new FriendService(authentication.Object, cache, network.Object);

            var result = await service.GetFriendRequests();

            Assert.That(result, Is.True);
            Assert.That(service.FriendRequests.Select(user => user.UserID), Is.EqualTo(new[] { "u2" }));
            Assert.That(service.OutgoingFriendRequests.Select(user => user.UserID), Is.EqualTo(new[] { "u3" }));
        }

        [Test]
        public async Task UpdateFriendsList_WhenCurrentUserNull_ReturnsFalse()
        {
            var authentication = new Mock<IAuthenticationService>();
            authentication.SetupGet(service => service.CurrentUser).Returns((User)null!);
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            var service = new FriendService(authentication.Object, cache, network.Object);
            service.Friends.Add(new UserSimple { UserID = "u2", UserName = "bob" });

            var result = await service.UpdateFriendsList();

            Assert.That(result, Is.False);
            Assert.That(service.Friends, Is.Empty);
            network.Verify(service => service.PerformBackendPostRequest<GetFriendsRequestData, GetFriendsResponseData>(
                EndpointNames.GET_FRIENDS,
                It.IsAny<GetFriendsRequestData>()), Times.Never);
        }

        private static Mock<IAuthenticationService> CreateAuthenticationMock(string userID, string username)
        {
            var authentication = new Mock<IAuthenticationService>();
            authentication.SetupGet(service => service.CurrentUser).Returns(new User { UserID = userID, Username = username });
            return authentication;
        }
    }
}
