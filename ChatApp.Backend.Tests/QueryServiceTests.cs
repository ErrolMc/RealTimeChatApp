using ChatApp.Backend.Services;
using ChatApp.Shared;
using ChatApp.Shared.Tables;
using Microsoft.Azure.Cosmos;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;
using User = ChatApp.Shared.Tables.User;

namespace ChatApp.Backend.Tests
{
    public class QueryServiceTests
    {
        [Test]
        public async Task GetUsers_NullIds_ReturnsInputError()
        {
            var queryService = new QueryService(null!);

            var result = await queryService.GetUsers(null!);

            Assert.That(result.ResultType, Is.EqualTo(ResultType.InputError));
            Assert.That(result.ErrorMessage, Is.EqualTo("No user ids provided"));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.Empty);
        }

        [Test]
        public async Task GetUsers_EmptyIds_ReturnsInputError()
        {
            var queryService = new QueryService(null!);

            var result = await queryService.GetUsers(new List<string>());

            Assert.That(result.ResultType, Is.EqualTo(ResultType.InputError));
            Assert.That(result.ErrorMessage, Is.EqualTo("No user ids provided"));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.Empty);
        }

        [Test]
        public async Task GetChatThreadsFromIDs_EmptyIds_ReturnsInputError()
        {
            var queryService = new QueryService(null!);

            var result = await queryService.GetChatThreadsFromIDs(new List<string>());

            Assert.That(result.ResultType, Is.EqualTo(ResultType.InputError));
            Assert.That(result.ErrorMessage, Is.EqualTo("No group ids provided"));
        }

        [Test]
        public async Task GetUsers_AllUsersFound_ReturnsSuccess()
        {
            var requestedIds = new List<string> { "u1", "u2" };
            var returnedUsers = new List<User>
            {
                new() { ID = "u1", UserID = "u1", Username = "Alice" },
                new() { ID = "u2", UserID = "u2", Username = "Bob" }
            };

            var userIterator = CreateIterator(returnedUsers);
            var usersContainerMock = new Mock<Container>();
            usersContainerMock
                .Setup(container => container.GetItemQueryIterator<User>(
                    It.IsAny<QueryDefinition>(),
                    It.IsAny<string>(),
                    It.IsAny<QueryRequestOptions>()))
                .Returns(userIterator);

            var queryService = new QueryService(CreateDatabaseService(usersContainer: usersContainerMock.Object));

            var result = await queryService.GetUsers(requestedIds);

            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.Data.Select(user => user.UserID), Is.EqualTo(new[] { "u1", "u2" }));
        }

        [Test]
        public async Task GetChatThreadsFromIDs_WhenThreadsFound_ReturnsSuccess()
        {
            var requestedIds = new List<string> { "t1", "t2" };
            var returnedThreads = new List<ChatThread>
            {
                new() { ID = "t1", Name = "One", Users = new List<string>() },
                new() { ID = "t2", Name = "Two", Users = new List<string>() }
            };

            var threadIterator = CreateIterator(returnedThreads);
            var threadContainerMock = new Mock<Container>();
            threadContainerMock
                .Setup(container => container.GetItemQueryIterator<ChatThread>(
                    It.IsAny<QueryDefinition>(),
                    It.IsAny<string>(),
                    It.IsAny<QueryRequestOptions>()))
                .Returns(threadIterator);

            var queryService = new QueryService(CreateDatabaseService(chatThreadsContainer: threadContainerMock.Object));

            var result = await queryService.GetChatThreadsFromIDs(requestedIds);

            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.Data.Select(thread => thread.ID), Is.EqualTo(new[] { "t1", "t2" }));
        }

        private static DatabaseService CreateDatabaseService(
            Container? usersContainer = null,
            Container? messagesContainer = null,
            Container? chatThreadsContainer = null)
        {
            var db = (DatabaseService)RuntimeHelpers.GetUninitializedObject(typeof(DatabaseService));
            SetAutoProperty(db, "UsersContainer", usersContainer ?? new Mock<Container>().Object);
            SetAutoProperty(db, "MessagesContainer", messagesContainer ?? new Mock<Container>().Object);
            SetAutoProperty(db, "ChatThreadsContainer", chatThreadsContainer ?? new Mock<Container>().Object);
            return db;
        }

        private static FeedIterator<T> CreateIterator<T>(IEnumerable<T> items)
        {
            var feedResponseMock = new Mock<FeedResponse<T>>();
            feedResponseMock
                .Setup(response => response.GetEnumerator())
                .Returns(() => items.ToList().GetEnumerator());

            var iteratorMock = new Mock<FeedIterator<T>>();
            iteratorMock.SetupSequence(iterator => iterator.HasMoreResults)
                .Returns(true)
                .Returns(false);
            iteratorMock
                .Setup(iterator => iterator.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(feedResponseMock.Object);
            return iteratorMock.Object;
        }

        private static void SetAutoProperty<T>(DatabaseService db, string propertyName, T value)
        {
            var field = typeof(DatabaseService).GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Backing field for {propertyName} not found.");
            field!.SetValue(db, value);
        }
    }
}
