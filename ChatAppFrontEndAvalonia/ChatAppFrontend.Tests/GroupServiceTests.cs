using System.Linq;
using ChatApp.Shared;
using ChatApp.Shared.Enums;
using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.Messages;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Other.Caching.Data;
using ChatAppFrontEnd.Source.Services;
using ChatAppFrontEnd.Source.Services.Concrete;
using Moq;

namespace ChatAppFrontend.Tests
{
    public class GroupServiceTests
    {
        [Test]
        public async Task CreateGroupDM_WhenSuccessful_AddsGroupLocally()
        {
            var authentication = CreateAuthenticationMock("u1", "alice");
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            var createdGroup = new GroupDMSimple { GroupID = "g1", Name = "group", Owner = "u1" };
            network
                .Setup(service => service.PerformBackendPostRequest<CreateGroupDMRequestData, CreateGroupDMResponseData>(
                    EndpointNames.CREATE_GROUP_DM,
                    It.IsAny<CreateGroupDMRequestData>()))
                .ReturnsAsync(new BackendPostResponse<CreateGroupDMResponseData>
                {
                    ConnectionSuccess = true,
                    Message = "ok",
                    ResponseData = new CreateGroupDMResponseData
                    {
                        CreatedGroupSuccess = true,
                        Message = "created",
                        GroupDMSimple = createdGroup
                    }
                });

            var service = new GroupService(authentication.Object, cache, network.Object);
            var groupUpdated = false;
            service.OnGroupDMsUpdated += () => groupUpdated = true;

            var (success, message, groupDMSimple) = await service.CreateGroupDM(new List<string> { "u2" });
            var cachedThreads = await cache.GetAllThreads();

            Assert.That(success, Is.True);
            Assert.That(message, Is.EqualTo("created"));
            Assert.That(groupDMSimple!.GroupID, Is.EqualTo("g1"));
            Assert.That(service.GroupDMs.Any(group => group.GroupID == "g1"), Is.True);
            Assert.That(groupUpdated, Is.True);
            Assert.That(cachedThreads.Any(thread => thread.ThreadID == "g1" && thread.Type == (int)MessageType.GroupMessage), Is.True);
        }

        [Test]
        public async Task CreateGroupDM_WhenConnectionFails_ReturnsFailure()
        {
            var authentication = CreateAuthenticationMock("u1", "alice");
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            network
                .Setup(service => service.PerformBackendPostRequest<CreateGroupDMRequestData, CreateGroupDMResponseData>(
                    EndpointNames.CREATE_GROUP_DM,
                    It.IsAny<CreateGroupDMRequestData>()))
                .ReturnsAsync(new BackendPostResponse<CreateGroupDMResponseData>
                {
                    ConnectionSuccess = false,
                    Message = "offline",
                    ResponseData = null!
                });

            var service = new GroupService(authentication.Object, cache, network.Object);

            var (success, message, groupDMSimple) = await service.CreateGroupDM(new List<string> { "u2" });

            Assert.That(success, Is.False);
            Assert.That(message, Is.EqualTo("offline"));
            Assert.That(groupDMSimple, Is.Null);
            Assert.That(service.GroupDMs, Is.Empty);
        }

        [Test]
        public async Task DeleteGroup_WhenSuccessful_RemovesLocalGroup()
        {
            var authentication = CreateAuthenticationMock("u1", "alice");
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            network
                .Setup(service => service.PerformBackendPostRequest<string, DeleteGroupDMResponseData>(
                    EndpointNames.DELETE_GROUP,
                    "g1"))
                .ReturnsAsync(new BackendPostResponse<DeleteGroupDMResponseData>
                {
                    ConnectionSuccess = true,
                    Message = "ok",
                    ResponseData = new DeleteGroupDMResponseData
                    {
                        Success = true,
                        Message = "deleted"
                    }
                });

            var service = new GroupService(authentication.Object, cache, network.Object);
            service.AddGroupLocally(new GroupDMSimple { GroupID = "g1", Name = "group", Owner = "u1" });

            var (success, message) = await service.DeleteGroup("g1");
            var cachedThreads = await cache.GetAllThreads();

            Assert.That(success, Is.True);
            Assert.That(message, Is.EqualTo("deleted"));
            Assert.That(service.GroupDMs.Any(group => group.GroupID == "g1"), Is.False);
            Assert.That(cachedThreads.Any(thread => thread.ThreadID == "g1"), Is.False);
        }

        [Test]
        public async Task RemoveUserFromGroup_WhenReasonIsUserLeft_RaisesThisUserLeft()
        {
            var authentication = CreateAuthenticationMock("u1", "alice");
            var cache = new MockCachingService();
            var network = new Mock<INetworkCallerService>();
            network
                .Setup(service => service.PerformBackendPostRequest<RemoveFromGroupRequestData, RemoveFromGroupResponseData>(
                    EndpointNames.REMOVE_USER_FROM_GROUP,
                    It.IsAny<RemoveFromGroupRequestData>()))
                .ReturnsAsync(new BackendPostResponse<RemoveFromGroupResponseData>
                {
                    ConnectionSuccess = true,
                    Message = "ok",
                    ResponseData = new RemoveFromGroupResponseData
                    {
                        Success = true,
                        Message = "updated",
                        GroupName = "renamed"
                    }
                });

            var service = new GroupService(authentication.Object, cache, network.Object);
            var group = new GroupDMSimple { GroupID = "g1", Name = "group", Owner = "u1" };
            service.AddGroupLocally(group);
            GroupUpdateReason? updateReason = null;
            service.OnGroupUpdated += update => updateReason = update.reason;

            var (success, message) = await service.RemoveUserFromGroup("u1", group, GroupUpdateReason.UserLeft);
            var cachedThreads = await cache.GetAllThreads();

            Assert.That(success, Is.True);
            Assert.That(message, Is.EqualTo("updated"));
            Assert.That(updateReason, Is.EqualTo(GroupUpdateReason.ThisUserLeft));
            Assert.That(group.Name, Is.EqualTo("renamed"));
            Assert.That(service.GroupDMs.Any(existing => existing.GroupID == "g1"), Is.False);
            Assert.That(cachedThreads.Any(thread => thread.ThreadID == "g1"), Is.False);
        }

        [Test]
        public async Task UpdateGroupDMList_WhenSuccessful_UpdatesGroupsAndThreadCache()
        {
            var authentication = CreateAuthenticationMock("u1", "alice");
            var cache = new MockCachingService();
            await cache.AddThreads(new List<ThreadCache>
            {
                new() { ThreadID = "g-existing", Type = (int)MessageType.GroupMessage, TimeStamp = 10 },
                new() { ThreadID = "g-stale", Type = (int)MessageType.GroupMessage, TimeStamp = 5 },
                new() { ThreadID = "dm-thread", Type = (int)MessageType.DirectMessage, TimeStamp = 15 }
            });

            var network = new Mock<INetworkCallerService>();
            network
                .Setup(service => service.PerformBackendPostRequest<UserSimple, GetGroupDMsResponseData>(
                    EndpointNames.GET_GROUP_DMS,
                    It.IsAny<UserSimple>()))
                .ReturnsAsync(new BackendPostResponse<GetGroupDMsResponseData>
                {
                    ConnectionSuccess = true,
                    Message = "ok",
                    ResponseData = new GetGroupDMsResponseData
                    {
                        Success = true,
                        Message = "updated",
                        GroupDMs = new List<GroupDMSimple>
                        {
                            new() { GroupID = "g-existing", Name = "Existing", Owner = "u1" },
                            new() { GroupID = "g-new", Name = "New", Owner = "u2" }
                        }
                    }
                });

            var service = new GroupService(authentication.Object, cache, network.Object);

            var result = await service.UpdateGroupDMList();
            var cachedThreads = await cache.GetAllThreads();

            Assert.That(result, Is.True);
            Assert.That(service.GroupDMs.Select(group => group.GroupID), Is.EqualTo(new[] { "g-existing", "g-new" }));
            Assert.That(cachedThreads.Any(thread => thread.ThreadID == "g-existing"), Is.True);
            Assert.That(cachedThreads.Any(thread => thread.ThreadID == "g-new"), Is.True);
            Assert.That(cachedThreads.Any(thread => thread.ThreadID == "g-stale"), Is.False);
            Assert.That(cachedThreads.Any(thread => thread.ThreadID == "dm-thread"), Is.True);
        }

        private static Mock<IAuthenticationService> CreateAuthenticationMock(string userID, string username)
        {
            var authentication = new Mock<IAuthenticationService>();
            authentication.SetupGet(service => service.CurrentUser).Returns(new User { UserID = userID, Username = username });
            return authentication;
        }
    }
}
