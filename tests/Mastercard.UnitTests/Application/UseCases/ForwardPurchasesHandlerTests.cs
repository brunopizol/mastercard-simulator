using Xunit;
using Moq;
using Mastercard.Application.Interfaces;
using Mastercard.Application.UseCases;
using Mastercard.Application.DTOs;
using Mastercard.Core.Entities;
using Mastercard.Core.Exceptions;

namespace Mastercard.UnitTests.Application.UseCases
{
    public class ForwardPurchasesHandlerTests
    {
        private readonly Mock<IPurchaseGenerator> _mockPurchaseGenerator;
        private readonly Mock<IForwardService> _mockForwardService;
        private readonly ForwardPurchasesHandler _handler;

        public ForwardPurchasesHandlerTests()
        {
            _mockPurchaseGenerator = new Mock<IPurchaseGenerator>();
            _mockForwardService = new Mock<IForwardService>();
            _handler = new ForwardPurchasesHandler(_mockPurchaseGenerator.Object, _mockForwardService.Object);
        }

        [Fact]
        public void Constructor_WithNullPurchaseGenerator_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new ForwardPurchasesHandler(null, _mockForwardService.Object));
        }

        [Fact]
        public void Constructor_WithNullForwardService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new ForwardPurchasesHandler(_mockPurchaseGenerator.Object, null));
        }

        [Fact]
        public async Task HandleAsync_WithEmptyUrl_ThrowsInvalidForwardRequestException()
        {
            // Arrange
            var request = new ForwardRequest { Url = "", Count = 1 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidForwardRequestException>(() => 
                _handler.HandleAsync(request, CancellationToken.None));
            
            Assert.Equal("Url is required in the request body", exception.Message);
        }

        [Fact]
        public async Task HandleAsync_WithNullUrl_ThrowsInvalidForwardRequestException()
        {
            // Arrange
            var request = new ForwardRequest { Url = null, Count = 1 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidForwardRequestException>(() => 
                _handler.HandleAsync(request, CancellationToken.None));
            
            Assert.Equal("Url is required in the request body", exception.Message);
        }

        [Fact]
        public async Task HandleAsync_WithWhitespaceUrl_ThrowsInvalidForwardRequestException()
        {
            // Arrange
            var request = new ForwardRequest { Url = "   ", Count = 1 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidForwardRequestException>(() => 
                _handler.HandleAsync(request, CancellationToken.None));
            
            Assert.Equal("Url is required in the request body", exception.Message);
        }

        [Fact]
        public async Task HandleAsync_WithValidUrl_ShouldGenerateAndForwardPurchases()
        {
            // Arrange
            var request = new ForwardRequest { Url = "https://webhook.example.com", Count = 3 };
            var mockPayload = new PurchasePayload { TransactionId = "123", CorrelationId = "456" };
            var mockResponse = new ForwardServiceResponse { StatusCode = 200, Body = "OK" };

            _mockPurchaseGenerator
                .Setup(x => x.Generate())
                .Returns(mockPayload);

            _mockForwardService
                .Setup(x => x.ForwardPurchaseAsync(It.IsAny<object>(), request.Url, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _handler.HandleAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(3, result.Forwarded);
            Assert.Equal(3, result.Sent.Count);
            Assert.Equal(3, result.Responses.Count);
            Assert.All(result.Responses, response => Assert.Equal(200, response.StatusCode));
        }

        [Fact]
        public async Task HandleAsync_WithCountLessThanOne_ShouldClampToOne()
        {
            // Arrange
            var request = new ForwardRequest { Url = "https://webhook.example.com", Count = 0 };
            var mockPayload = new PurchasePayload { TransactionId = "123" };
            var mockResponse = new ForwardServiceResponse { StatusCode = 200 };

            _mockPurchaseGenerator.Setup(x => x.Generate()).Returns(mockPayload);
            _mockForwardService.Setup(x => x.ForwardPurchaseAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _handler.HandleAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.Forwarded);
            _mockPurchaseGenerator.Verify(x => x.Generate(), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WithCountGreaterThan100_ShouldClampTo100()
        {
            // Arrange
            var request = new ForwardRequest { Url = "https://webhook.example.com", Count = 500 };
            var mockPayload = new PurchasePayload { TransactionId = "123" };
            var mockResponse = new ForwardServiceResponse { StatusCode = 200 };

            _mockPurchaseGenerator.Setup(x => x.Generate()).Returns(mockPayload);
            _mockForwardService.Setup(x => x.ForwardPurchaseAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _handler.HandleAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(100, result.Forwarded);
            _mockPurchaseGenerator.Verify(x => x.Generate(), Times.Exactly(100));
        }

        [Fact]
        public async Task HandleAsync_WhenForwardServiceThrowsException_ShouldCaptureErrorResponse()
        {
            // Arrange
            var request = new ForwardRequest { Url = "https://webhook.example.com", Count = 1 };
            var mockPayload = new PurchasePayload { TransactionId = "123" };
            var exceptionMessage = "Network timeout";

            _mockPurchaseGenerator.Setup(x => x.Generate()).Returns(mockPayload);
            _mockForwardService
                .Setup(x => x.ForwardPurchaseAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException(exceptionMessage));

            // Act
            var result = await _handler.HandleAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.Forwarded);
            Assert.Single(result.Responses);
            Assert.Equal(0, result.Responses[0].StatusCode);
            Assert.Contains(exceptionMessage, result.Responses[0].Body);
        }

        [Fact]
        public async Task HandleAsync_WithMultiplePurchases_ShouldCallGenerateMultipleTimes()
        {
            // Arrange
            var request = new ForwardRequest { Url = "https://webhook.example.com", Count = 5 };
            var mockPayload = new PurchasePayload { TransactionId = "123" };
            var mockResponse = new ForwardServiceResponse { StatusCode = 200 };

            _mockPurchaseGenerator.Setup(x => x.Generate()).Returns(mockPayload);
            _mockForwardService.Setup(x => x.ForwardPurchaseAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            // Act
            await _handler.HandleAsync(request, CancellationToken.None);

            // Assert
            _mockPurchaseGenerator.Verify(x => x.Generate(), Times.Exactly(5));
        }

        [Fact]
        public async Task HandleAsync_WithMultiplePurchases_ShouldCallForwardServiceMultipleTimes()
        {
            // Arrange
            var request = new ForwardRequest { Url = "https://webhook.example.com", Count = 3 };
            var mockPayload = new PurchasePayload { TransactionId = "123" };
            var mockResponse = new ForwardServiceResponse { StatusCode = 200 };

            _mockPurchaseGenerator.Setup(x => x.Generate()).Returns(mockPayload);
            _mockForwardService.Setup(x => x.ForwardPurchaseAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            // Act
            await _handler.HandleAsync(request, CancellationToken.None);

            // Assert
            _mockForwardService.Verify(
                x => x.ForwardPurchaseAsync(It.IsAny<object>(), request.Url, It.IsAny<CancellationToken>()), 
                Times.Exactly(3));
        }

        [Theory]
        [InlineData(200)]
        [InlineData(201)]
        [InlineData(400)]
        [InlineData(404)]
        [InlineData(500)]
        public async Task HandleAsync_ShouldReturnResponseWithCorrectStatusCode(int statusCode)
        {
            // Arrange
            var request = new ForwardRequest { Url = "https://webhook.example.com", Count = 1 };
            var mockPayload = new PurchasePayload { TransactionId = "123" };
            var mockResponse = new ForwardServiceResponse { StatusCode = statusCode, Body = "Test" };

            _mockPurchaseGenerator.Setup(x => x.Generate()).Returns(mockPayload);
            _mockForwardService.Setup(x => x.ForwardPurchaseAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _handler.HandleAsync(request, CancellationToken.None);

            // Assert
            Assert.Single(result.Responses);
            Assert.Equal(statusCode, result.Responses[0].StatusCode);
        }

        [Fact]
        public async Task HandleAsync_ShouldIncludeForwardedCountInResult()
        {
            // Arrange
            var request = new ForwardRequest { Url = "https://webhook.example.com", Count = 7 };
            var mockPayload = new PurchasePayload { TransactionId = "123" };
            var mockResponse = new ForwardServiceResponse { StatusCode = 200 };

            _mockPurchaseGenerator.Setup(x => x.Generate()).Returns(mockPayload);
            _mockForwardService.Setup(x => x.ForwardPurchaseAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _handler.HandleAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(7, result.Forwarded);
            Assert.Equal(result.Sent.Count, result.Forwarded);
        }
    }
}