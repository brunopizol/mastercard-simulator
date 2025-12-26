using Mastercard.Application.DTOs;
using Mastercard.Core.Entities;
using Mastercard.Infrastructure.Http;
using Moq;
using Moq.Protected;
using System.Net.Http.Json;
using Xunit;

namespace Mastercard.UnitTests.Infrastructure.Http
{
    public class HttpForwardServiceTests
    {
        [Fact]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new HttpForwardService(null));
        }

        [Fact]
        public async Task ForwardPurchaseAsync_WithSuccessfulResponse_ReturnStatusCodeAndBody()
        {
            // Arrange
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var mockHttpClient = new HttpClient(new MockHttpMessageHandler(200, "Success"));
            var service = new HttpForwardService(mockHttpClient);
            var payload = new PurchasePayload { TransactionId = "123" };
            var url = "https://webhook.example.com";

            // Act
            var result = await service.ForwardPurchaseAsync(payload, url, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Body);
        }

        [Fact]
        public async Task ForwardPurchaseAsync_WithExceptionThrown_ReturnStatusCode0AndErrorMessage()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(mockHandler.Object);
            var service = new HttpForwardService(httpClient);
            var payload = new PurchasePayload { TransactionId = "123" };
            var url = "https://webhook.example.com";

            // Act
            var result = await service.ForwardPurchaseAsync(payload, url, CancellationToken.None);

            // Assert
            Assert.Equal(0, result.StatusCode);
            Assert.Contains("Network error", result.Body);
        }
    }

    /// <summary>
    /// Mock para simular respostas HTTP
    /// </summary>
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly int _statusCode;
        private readonly string _content;

        public MockHttpMessageHandler(int statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage((System.Net.HttpStatusCode)_statusCode)
            {
                Content = new StringContent(_content)
            };
            return Task.FromResult(response);
        }
    }
}