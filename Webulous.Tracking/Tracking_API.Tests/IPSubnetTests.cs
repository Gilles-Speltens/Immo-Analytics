using Common;
using Common.Exceptions;
using System.Net;


namespace Tracking_API.Tests
{
    public class IPSubnetTests
    {
        [Fact]
        public void Constructor_ShouldParseIPv4CIDR()
        {
            var subnet = new IPSubnet("192.168.0.0/24");
            Assert.True(subnet.Contains("192.168.0.1"));
            Assert.False(subnet.Contains("192.168.1.1"));
        }

        [Fact]
        public void Constructor_ShouldParseIPv6CIDR()
        {
            var subnet = new IPSubnet("2001:db8::/32");
            Assert.True(subnet.Contains("2001:db8::1"));
            Assert.False(subnet.Contains("2001:db9::1"));
        }

        [Fact]
        public void Constructor_ShouldHandleSingleIPv4()
        {
            var subnet = new IPSubnet("10.0.0.1");
            Assert.True(subnet.Contains("10.0.0.1"));
            Assert.False(subnet.Contains("10.0.0.2"));
        }

        [Fact]
        public void Constructor_ShouldHandleSingleIPv6()
        {
            var subnet = new IPSubnet("2001:db8::1");
            Assert.True(subnet.Contains("2001:db8::1"));
            Assert.False(subnet.Contains("2001:db8::2"));
        }

        [Fact]
        public void Constructor_InvalidFormat_ShouldThrow()
        {
            Assert.Throws<FormatException>(() => new IPSubnet("abc"));
            Assert.Throws<InvalidIpException>(() => new IPSubnet("192.168.0.0/33"));
        }

        [Fact]
        public void Contains_NullAddress_ShouldThrow()
        {
            var subnet = new IPSubnet("192.168.0.0/24");
            Assert.Throws<ArgumentNullException>(() => subnet.Contains((byte[])null));
        }

        [Fact]
        public void Contains_ByteArray_ShouldMatchCorrectly()
        {
            var subnet = new IPSubnet("192.168.1.0/24");
            var addressIn = IPAddress.Parse("192.168.1.5").GetAddressBytes();
            var addressOut = IPAddress.Parse("192.168.2.5").GetAddressBytes();

            Assert.True(subnet.Contains(addressIn));
            Assert.False(subnet.Contains(addressOut));

            var ipv6 = new IPSubnet("2001:db8::/32");
            var ipv6Address = IPAddress.Parse("2001:db8::1").GetAddressBytes();
            var ipv6Wrong = IPAddress.Parse("2001:db9::1").GetAddressBytes();

            Assert.True(ipv6.Contains(ipv6Address));
            Assert.False(ipv6.Contains(ipv6Wrong));

            // mismatch IPv4 vs IPv6
            Assert.False(ipv6.Contains(addressIn));
        }

        [Fact]
        public void Matches_ShouldReturnTrue_ForExactIPv4WithPrefix()
        {
            var subnet = new IPSubnet("192.168.1.1/32");

            bool result = subnet.Matches("192.168.1.1/32");

            Assert.True(result);
        }

        [Fact]
        public void Matches_ShouldReturnTrue_ForIPv4WithoutPrefix()
        {
            var subnet = new IPSubnet("192.168.1.1/32");

            bool result = subnet.Matches("192.168.1.1");

            Assert.True(result); // /32 par défaut
        }

        [Fact]
        public void Matches_ShouldReturnFalse_ForDifferentIPv4()
        {
            var subnet = new IPSubnet("192.168.1.1/32");

            bool result = subnet.Matches("192.168.1.2");

            Assert.False(result);
        }

        [Fact]
        public void Matches_ShouldReturnTrue_ForExactIPv6WithPrefix()
        {
            var subnet = new IPSubnet("2001:db8::1/128");

            bool result = subnet.Matches("2001:db8::1/128");

            Assert.True(result);
        }

        [Fact]
        public void Matches_ShouldReturnTrue_ForIPv6WithoutPrefix()
        {
            var subnet = new IPSubnet("2001:db8::1/128");

            bool result = subnet.Matches("2001:db8::1");

            Assert.True(result); // /128 par défaut
        }

        [Fact]
        public void Matches_ShouldReturnFalse_ForDifferentIPv6()
        {
            var subnet = new IPSubnet("2001:db8::1/128");

            bool result = subnet.Matches("2001:db8::2");

            Assert.False(result);
        }

        [Fact]
        public void Matches_ShouldReturnFalse_ForInvalidFormat()
        {
            var subnet = new IPSubnet("192.168.1.1/32");

            bool result1 = subnet.Matches("invalid-ip");
            bool result2 = subnet.Matches("192.168.1.1/abc");

            Assert.False(result1);
            Assert.False(result2);
        }

        [Fact]
        public void Matches_ShouldReturnFalse_ForNullOrWhitespace()
        {
            var subnet = new IPSubnet("192.168.1.1/32");

            Assert.False(subnet.Matches(null));
            Assert.False(subnet.Matches(""));
            Assert.False(subnet.Matches("   "));
        }

        [Fact]
        public void Matches_ShouldRespectPrefix_WhenProvided()
        {
            var subnet = new IPSubnet("192.168.1.1/24");

            Assert.True(subnet.Matches("192.168.1.1/24"));
            Assert.False(subnet.Matches("192.168.1.1/32"));
        }
    }
}
