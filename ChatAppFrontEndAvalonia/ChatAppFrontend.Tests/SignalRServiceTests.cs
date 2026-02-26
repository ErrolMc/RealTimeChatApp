using ChatApp.Shared;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Services;
using ChatAppFrontEnd.Source.Services.Concrete;
using Moq;

namespace ChatAppFrontend.Tests
{
    public class SignalRServiceTests
    {
        [Test]
        public async Task ConnectToSignalR_WhenUserIsNull_ReturnsFailureWithoutHubConnection()
        {
            var notificationService = new Mock<INotificationService>();
            var networkCaller = new Mock<INetworkCallerService>();
            var service = new SignalRService(notificationService.Object, networkCaller.Object);

            var (success, message) = await service.ConnectToSignalR(null!);

            Assert.That(success, Is.False);
            Assert.That(message, Is.EqualTo("User is null"));
            Assert.That(service.Connection, Is.Null);
            networkCaller.Verify(x => x.PerformBackendPostRequest<AuthenticateRequestData, AuthenticateResponseData>(
                It.IsAny<string>(),
                It.IsAny<AuthenticateRequestData>()), Times.Never);
        }

        [Test]
        public async Task ConnectToSignalR_WhenTokenRequestConnectionFails_PropagatesMessage()
        {
            var notificationService = new Mock<INotificationService>();
            var networkCaller = new Mock<INetworkCallerService>();
            networkCaller
                .Setup(x => x.PerformBackendPostRequest<AuthenticateRequestData, AuthenticateResponseData>(
                    EndpointNames.AUTHENTICATE_SIGNALR,
                    It.IsAny<AuthenticateRequestData>()))
                .ReturnsAsync(new BackendPostResponse<AuthenticateResponseData>
                {
                    ConnectionSuccess = false,
                    Message = "network down",
                    ResponseData = null!
                });

            var service = new SignalRService(notificationService.Object, networkCaller.Object);
            var user = new User { UserID = "u1", Username = "alice" };

            var (success, message) = await service.ConnectToSignalR(user);

            Assert.That(success, Is.False);
            Assert.That(message, Is.EqualTo("PerformTokenRequest - Request Failed: \nnetwork down"));
            Assert.That(service.Connection, Is.Null);
            networkCaller.Verify(x => x.PerformBackendPostRequest<AuthenticateRequestData, AuthenticateResponseData>(
                EndpointNames.AUTHENTICATE_SIGNALR,
                It.Is<AuthenticateRequestData>(d => d.UserID == "u1" && d.UserName == "alice")), Times.Once);
        }
    }
}
