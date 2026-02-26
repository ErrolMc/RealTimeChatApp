using ChatApp.Backend.Repositories;
using ChatApp.Backend.Services;
using ChatApp.Shared;
using ChatApp.Shared.Authentication;
using ChatApp.Shared.Friends;
using ChatApp.Shared.Messages;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using Moq;

namespace ChatApp.Backend.Tests
{
    public class RepositoryTests
    {
        [Test]
        public async Task Login_ReturnsFailureWhenUserLookupFails()
        {
            var queryMock = new Mock<QueryService>(null!);
            queryMock.Setup(service => service.GetUserFromUsername("missing"))
                     .ReturnsAsync(new Result<User>(ResultType.NotFound, "No user"));

            var repository = new LoginRepository(null!, queryMock.Object);

            var response = await repository.Login(new UserLoginData { UserName = "missing", Password = "x" });

            Assert.That(response.Status, Is.False);
            Assert.That(response.Message, Is.EqualTo("No user"));
            Assert.That(response.User, Is.Null);
        }

        [Test]
        public async Task Login_ReturnsFailureForWrongPassword()
        {
            var user = new User
            {
                UserID = "u1",
                Username = "alice",
                HashedPassword = PasswordHasher.HashPassword("correct-password")
            };

            var queryMock = new Mock<QueryService>(null!);
            queryMock.Setup(service => service.GetUserFromUsername("alice"))
                     .ReturnsAsync(Result<User>.Success(user));

            var repository = new LoginRepository(null!, queryMock.Object);

            var response = await repository.Login(new UserLoginData { UserName = "alice", Password = "wrong-password" });

            Assert.That(response.Status, Is.False);
            Assert.That(response.Message, Is.EqualTo("Wrong password!"));
            Assert.That(response.User, Is.Null);
        }

        [Test]
        public async Task Login_ReturnsSuccessAndTokenForValidCredentials()
        {
            var user = new User
            {
                UserID = "u1",
                Username = "alice",
                HashedPassword = PasswordHasher.HashPassword("correct-password")
            };

            var queryMock = new Mock<QueryService>(null!);
            queryMock.Setup(service => service.GetUserFromUsername("alice"))
                .ReturnsAsync(Result<User>.Success(user));

            var repository = new LoginRepository(null!, queryMock.Object);

            var response = await repository.Login(new UserLoginData { UserName = "alice", Password = "correct-password" });

            Assert.That(response.Status, Is.True);
            Assert.That(response.User, Is.SameAs(user));
            Assert.That(response.LoginToken, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task AutoLogin_ReturnsFailureForInvalidToken()
        {
            var repository = new LoginRepository(null!, null!);

            var response = await repository.AutoLogin(new AutoLoginData { Token = "invalid-token" });

            Assert.That(response.Status, Is.False);
            Assert.That(response.Message, Is.EqualTo("Couldnt process login token"));
        }

        [Test]
        public async Task AutoLogin_ReturnsSuccessForValidToken()
        {
            var user = new User { UserID = "u1", Username = "alice" };
            var token = LoginRepository.GenerateJwtToken("alice");

            var queryMock = new Mock<QueryService>(null!);
            queryMock.Setup(service => service.GetUserFromUsername("alice"))
                .ReturnsAsync(Result<User>.Success(user));

            var repository = new LoginRepository(null!, queryMock.Object);

            var response = await repository.AutoLogin(new AutoLoginData { Token = token });

            Assert.That(response.Status, Is.True);
            Assert.That(response.User, Is.SameAs(user));
            Assert.That(response.LoginToken, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task GetFriends_ReturnsUpToDateWhenVersionMatches()
        {
            var queryMock = new Mock<QueryService>(null!);
            queryMock.Setup(service => service.GetUserFromUserID("owner"))
                .ReturnsAsync(Result<User>.Success(new User { UserID = "owner", FriendsVNum = 4 }));

            var repository = new FriendsRepository(null!, queryMock.Object);

            var response = await repository.GetFriends(new GetFriendsRequestData { UserID = "owner", LocalVNum = 4 });

            Assert.That(response.Success, Is.True);
            Assert.That(response.HasUpdate, Is.False);
            Assert.That(response.Message, Is.EqualTo("Friends list up to date"));
        }

        [Test]
        public async Task GetFriends_ReturnsFriendListWhenVersionChanged()
        {
            var owner = new User { UserID = "owner", FriendsVNum = 5, Friends = new List<string> { "u1", "u2" } };
            var friends = new List<User>
            {
                new() { UserID = "u1", Username = "alice" },
                new() { UserID = "u2", Username = "bob" }
            };

            var queryMock = new Mock<QueryService>(null!);
            queryMock.Setup(service => service.GetUserFromUserID("owner"))
                .ReturnsAsync(Result<User>.Success(owner));
            queryMock.Setup(service => service.GetUsers(It.Is<List<string>>(ids => ids.Count == 2)))
                .ReturnsAsync(Result<List<User>>.Success(friends));

            var repository = new FriendsRepository(null!, queryMock.Object);

            var response = await repository.GetFriends(new GetFriendsRequestData { UserID = "owner", LocalVNum = 1 });

            Assert.That(response.Success, Is.True);
            Assert.That(response.HasUpdate, Is.True);
            Assert.That(response.VNum, Is.EqualTo(5));
            Assert.That(response.Friends.Select(friend => friend.UserID), Is.EqualTo(new[] { "u1", "u2" }));
        }

        [Test]
        public async Task GetMessages_ReturnsFailureWhenQueryFails()
        {
            var queryMock = new Mock<QueryService>(null!);
            queryMock.Setup(service => service.GetMessagesByThreadIDAfterTimeStamp("thread-1", 10))
                .ReturnsAsync(Result<List<Message>>.Failure("db error"));

            var repository = new MessagesRepository(null!, queryMock.Object);

            var response = await repository.GetMessages(new GetMessagesRequestData { ThreadID = "thread-1", LocalTimeStamp = 10 });

            Assert.That(response.Success, Is.False);
            Assert.That(response.ResponseMessage, Is.EqualTo("Failed to get messages for Thread thread-1"));
        }

        [Test]
        public async Task GetMessages_ReturnsMessagesOnSuccess()
        {
            var messages = new List<Message>
            {
                new()
                {
                    ID = "m1",
                    ThreadID = "thread-1",
                    FromUser = new UserSimple { UserID = "u1", UserName = "alice" },
                    MessageContents = "hello",
                    TimeStamp = 123
                }
            };

            var queryMock = new Mock<QueryService>(null!);
            queryMock.Setup(service => service.GetMessagesByThreadIDAfterTimeStamp("thread-1", 10))
                .ReturnsAsync(Result<List<Message>>.Success(messages));

            var repository = new MessagesRepository(null!, queryMock.Object);

            var response = await repository.GetMessages(new GetMessagesRequestData { ThreadID = "thread-1", LocalTimeStamp = 10 });

            Assert.That(response.Success, Is.True);
            Assert.That(response.ResponseMessage, Is.EqualTo("Success"));
            Assert.That(response.Messages, Is.SameAs(messages));
        }
    }
}
