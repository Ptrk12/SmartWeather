using FluentAssertions;
using Interfaces.Repositories;
using Managers;
using Models.SqlEntities;
using Moq;

namespace SmartWeather.Tests
{
    public class GroupTests
    {
        private readonly Mock<IGroupRepository> _groupRepositoryMock;
        private readonly GroupManager _groupManager;

        public GroupTests()
        {
            _groupRepositoryMock = new Mock<IGroupRepository>();
            _groupManager = new GroupManager(_groupRepositoryMock.Object);
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

            var result = await _groupManager.GetGroupByIdAsync(groupId);

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

            var result = await _groupManager.GetGroupByIdAsync(groupId);

            result.Should().BeNull();

            _groupRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<Func<IQueryable<Group>, IQueryable<Group>>>()), Times.Once);
        }
    }
}
