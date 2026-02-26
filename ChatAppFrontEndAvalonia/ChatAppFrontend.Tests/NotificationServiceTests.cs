using ChatApp.Services;
using ChatApp.Shared.Enums;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using ChatApp.Source.Services;
using ChatAppFrontEnd.Source.Services;
using ChatAppFrontEnd.Source.Services.Concrete;
using Moq;
using Newtonsoft.Json;

namespace ChatAppFrontend.Tests
{
    public class NotificationServiceTests
    {
        [Test]
        public void HandleNotification_WhenJsonIsInvalid_ReturnsFalse()
        {
            var service = CreateService(out _, out _, out _);

            var result = service.HandleNotification("{invalid-json");

            Assert.That(result, Is.False);
        }

        [Test]
        public void HandleNotification_WhenFriendRequest_InvokesFriendServiceHandler()
        {
            var service = CreateService(out var friendService, out _, out _);
            var payload = new FriendRequestNotification
            {
                FromUser = new UserSimple { UserID = "u1", UserName = "alice" },
                ToUserName = "bob"
            };

            var result = service.HandleNotification(CreateNotificationJson(NotificationType.FriendRequest, payload));

            Assert.That(result, Is.True);
            friendService.Verify(x => x.OnReceiveFriendRequestNotification(
                It.Is<FriendRequestNotification>(n => n.FromUser.UserID == "u1" && n.ToUserName == "bob")), Times.Once);
        }

        [Test]
        public void HandleNotification_WhenDirectMessage_InvokesChatServiceHandler()
        {
            var service = CreateService(out _, out var chatService, out _);
            var payload = new Message
            {
                ID = "m1",
                ThreadID = "t1",
                FromUser = new UserSimple { UserID = "u2", UserName = "sender" },
                MessageContents = "hello",
                MessageType = 0,
                TimeStamp = 1
            };

            var result = service.HandleNotification(CreateNotificationJson(NotificationType.DirectMessage, payload));

            Assert.That(result, Is.True);
            chatService.Verify(x => x.OnReceiveMessage(It.Is<Message>(m => m.ID == "m1" && m.ThreadID == "t1")), Times.Once);
        }

        [Test]
        public void HandleNotification_WhenGroupDeleted_InvokesGroupServiceWithGroupDeletedReason()
        {
            var service = CreateService(out _, out _, out var groupService);
            var payload = new GroupDMSimple { GroupID = "g1", Name = "group", Owner = "u1" };

            var result = service.HandleNotification(CreateNotificationJson(NotificationType.GroupDeleted, payload));

            Assert.That(result, Is.True);
            groupService.Verify(x => x.UpdateGroupLocally(
                It.Is<GroupDMSimple>(g => g.GroupID == "g1"),
                GroupUpdateReason.GroupDeleted), Times.Once);
        }

        private static NotificationService CreateService(
            out Mock<IFriendService> friendService,
            out Mock<IChatService> chatService,
            out Mock<IGroupService> groupService)
        {
            friendService = new Mock<IFriendService>();
            chatService = new Mock<IChatService>();
            groupService = new Mock<IGroupService>();

            var friend = friendService;
            var chat = chatService;
            var group = groupService;

            return new NotificationService(
                new Lazy<IFriendService>(() => friend.Object),
                new Lazy<IChatService>(() => chat.Object),
                new Lazy<IGroupService>(() => group.Object));
        }

        private static string CreateNotificationJson(NotificationType notificationType, object payload)
        {
            return JsonConvert.SerializeObject(new NotificationData
            {
                NotificationType = (int)notificationType,
                RecipientUserID = "recipient",
                NotificationJson = JsonConvert.SerializeObject(payload)
            });
        }
    }
}
