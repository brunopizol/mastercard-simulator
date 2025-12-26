using Xunit;
using Mastercard.Infrastructure.Simulation;
using Mastercard.Core.Entities;

namespace Mastercard.UnitTests.Infrastructure.Simulation
{
    public class PurchaseGeneratorTests
    {
        private readonly PurchaseGenerator _generator;

        public PurchaseGeneratorTests()
        {
            _generator = new PurchaseGenerator();
        }

        [Fact]
        public void Generate_ShouldReturnValidPurchasePayload()
        {
            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.NotNull(payload);
            Assert.NotEmpty(payload.TransactionId);
            Assert.NotEmpty(payload.CorrelationId);
            Assert.NotEmpty(payload.CardId);
            Assert.NotEmpty(payload.Pan);
            Assert.NotEmpty(payload.PanHash);
            Assert.NotEmpty(payload.Account);
            Assert.NotEmpty(payload.Merchant);
        }

        [Fact]
        public void Generate_ShouldCreateValidPan()
        {
            // Act
            var payload = _generator.Generate();

            // Assert
            // PAN deve ter 16 dígitos
            Assert.Equal(16, payload.Pan.Length);
            
            // Deve ser apenas números
            Assert.True(payload.Pan.All(char.IsDigit), "PAN deve conter apenas dígitos");
            
            // Deve começar com 5 (Mastercard)
            Assert.True(payload.Pan.StartsWith("5"), "PAN deve começar com 5 (Mastercard)");
            
            // Checksum Luhn deve ser válido
            Assert.True(IsValidLuhn(payload.Pan), "PAN deve ter checksum Luhn válido");
        }

        [Fact]
        public void Generate_ShouldCreateValidPanHash()
        {
            // Act
            var payload = _generator.Generate();

            // Assert
            // SHA256 gera hash de 64 caracteres (hexadecimal)
            Assert.Equal(64, payload.PanHash.Length);
            
            // Deve conter apenas caracteres hexadecimais
            Assert.True(payload.PanHash.All(c => "0123456789abcdef".Contains(c)), 
                "PAN Hash deve ser válido hexadecimal");
        }

        [Fact]
        public void Generate_ShouldHaveValidAmount()
        {
            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.True(payload.Amount > 0, "Amount deve ser maior que 0");
            Assert.True(payload.Amount <= 9999.99m, "Amount deve ser no máximo 9999.99");
            
            // Deve ter no máximo 2 casas decimais
            var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(payload.Amount)[3])[2];
            Assert.True(decimalPlaces <= 2, "Amount deve ter no máximo 2 casas decimais");
        }

        [Theory]
        [InlineData("BRL")]
        [InlineData("USD")]
        [InlineData("EUR")]
        [InlineData("GBP")]
        [InlineData("ARS")]
        public void Generate_ShouldHaveValidCurrency(string expectedCurrency)
        {
            // Arrange
            var validCurrencies = new[] { "BRL", "USD", "EUR", "GBP", "ARS" };

            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.Contains(payload.Currency, validCurrencies);
        }

        [Fact]
        public void Generate_ShouldHaveValidMerchant()
        {
            // Arrange
            var validMerchants = new[] { "Loja Exemplo", "Mercado Central", "E-commerce XYZ", "Restaurante Bom Sabor" };

            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.Contains(payload.Merchant, validMerchants);
        }

        [Fact]
        public void Generate_ShouldHaveValidCardExpiration()
        {
            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.NotEmpty(payload.CardExpiration);
            
            // Formato: MM/yy
            var parts = payload.CardExpiration.Split('/');
            Assert.Equal(2, parts.Length);
            Assert.True(int.TryParse(parts[0], out var month) && month >= 1 && month <= 12, 
                "Mês deve estar entre 01 e 12");
            Assert.True(int.TryParse(parts[1], out var year) && year > 0, 
                "Ano deve ser válido");
        }

        [Fact]
        public void Generate_ShouldHaveValidCardHolder()
        {
            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.NotEmpty(payload.CardHolder);
            
            // Deve ter nome e sobrenome
            var parts = payload.CardHolder.Split(' ');
            Assert.True(parts.Length >= 2, "CardHolder deve ter primeiro e último nome");
        }

        [Fact]
        public void Generate_ShouldHaveValidInstallments()
        {
            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.True(payload.Installments >= 1, "Installments deve ser pelo menos 1");
            Assert.True(payload.Installments <= 12, "Installments deve ser no máximo 12");
        }

        [Fact]
        public void Generate_ShouldHaveUniqueTransactionId()
        {
            // Act
            var payload1 = _generator.Generate();
            var payload2 = _generator.Generate();

            // Assert
            Assert.NotEqual(payload1.TransactionId, payload2.TransactionId);
        }

        [Fact]
        public void Generate_ShouldHaveUniqueCorrelationId()
        {
            // Act
            var payload1 = _generator.Generate();
            var payload2 = _generator.Generate();

            // Assert
            Assert.NotEqual(payload1.CorrelationId, payload2.CorrelationId);
        }

        [Fact]
        public void Generate_ShouldHaveUniqueNsu()
        {
            // Act
            var payload1 = _generator.Generate();
            var payload2 = _generator.Generate();

            // Assert
            Assert.NotEqual(payload1.Nsu, payload2.Nsu);
        }

        [Fact]
        public void Generate_ShouldHaveValidAdditionalData()
        {
            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.NotNull(payload.AdditionalData);
            Assert.Contains("processor", payload.AdditionalData.Keys);
            Assert.Contains("testMode", payload.AdditionalData.Keys);
            Assert.Equal("MastercardSimulator", payload.AdditionalData["processor"]);
            Assert.Equal("true", payload.AdditionalData["testMode"]);
        }

        [Fact]
        public void Generate_ShouldHaveConstantMerchantCountryBR()
        {
            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.Equal("BR", payload.MerchantCountry);
        }

        [Fact]
        public void Generate_ShouldHaveConstantEntryModeCHIP()
        {
            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.Equal("CHIP", payload.EntryMode);
        }
        [Fact]
        public void Generate_MultipleGenerations_ShouldAllBeValid()
        {
            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                var payload = _generator.Generate();

                Assert.NotNull(payload);
                Assert.Equal(16, payload.Pan.Length);
                Assert.StartsWith("5", payload.Pan); // Mastercard começa com 5
                Assert.True(IsValidLuhn(payload.Pan), $"PAN {payload.Pan} falhou na validação Luhn");
            }
        }

        private static bool IsValidLuhn(string pan)
        {
            if (string.IsNullOrEmpty(pan) || pan.Length != 16)
                return false;

            int sum = 0;
            bool alternate = false;

            for (int i = pan.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(pan[i]))
                    return false;

                int digit = pan[i] - '0';

                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                }

                sum += digit;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }

      
    }
}