using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChatApp.Backend.Controllers;
using ChatApp.Backend.Repositories;
using ChatApp.Backend.Services;
using ChatApp.Shared.Authentication;
using ChatApp.Shared.Friends;
using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.Messages;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.TableDataSimple;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace ChatApp.Backend.Tests
{
    public class ControllerTests
    {
        [Test]
        public async Task Login_InvalidPayload_ReturnsInvalidUserData()
        {
            var controller = CreateControllerWithBody(
                new LoginController(
                    new Mock<LoginRepository>(null!, null!).Object,
                    new Mock<ILogger<LoginController>>().Object),
                "null");

            var result = await controller.Login();
            var payload = ExtractOkPayload<UserLoginResponseData>(result);

            Assert.That(payload.Status, Is.False);
            Assert.That(payload.Message, Is.EqualTo("Invalid user data"));
        }

        [Test]
        public async Task Login_ValidPayload_ReturnsRepositoryResponse()
        {
            var expected = new UserLoginResponseData { Status = true, Message = "Logged in successfully!" };
            var repoMock = new Mock<LoginRepository>(null!, null!);
            repoMock
                .Setup(repo => repo.Login(It.Is<UserLoginData>(data => data.UserName == "alice" && data.Password == "pass123")))
                .ReturnsAsync(expected);

            var controller = CreateControllerWithBody(
                new LoginController(
                    repoMock.Object,
                    new Mock<ILogger<LoginController>>().Object),
                JsonConvert.SerializeObject(new UserLoginData { UserName = "alice", Password = "pass123" }));

            var result = await controller.Login();
            var payload = ExtractOkPayload<UserLoginResponseData>(result);

            Assert.That(payload, Is.SameAs(expected));
            repoMock.Verify(repo => repo.Login(It.IsAny<UserLoginData>()), Times.Once);
        }

        [Test]
        public async Task SendFriendRequest_InvalidPayload_ReturnsInvalidRequestData()
        {
            var controller = CreateControllerWithBody(
                new FriendsController(
                    new Mock<FriendsRepository>(null!, null!).Object,
                    new Mock<NotificationService>(new HttpClient()).Object,
                    new Mock<ILogger<FriendsController>>().Object),
                "null");

            var result = await controller.SendFriendRequest();
            var payload = ExtractOkPayload<FriendRequestNotificationResponseData>(result);

            Assert.That(payload.Status, Is.False);
            Assert.That(payload.Message, Is.EqualTo("Invalid request data"));
        }

        [Test]
        public async Task GetFriends_ValidPayload_ReturnsRepositoryResponse()
        {
            var expected = new GetFriendsResponseData { Success = true, HasUpdate = true, VNum = 2, Message = "Success" };
            var repoMock = new Mock<FriendsRepository>(null!, null!);
            repoMock
                .Setup(repo => repo.GetFriends(It.Is<GetFriendsRequestData>(data => data.UserID == "user-1" && data.LocalVNum == 1)))
                .ReturnsAsync(expected);

            var controller = CreateControllerWithBody(
                new FriendsController(
                    repoMock.Object,
                    new Mock<NotificationService>(new HttpClient()).Object,
                    new Mock<ILogger<FriendsController>>().Object),
                JsonConvert.SerializeObject(new GetFriendsRequestData { UserID = "user-1", LocalVNum = 1 }));

            var result = await controller.GetFriends();
            var payload = ExtractOkPayload<GetFriendsResponseData>(result);

            Assert.That(payload, Is.SameAs(expected));
            repoMock.Verify(repo => repo.GetFriends(It.IsAny<GetFriendsRequestData>()), Times.Once);
        }

        [Test]
        public async Task GetGroupParticipants_EmptyGroupId_ReturnsInvalidRequestData()
        {
            var controller = CreateControllerWithBody(
                new GroupsController(
                    new Mock<GroupsRepository>(null!, null!).Object,
                    new Mock<NotificationService>(new HttpClient()).Object,
                    new Mock<ILogger<GroupsController>>().Object),
                "\"\"");

            var result = await controller.GetGroupParticipants();
            var payload = ExtractOkPayload<GetGroupParticipantsResponseData>(result);

            Assert.That(payload.Success, Is.False);
            Assert.That(payload.Message, Is.EqualTo("Invalid request data"));
        }

        [Test]
        public async Task GetGroupDMs_ValidPayload_ReturnsRepositoryResponse()
        {
            var expected = new GetGroupDMsResponseData { Success = true, Message = "Success" };
            var repoMock = new Mock<GroupsRepository>(null!, null!);
            repoMock
                .Setup(repo => repo.GetGroupDMs(It.Is<UserSimple>(data => data.UserID == "user-1")))
                .ReturnsAsync(expected);

            var controller = CreateControllerWithBody(
                new GroupsController(
                    repoMock.Object,
                    new Mock<NotificationService>(new HttpClient()).Object,
                    new Mock<ILogger<GroupsController>>().Object),
                JsonConvert.SerializeObject(new UserSimple { UserID = "user-1", UserName = "alice" }));

            var result = await controller.GetGroupDMs();
            var payload = ExtractOkPayload<GetGroupDMsResponseData>(result);

            Assert.That(payload, Is.SameAs(expected));
            repoMock.Verify(repo => repo.GetGroupDMs(It.IsAny<UserSimple>()), Times.Once);
        }

        [Test]
        public async Task SendMessage_InvalidPayload_ReturnsInvalidMessageData()
        {
            var controller = CreateControllerWithBody(
                new MessagesController(
                    new Mock<MessagesRepository>(null!, null!).Object,
                    new Mock<NotificationService>(new HttpClient()).Object,
                    new Mock<ILogger<MessagesController>>().Object),
                "null");

            var result = await controller.SendMessage();
            var payload = ExtractOkPayload<SendMessageResponseData>(result);

            Assert.That(payload.Success, Is.False);
            Assert.That(payload.NotificationSuccess, Is.False);
            Assert.That(payload.ResponseMessage, Is.EqualTo("Invalid message data"));
        }

        [Test]
        public async Task GetMessages_ValidPayload_ReturnsRepositoryResponse()
        {
            var expected = new GetMessagesResponseData { Success = true, ResponseMessage = "Success" };
            var repoMock = new Mock<MessagesRepository>(null!, null!);
            repoMock
                .Setup(repo => repo.GetMessages(It.Is<GetMessagesRequestData>(data => data.ThreadID == "thread-1" && data.LocalTimeStamp == 123)))
                .ReturnsAsync(expected);

            var controller = CreateControllerWithBody(
                new MessagesController(
                    repoMock.Object,
                    new Mock<NotificationService>(new HttpClient()).Object,
                    new Mock<ILogger<MessagesController>>().Object),
                JsonConvert.SerializeObject(new GetMessagesRequestData { ThreadID = "thread-1", LocalTimeStamp = 123 }));

            var result = await controller.GetMessages();
            var payload = ExtractOkPayload<GetMessagesResponseData>(result);

            Assert.That(payload, Is.SameAs(expected));
            repoMock.Verify(repo => repo.GetMessages(It.IsAny<GetMessagesRequestData>()), Times.Once);
        }

        [Test]
        public async Task AuthenticateSignalR_NullPayload_ReturnsBadRequestData()
        {
            var controller = CreateControllerWithBody(
                new AuthenticateSignalRController(
                    new Mock<ILogger<AuthenticateSignalRController>>().Object,
                    new Mock<IConfiguration>().Object),
                "null");

            var result = await controller.Run();
            var payload = ExtractOkPayload<AuthenticateResponseData>(result);

            Assert.That(payload.Status, Is.False);
            Assert.That(payload.Message, Is.EqualTo("Bad Request Data"));
            Assert.That(payload.AccessToken, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task AuthenticateSignalR_MissingUserData_ReturnsValidationError()
        {
            var request = new AuthenticateRequestData { UserID = string.Empty, UserName = string.Empty };
            var controller = CreateControllerWithBody(
                new AuthenticateSignalRController(
                    new Mock<ILogger<AuthenticateSignalRController>>().Object,
                    new Mock<IConfiguration>().Object),
                JsonConvert.SerializeObject(request));

            var result = await controller.Run();
            var payload = ExtractOkPayload<AuthenticateResponseData>(result);

            Assert.That(payload.Status, Is.False);
            Assert.That(payload.Message, Is.EqualTo("Username or password not provided"));
            Assert.That(payload.AccessToken, Is.Null);
        }

        [Test]
        public async Task AuthenticateSignalR_ValidPayload_ReturnsJwtWithExpectedClaims()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.SetupGet(config => config["ServiceUrls:BackendUri"]).Returns("https://backend.test");
            configMock.SetupGet(config => config["ServiceUrls:SignalRUri"]).Returns("https://signalr.test");

            var request = new AuthenticateRequestData { UserID = "user-1", UserName = "alice" };
            var controller = CreateControllerWithBody(
                new AuthenticateSignalRController(
                    new Mock<ILogger<AuthenticateSignalRController>>().Object,
                    configMock.Object),
                JsonConvert.SerializeObject(request));

            var result = await controller.Run();
            var payload = ExtractOkPayload<AuthenticateResponseData>(result);

            Assert.That(payload.Status, Is.True);
            Assert.That(payload.AccessToken, Is.Not.Null.And.Not.Empty);

            var token = new JwtSecurityTokenHandler().ReadJwtToken(payload.AccessToken);
            Assert.That(token.Claims.First(claim => claim.Type == ClaimTypes.Name).Value, Is.EqualTo("alice"));
            Assert.That(token.Claims.First(claim => claim.Type == "userid").Value, Is.EqualTo("user-1"));
        }

        private static TController CreateControllerWithBody<TController>(TController controller, string requestBody)
            where TController : ControllerBase
        {
            var context = new DefaultHttpContext();
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));
            context.Request.ContentType = "application/json";

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
            return controller;
        }

        private static T ExtractOkPayload<T>(IActionResult actionResult)
        {
            var okResult = actionResult as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.InstanceOf<T>());
            return (T)okResult.Value!;
        }
    }
}
