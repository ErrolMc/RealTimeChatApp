using ChatApp.Shared;
using ChatApp.Shared.Authentication;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Services;
using ChatAppFrontEnd.Source.Services.Concrete;
using Moq;

namespace ChatAppFrontend.Tests
{
    public class AuthenticationServiceTests
    {
        [Test]
        public async Task TryRegister_WhenConnectionFails_ReturnsFailure()
        {
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            network
                .Setup(service => service.PerformBackendPostRequest<UserLoginData, UserLoginResponseData>(
                    EndpointNames.REGISTER,
                    It.IsAny<UserLoginData>()))
                .ReturnsAsync(new BackendPostResponse<UserLoginResponseData>
                {
                    ConnectionSuccess = false,
                    Message = "network error",
                    ResponseData = null!
                });

            var service = new AuthenticationService(cache, network.Object);

            var (success, message) = await service.TryRegister("alice", "pass");

            Assert.That(success, Is.False);
            Assert.That(message, Is.EqualTo("network error"));
        }

        [Test]
        public async Task TryRegister_WhenBackendSucceeds_ReturnsSuccess()
        {
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            network
                .Setup(service => service.PerformBackendPostRequest<UserLoginData, UserLoginResponseData>(
                    EndpointNames.REGISTER,
                    It.IsAny<UserLoginData>()))
                .ReturnsAsync(new BackendPostResponse<UserLoginResponseData>
                {
                    ConnectionSuccess = true,
                    Message = "Request Success",
                    ResponseData = new UserLoginResponseData { Status = true, Message = "registered" }
                });

            var service = new AuthenticationService(cache, network.Object);

            var (success, message) = await service.TryRegister("alice", "pass");

            Assert.That(success, Is.True);
            Assert.That(message, Is.EqualTo("registered"));
        }

        [Test]
        public async Task TryLogin_WhenBackendReturnsFailure_DoesNotCacheToken()
        {
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            network
                .Setup(service => service.PerformBackendPostRequest<UserLoginData, UserLoginResponseData>(
                    EndpointNames.LOGIN,
                    It.IsAny<UserLoginData>()))
                .ReturnsAsync(new BackendPostResponse<UserLoginResponseData>
                {
                    ConnectionSuccess = true,
                    Message = "Request Success",
                    ResponseData = new UserLoginResponseData { Status = false, Message = "Wrong password!", User = null!, LoginToken = string.Empty }
                });

            var service = new AuthenticationService(cache, network.Object);

            var (success, message, user) = await service.TryLogin("alice", "bad-pass");
            var (hasToken, _) = await cache.GetLoginToken();
            var isLoggedIn = await cache.GetIsLoggedIn();

            Assert.That(success, Is.False);
            Assert.That(message, Is.EqualTo("Wrong password!"));
            Assert.That(user, Is.Null);
            Assert.That(hasToken, Is.False);
            Assert.That(isLoggedIn, Is.False);
        }

        [Test]
        public async Task TryLogin_WhenBackendReturnsSuccess_CachesTokenAndLoginState()
        {
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            var user = new User { UserID = "u1", Username = "alice" };
            network
                .Setup(service => service.PerformBackendPostRequest<UserLoginData, UserLoginResponseData>(
                    EndpointNames.LOGIN,
                    It.IsAny<UserLoginData>()))
                .ReturnsAsync(new BackendPostResponse<UserLoginResponseData>
                {
                    ConnectionSuccess = true,
                    Message = "Request Success",
                    ResponseData = new UserLoginResponseData
                    {
                        Status = true,
                        Message = "Logged in successfully!",
                        User = user,
                        LoginToken = "token-123"
                    }
                });

            var service = new AuthenticationService(cache, network.Object);

            var (success, message, returnedUser) = await service.TryLogin("alice", "pass");
            var (hasToken, token) = await cache.GetLoginToken();
            var isLoggedIn = await cache.GetIsLoggedIn();

            Assert.That(success, Is.True);
            Assert.That(message, Is.EqualTo("Logged in successfully!"));
            Assert.That(returnedUser, Is.SameAs(user));
            Assert.That(hasToken, Is.True);
            Assert.That(token, Is.EqualTo("token-123"));
            Assert.That(isLoggedIn, Is.True);
        }

        [Test]
        public async Task TryAutoLogin_WhenSuccess_UsesAutoLoginEndpointAndCachesToken()
        {
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            network
                .Setup(service => service.PerformBackendPostRequest<AutoLoginData, UserLoginResponseData>(
                    EndpointNames.AUTO_LOGIN,
                    It.IsAny<AutoLoginData>()))
                .ReturnsAsync(new BackendPostResponse<UserLoginResponseData>
                {
                    ConnectionSuccess = true,
                    Message = "Request Success",
                    ResponseData = new UserLoginResponseData
                    {
                        Status = true,
                        Message = "Logged in successfully!",
                        User = new User { UserID = "u2", Username = "bob" },
                        LoginToken = "auto-token"
                    }
                });

            var service = new AuthenticationService(cache, network.Object);

            var (success, _, _) = await service.TryAutoLogin("incoming-token");
            var (hasToken, token) = await cache.GetLoginToken();

            Assert.That(success, Is.True);
            network.Verify(service => service.PerformBackendPostRequest<AutoLoginData, UserLoginResponseData>(
                EndpointNames.AUTO_LOGIN,
                It.IsAny<AutoLoginData>()), Times.Once);
            Assert.That(hasToken, Is.True);
            Assert.That(token, Is.EqualTo("auto-token"));
        }

        [Test]
        public async Task TryLogout_ClearsCacheAndRaisesEvent()
        {
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            await cache.SaveLoginToken("token-123");
            await cache.SaveIsLoggedIn(true);

            var service = new AuthenticationService(cache, network.Object);
            var onLogoutRaised = false;
            service.OnLogout += () => onLogoutRaised = true;

            var result = await service.TryLogout();
            var (hasToken, _) = await cache.GetLoginToken();
            var isLoggedIn = await cache.GetIsLoggedIn();

            Assert.That(result, Is.True);
            Assert.That(onLogoutRaised, Is.True);
            Assert.That(hasToken, Is.False);
            Assert.That(isLoggedIn, Is.False);
        }
    }
}
