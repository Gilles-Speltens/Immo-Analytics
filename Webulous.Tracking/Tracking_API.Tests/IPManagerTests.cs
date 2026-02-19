using Moq;
using Tracking_API.Model;

namespace Tracking_API.Tests
{
    public class IPManagerTests
    {
        private IPManager CreateManager(Mock<IFileManager> mock, string[] initialData = null)
        {
            mock.Setup(f => f.ReadFile())
                .Returns(initialData ?? Array.Empty<string>());

            mock.Setup(f => f.OverwriteFromList(It.IsAny<List<string>>()));

            return new IPManager(mock.Object);
        }

        [Fact]
        public void AddIp_ShouldAddIp_WhenNotExists()
        {
            var mock = new Mock<IFileManager>();
            var manager = CreateManager(mock);

            manager.AddIpToSafeList("192.168.1.1/32");

            Assert.Contains("192.168.1.1", manager.GetSafeList());
        }

        [Fact]
        public void AddIpv4_ShouldAddIp_WithImplicitePrefix()
        {
            var mock = new Mock<IFileManager>();
            var manager = CreateManager(mock);

            manager.AddIpToSafeList("192.168.1.1");

            Assert.Contains("192.168.1.1", manager.GetSafeList());
        }

        [Fact]
        public void AddIpv6_ShouldAddIp_WithImplicitePrefix()
        {
            var mock = new Mock<IFileManager>();
            var manager = CreateManager(mock);

            manager.AddIpToSafeList("2001:db8::1");

            Assert.Contains("2001:db8::1", manager.GetSafeList());
        }

        [Fact]
        public void AddIp_ShouldNotAddDuplicate()
        {
            var mock = new Mock<IFileManager>();
            var manager = CreateManager(mock, new[] { "192.168.1.1" });

            manager.AddIpToSafeList("192.168.1.1");

            Assert.Single(manager.GetSafeList());
        }

        [Fact]
        public void AddIpv4_WithImplicitePrefix_ShouldNotAddDuplicate()
        {
            var mock = new Mock<IFileManager>();
            var manager = CreateManager(mock, new[] { "192.168.1.1" });

            manager.AddIpToSafeList("192.168.1.1/32");

            Assert.Single(manager.GetSafeList());
        }

        [Fact]
        public void AddIpv6_WithImplicitePrefix_ShouldNotAddDuplicate()
        {
            var mock = new Mock<IFileManager>();
            var manager = CreateManager(mock, new[] { "2001:db8::1" });

            manager.AddIpToSafeList("2001:db8::1/128");

            Assert.Single(manager.GetSafeList());
        }

        [Fact]
        public void RemoveIp_ShouldRemoveIPv4_WithImplicitPrefix()
        {
            var mock = new Mock<IFileManager>();
            var manager = CreateManager(mock, new[] { "192.168.1.1" });

            manager.RemoveIpFromSafeList("192.168.1.1/32");

            Assert.Empty(manager.GetSafeList());
        }

        [Fact]
        public void RemoveIp_ShouldRemoveIPv6_WithImplicitPrefix()
        {
            var mock = new Mock<IFileManager>();
            var manager = CreateManager(mock, new[] { "2001:db8::1" });

            manager.RemoveIpFromSafeList("2001:db8::1/128");

            Assert.Empty(manager.GetSafeList());
        }

        [Fact]
        public void RemoveIp_ShouldRemoveExactSubnet()
        {
            var mock = new Mock<IFileManager>();
            var manager = CreateManager(mock, new[] { "192.168.1.0" });

            manager.RemoveIpFromSafeList("192.168.1.0");

            Assert.Empty(manager.GetSafeList());
        }

        [Fact]
        public void RemoveIp_ShouldRemoveOnlyTheExactIP()
        {
            var mock = new Mock<IFileManager>();
            var manager = CreateManager(mock, new[] { "192.168.1.1", "192.168.1.2", "192.168.1.3" });

            manager.RemoveIpFromSafeList("192.168.1.2");

            Assert.Contains("192.168.1.1", manager.GetSafeList());
            Assert.Contains("192.168.1.3", manager.GetSafeList());
            Assert.DoesNotContain("192.168.1.2", manager.GetSafeList());
        }

        [Fact]
        public void IsInSafeList_ShouldReturnTrue_WhenIpMatchesSubnet()
        {
            var mock = new Mock<IFileManager>();
            var manager = CreateManager(mock, new[] { "192.168.1.0/24" });

            Assert.True(manager.IsInSafeList("192.168.1.5"));
            Assert.False(manager.IsInSafeList("192.168.2.5"));
        }

        [Fact]
        public void GetSafeList_ShouldReturnAllIps()
        {
            var mock = new Mock<IFileManager>();
            var data = new[] { "192.168.1.1", "10.0.0.0/8" };
            var manager = CreateManager(mock, data);

            var result = manager.GetSafeList();

            Assert.Equal(2, result.Length);
            Assert.Contains("192.168.1.1", result);
            Assert.Contains("10.0.0.0/8", result);
        }

        [Fact]
        public void AddIp_ShouldCallSaveToFile()
        {
            var mock = new Mock<IFileManager>();
            var manager = CreateManager(mock);

            manager.AddIpToSafeList("192.168.1.1");

            mock.Verify(f => f.OverwriteFromList(It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public void RemoveIp_ShouldCallSaveToFile()
        {
            var mock = new Mock<IFileManager>();
            var manager = CreateManager(mock, new[] { "192.168.1.1" });
            mock.Invocations.Clear();

            manager.RemoveIpFromSafeList("192.168.1.1");

            mock.Verify(f => f.OverwriteFromList(It.IsAny<List<string>>()), Times.Once);
        }
    }
}
