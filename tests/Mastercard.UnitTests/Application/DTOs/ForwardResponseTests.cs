using Xunit;
using Mastercard.Application.DTOs;

namespace Mastercard.UnitTests.Application.DTOs
{
    public class ForwardResponseTests
    {
        [Fact]
        public void ForwardResponse_ShouldAllowSettingStatusCode()
        {
            // Act
            var response = new ForwardResponse { StatusCode = 200, Body = "OK" };

            // Assert
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public void ForwardResponse_ShouldAllowSettingBody()
        {
            // Arrange
            var body = "Response body content";

            // Act
            var response = new ForwardResponse { StatusCode = 200, Body = body };

            // Assert
            Assert.Equal(body, response.Body);
        }
    }
}