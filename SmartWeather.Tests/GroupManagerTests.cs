using Core.Enums;
using FluentAssertions;
using Interfaces.Repositories;
using Managers;
using Models.requests;
using Models.SqlEntities;
using Moq;

namespace SmartWeather.Tests
{
    public class GroupManagerTests
    {
        private readonly Mock<IGroupRepository> _groupRepositoryMock;
        private readonly GroupManager _sut;

        public GroupManagerTests()
        {
            _groupRepositoryMock = new Mock<IGroupRepository>();
            _sut = new GroupManager(_groupRepositoryMock.Object);
        }

        [Fact]
        public async Task GetGroupByIdAsync_ShouldReturnGroup_WhenGroupExists()
        {
            var groupId = 1;
            var expectedGroup = new Group()
            {
                Id = groupId,
                Name = "Test Group",
                Description = "A group for testing",
                CreatedAt = DateTime.UtcNow,
                Devices = new List<Device>()
                {
                    new Device { Id = 1, SerialNumber = "Device 1" },
                    new Device { Id = 2, SerialNumber = "Device 2" }
                }
            };

            _groupRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<Func<IQueryable<Group>, IQueryable<Group>>>())).ReturnsAsync(expectedGroup);

            var result = await _sut.GetGroupByIdAsync(groupId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(groupId);
            result!.NumberOfDevices.Should().Be(2);
            result.Name.Should().Be("Test Group");

            _groupRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<Func<IQueryable<Group>, IQueryable<Group>>>()), Times.Once);
        }

        [Fact]
        public async Task GetGroupByIdAsync_ShouldReturnNull_WhenGroupDoesNotExists()
        {
            var groupId = -1;
           
            _groupRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<Func<IQueryable<Group>, IQueryable<Group>>>())).ReturnsAsync((Group?)null);

            var result = await _sut.GetGroupByIdAsync(groupId);

            result.Should().BeNull();

            _groupRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<Func<IQueryable<Group>, IQueryable<Group>>>()), Times.Once);
        }

        [Fact]
        public async Task GetAllGroupsAsync_ShouldReturnGroups_ForUser()
        {
            var userId = "13f46udf5938daf";
            var groups = new List<Group>()
            {
                new Group()
                {
                    Id = 1,
                    Name = "Group 1",
                    Description = "First group",
                    CreatedAt = DateTime.UtcNow,
                    Devices = new List<Device>()
                    {
                        new Device { Id = 1, SerialNumber = "Device 1" }
                    }
                },
                new Group()
                {
                    Id = 2,
                    Name = "Group 2",
                    Description = "Second group",
                    CreatedAt = DateTime.UtcNow,
                    Devices = new List<Device>()
                    {
                        new Device { Id = 2, SerialNumber = "Device 2" },
                        new Device { Id = 3, SerialNumber = "Device 3" }
                    }
                }
            };

            _groupRepositoryMock.Setup(x => x.GetCurrentLoggedUserGroups(userId)).ReturnsAsync(groups);

            var result = await _sut.GetAllGroupsAsync(userId);

            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.ElementAt(0).NumberOfDevices.Should().Be(1);
            result.ElementAt(1).NumberOfDevices.Should().Be(2);
            result.Should().BeEquivalentTo(groups, options => options.ExcludingMissingMembers());

            _groupRepositoryMock.Verify(x => x.GetCurrentLoggedUserGroups(userId), Times.Once);
        }

        [Fact]
        public async Task GetAllGroupsAsync_ShouldReturnEmptyList_WhenUserHasNoGroups()
        {
            _groupRepositoryMock.Setup(x => x.GetCurrentLoggedUserGroups(It.IsAny<string>()))
                                .ReturnsAsync(new List<Group>());

            var result = await _sut.GetAllGroupsAsync("any-id");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AddGroupAsync_ShoudlReturnTrue_WhenAddedSuccessfully()
        {
            var req = new CreateGroupReq
            {
                Name = "New Group",
                Description = "Group Description"
            };
            var userId = "user-123";

            _groupRepositoryMock.Setup(x => x.AddAsync(It.Is<Group>(g =>
                g.Name == req.Name &&
                g.Description == req.Description &&
                g.Memberships.Any(m => m.ApplicationUserId == userId && m.Role == GroupRole.Admin)
            ))).ReturnsAsync(true);

            var result = await _sut.AddGroupAsync(req, userId);

            result.Should().BeTrue();

            _groupRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Group>()), Times.Once);
        }
        [Fact]
        public async Task AddGroupAsync_ShouldReturnFalse_WhenRepositoryFails()
        {
            var req = new CreateGroupReq { Name = "Fail Group", Description = "Fail" };
            var userId = "user-123";

            _groupRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Group>()))
                .ReturnsAsync(false);

            var result = await _sut.AddGroupAsync(req, userId);

            result.Should().BeFalse();

            _groupRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Group>()), Times.Once);
        }

        [Fact]
        public async Task DeleteGroupAsync_ShouldReturnTrue_WhenDeleteSuccessfully()
        {
            var groupId = 1;

            var group = new Group
            {
                Id = groupId,
                Devices = new List<Device>()
            };

            _groupRepositoryMock.Setup(x=>x.GetByIdAsync(groupId,It.IsAny<Func<IQueryable<Group>,IQueryable<Group>>>())).ReturnsAsync(group);

            _groupRepositoryMock.Setup(x => x.DeleteAsync(group)).ReturnsAsync(true);

            var result = await _sut.DeleteGroupAsync(groupId);

            result.Success.Should().BeTrue();

            _groupRepositoryMock.Verify(x => x.DeleteAsync(group), Times.Once);
        }

        [Fact]
        public async Task DeleteGroupAsync_ShouldReturnFalse_WhenGroupDoesNotExists()
        {
            var groupId = -1;

            _groupRepositoryMock.Setup(x => x.GetByIdAsync(groupId, It.IsAny<Func<IQueryable<Group>, IQueryable<Group>>>())).ReturnsAsync((Group?)null);

            var result = await _sut.DeleteGroupAsync(groupId);

            result.Success.Should().BeFalse();

            _groupRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Group>()), Times.Never);
        }

        [Fact]
        public async Task DeleteGroupAsync_ShouldReturnTrue_WhenGroupHasDevices()
        {
            var groupId = 1;

            var group = new Group
            {
                Id = groupId,
                Devices = new List<Device>
                {
                    new Device { Id = 1, SerialNumber = "Device 1" }
                }
            };

            _groupRepositoryMock.Setup(x => x.GetByIdAsync(groupId, It.IsAny<Func<IQueryable<Group>, IQueryable<Group>>>())).ReturnsAsync(group);

            var result = await _sut.DeleteGroupAsync(groupId);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("Group has associated devices and cannot be deleted");

            _groupRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Group>()), Times.Never);
        }

        [Fact]
        public async Task UpdateGroupAsync_ShouldReturnTrue_WhenUpdateSuccessfully()
        {
            var groupId = 1;
            var req = new CreateGroupReq
            {
                Name = "Updated Group",
                Description = "Updated Description"
            };
            var existingGroup = new Group
            {
                Id = groupId,
                Name = "Old Group",
                Description = "Old Description"
            };

            _groupRepositoryMock.Setup(x => x.GetByIdAsync(groupId, It.IsAny<Func<IQueryable<Group>, IQueryable<Group>>>())).ReturnsAsync(existingGroup);

            _groupRepositoryMock.Setup(x => x.UpdateAsync(existingGroup)).ReturnsAsync(true);

            var result = await _sut.UpdateGroupAsync(groupId, req);

            result.Should().BeTrue();

            existingGroup.Name.Should().Be(req.Name);
            existingGroup.Description.Should().Be(req.Description);

            _groupRepositoryMock.Verify(x => x.UpdateAsync(existingGroup), Times.Once);
        }

        [Fact]
        public async Task UpdateGroupsAsync_ShouldReturnFalse_WhenGroupDoesNotExists()
        {
            var groupId = -1;

            _groupRepositoryMock.Setup(x => x.GetByIdAsync(groupId, It.IsAny<Func<IQueryable<Group>, IQueryable<Group>>>())).ReturnsAsync((Group?)null);

            var result = await _sut.UpdateGroupAsync(groupId, new CreateGroupReq { Name = "Name", Description = "Desc" });

            result.Should().BeFalse();

            _groupRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Group>()), Times.Never);
        }
    }
}
