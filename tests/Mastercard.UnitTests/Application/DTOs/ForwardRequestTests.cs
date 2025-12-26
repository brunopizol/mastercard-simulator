using Xunit;
using Mastercard.Application.DTOs;

namespace Mastercard.UnitTests.Application.DTOs
{
    public class ForwardRequestTests
    {
        [Fact]
        public void ForwardRequest_ShouldHaveDefaultCountOfOne()
        {
            // Act
            var request = new ForwardRequest { Url = "https://example.com" };

            // Assert
            Assert.Equal(1, request.Count);
        }

        [Fact]
        public void ForwardRequest_ShouldAllowSettingUrl()
        {
            // Arrange
            var url = "https://webhook.example.com";

            // Act
            var request = new ForwardRequest { Url = url };

            // Assert
            Assert.Equal(url, request.Url);
        }

        [Fact]
        public void ForwardRequest_ShouldAllowSettingCount()
        {
            // Arrange
            var count = 10;

            // Act
            var request = new ForwardRequest { Url = "https://example.com", Count = count };

            // Assert
            Assert.Equal(count, request.Count);
        }
    }
}