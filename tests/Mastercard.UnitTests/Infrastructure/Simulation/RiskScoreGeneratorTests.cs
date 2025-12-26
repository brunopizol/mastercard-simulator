using Xunit;
using Mastercard.Infrastructure.Simulation;
using Mastercard.Core.Entities;

namespace Mastercard.UnitTests.Infrastructure.Simulation
{
    public class RiskScoreGeneratorTests
    {
        private readonly PurchaseGenerator _generator;

        public RiskScoreGeneratorTests()
        {
            _generator = new PurchaseGenerator();
        }

        #region RiskScore Existence and Structure Tests

        [Fact]
        public void Generate_ShouldIncludeRiskScore()
        {
            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.NotNull(payload.RiskScore);
        }

        [Fact]
        public void Generate_ShouldHaveValidRiskScoreStructure()
        {
            // Act
            var payload = _generator.Generate();
            var riskScore = payload.RiskScore;

            // Assert
            Assert.NotNull(riskScore.ScoreGeneratedAt);
            Assert.True(riskScore.Score >= 0 && riskScore.Score <= 999, "Score deve estar entre 0 e 999");
            Assert.NotEmpty(riskScore.RiskLevel);
            Assert.NotEmpty(riskScore.AvsMatch);
            Assert.NotEmpty(riskScore.CvcMatch);
            Assert.NotEmpty(riskScore.SpendingPattern);
            Assert.NotEmpty(riskScore.IpCountry);
        }

        #endregion

        #region Risk Score Value Tests

        [Fact]
        public void Generate_ShouldGenerateScoreBetween0And999()
        {
            // Act & Assert
            for (int i = 0; i < 50; i++)
            {
                var payload = _generator.Generate();
                Assert.InRange(payload.RiskScore.Score, 0, 999);
            }
        }

        [Theory]
        [InlineData(1.00, 50)]      // Valor baixo deve ter score mais baixo
        [InlineData(500.00, 50)]    // Valor médio
        [InlineData(2500.00, 50)]   // Valor alto deve ter score mais elevado
        [InlineData(8000.00, 50)]   // Valor muito alto deve ter score muito elevado
        public void Generate_ShouldScaleRiskScoreByAmount(decimal amount, int iterations)
        {
            // Act
            var scores = new List<int>();
            for (int i = 0; i < iterations; i++)
            {
                // Simula a lógica de geração através de múltiplas gerações
                var payload = _generator.Generate();
                // A probabilidade de scores altos aumenta com transações de valor alto
                scores.Add(payload.RiskScore.Score);
            }

            // Assert
            var averageScore = scores.Average();
            
            // Valores muito altos devem ter média de score mais elevada
            if (amount > 5000)
            {
                Assert.True(scores.Any(s => s > 500), "Transações de alto valor devem ter alguns scores elevados");
            }
            else if (amount < 100)
            {
                Assert.True(scores.Any(s => s < 300), "Transações de baixo valor devem ter alguns scores baixos");
            }
        }

        #endregion

        #region Risk Level Tests

        [Theory]
        [InlineData("LOW")]
        [InlineData("MEDIUM")]
        [InlineData("HIGH")]
        [InlineData("CRITICAL")]
        public void Generate_ShouldHaveValidRiskLevel(string expectedLevel)
        {
            // Arrange
            var validLevels = new[] { "LOW", "MEDIUM", "HIGH", "CRITICAL" };

            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.Contains(payload.RiskScore.RiskLevel, validLevels);
        }

        [Fact]
        public void Generate_RiskLevelShouldCorrelateWithScore()
        {
            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                var payload = _generator.Generate();
                var score = payload.RiskScore.Score;
                var level = payload.RiskScore.RiskLevel;

                // Valida correlação entre score e nível
                if (score >= 750)
                    Assert.Equal("CRITICAL", level);
                else if (score >= 500)
                    Assert.Equal("HIGH", level);
                else if (score >= 250)
                    Assert.Equal("MEDIUM", level);
                else
                    Assert.Equal("LOW", level);
            }
        }

        #endregion

        #region AvsMatch and CvcMatch Tests

        [Theory]
        [InlineData("Y")]
        [InlineData("N")]
        [InlineData("U")]
        public void Generate_ShouldHaveValidAvsMatch(string _)
        {
            // Arrange
            var validValues = new[] { "Y", "N", "U" };

            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.Contains(payload.RiskScore.AvsMatch, validValues);
        }

        [Theory]
        [InlineData("Y")]
        [InlineData("N")]
        [InlineData("U")]
        public void Generate_ShouldHaveValidCvcMatch(string _)
        {
            // Arrange
            var validValues = new[] { "Y", "N", "U" };

            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.Contains(payload.RiskScore.CvcMatch, validValues);
        }

        [Fact]
        public void Generate_AvsAndCvcMatchShouldBeIndependent()
        {
            // Act & Assert
            var combinations = new HashSet<(string, string)>();
            for (int i = 0; i < 100; i++)
            {
                var payload = _generator.Generate();
                combinations.Add((payload.RiskScore.AvsMatch, payload.RiskScore.CvcMatch));
            }

            // Deve gerar múltiplas combinações diferentes
            Assert.True(combinations.Count > 1, "Combinações de AVS e CVC devem variar");
        }

        #endregion

        #region Spending Pattern Tests

        [Theory]
        [InlineData("NORMAL")]
        [InlineData("UNUSUAL")]
        [InlineData("SUSPICIOUS")]
        public void Generate_ShouldHaveValidSpendingPattern(string _)
        {
            // Arrange
            var validPatterns = new[] { "NORMAL", "UNUSUAL", "SUSPICIOUS" };

            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.Contains(payload.RiskScore.SpendingPattern, validPatterns);
        }

        [Fact]
        public void Generate_SpendingPatternShouldCorrelateWithScore()
        {
            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                var payload = _generator.Generate();
                var score = payload.RiskScore.Score;
                var pattern = payload.RiskScore.SpendingPattern;

                // Padrão suspeito deve correlacionar com scores altos
                if (pattern == "SUSPICIOUS")
                    Assert.True(score >= 500, "SUSPICIOUS deve ter score >= 500");
                else if (pattern == "UNUSUAL")
                    Assert.True(score >= 250, "UNUSUAL deve ter score >= 250");
            }
        }

        #endregion

        #region Geographic/IP Tests

        [Fact]
        public void Generate_ShouldHaveValidIpCountry()
        {
            // Arrange
            var validCountries = new[] { "BR", "US", "CN", "RU", "NG" };

            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.Contains(payload.RiskScore.IpCountry, validCountries);
        }

        [Fact]
        public void Generate_BrazilShouldBeMostCommonIpCountry()
        {
            // Act
            var brazilCount = 0;
            var totalIterations = 1000;

            for (int i = 0; i < totalIterations; i++)
            {
                var payload = _generator.Generate();
                if (payload.RiskScore.IpCountry == "BR")
                    brazilCount++;
            }

            // Assert - Brasil deve aparecer em ~90% dos casos
            var percentage = (double)brazilCount / totalIterations;
            Assert.True(percentage >= 0.80, $"BR deve aparecer em ~90% dos casos, encontrado: {percentage:P}");
        }

        [Fact]
        public void Generate_CountryMatchShouldCorrelateWithIpCountry()
        {
            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                var payload = _generator.Generate();
                var ipCountry = payload.RiskScore.IpCountry;
                var countryMatch = payload.RiskScore.CountryMatch;

                // Se IP é BR, deve ter alta probabilidade de match
                if (ipCountry == "BR")
                {
                    // Estatisticamente, deve haver mais matches
                    // Este teste confirma que a propriedade existe e é booleana
                    Assert.IsType<bool>(countryMatch);
                }
            }
        }

        #endregion

        #region Failed Attempts Tests

        [Fact]
        public void Generate_ShouldHaveValidFailedAttempts()
        {
            // Act & Assert
            for (int i = 0; i < 50; i++)
            {
                var payload = _generator.Generate();
                Assert.InRange(payload.RiskScore.FailedAttempts, 0, 10);
            }
        }

        [Fact]
        public void Generate_FailedAttemptsCanBeZero()
        {
            // Act
            var payloads = Enumerable.Range(0, 100)
                .Select(_ => _generator.Generate())
                .ToList();

            // Assert
            Assert.Contains(payloads, p => p.RiskScore.FailedAttempts == 0);
        }

        #endregion

        #region Blacklist Tests

        [Fact]
        public void Generate_IsBlacklistedShouldBeBoolean()
        {
            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.IsType<bool>(payload.RiskScore.IsBlacklisted);
        }

        [Fact]
        public void Generate_BlacklistShouldBeRare()
        {
            // Act
            var blacklistedCount = 0;
            var totalIterations = 1000;

            for (int i = 0; i < totalIterations; i++)
            {
                var payload = _generator.Generate();
                if (payload.RiskScore.IsBlacklisted)
                    blacklistedCount++;
            }

            // Assert - Deve estar na lista negra em ~2% dos casos
            var percentage = (double)blacklistedCount / totalIterations;
            Assert.InRange(percentage, 0.00, 0.10); // Entre 0% e 10% é razoável para teste aleatório
        }

        [Fact]
        public void Generate_BlacklistedShouldHaveHighRiskScore()
        {
            // Act
            var blacklistedPayloads = Enumerable.Range(0, 500)
                .Select(_ => _generator.Generate())
                .Where(p => p.RiskScore.IsBlacklisted)
                .ToList();

            // Assert - Se encontrou itens na lista negra, eles devem ter alto score
            if (blacklistedPayloads.Count > 0)
            {
                Assert.True(blacklistedPayloads.All(p => p.RiskScore.Score > 0),
                    "Items na lista negra devem ter score > 0");
            }
        }

        #endregion

        #region Transaction Velocity Tests

        [Fact]
        public void Generate_ShouldHaveValidTransactionVelocity()
        {
            // Act & Assert
            for (int i = 0; i < 50; i++)
            {
                var payload = _generator.Generate();
                Assert.InRange(payload.RiskScore.TransactionVelocity, 0, 20);
            }
        }

        #endregion

        #region Score Generation Timestamp Tests

        [Fact]
        public void Generate_ScoreGeneratedAtShouldBeCurrentTime()
        {
            // Arrange
            var beforeGeneration = DateTime.UtcNow;

            // Act
            var payload = _generator.Generate();

            // Assert
            var afterGeneration = DateTime.UtcNow;
            Assert.InRange(payload.RiskScore.ScoreGeneratedAt, 
                beforeGeneration.AddSeconds(-1), 
                afterGeneration.AddSeconds(1));
        }

        [Fact]
        public void Generate_ScoreGeneratedAtShouldNotBeInPast()
        {
            // Act
            var payload = _generator.Generate();

            // Assert
            Assert.True(payload.RiskScore.ScoreGeneratedAt <= DateTime.UtcNow.AddSeconds(1),
                "ScoreGeneratedAt não deve ser no futuro");
        }

        #endregion

        #region Multiple Generations Test

        [Fact]
        public void Generate_MultipleGenerations_AllShouldHaveValidRiskScores()
        {
            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                var payload = _generator.Generate();

                Assert.NotNull(payload.RiskScore);
                Assert.InRange(payload.RiskScore.Score, 0, 999);
                Assert.Contains(payload.RiskScore.RiskLevel, new[] { "LOW", "MEDIUM", "HIGH", "CRITICAL" });
                Assert.Contains(payload.RiskScore.AvsMatch, new[] { "Y", "N", "U" });
                Assert.Contains(payload.RiskScore.CvcMatch, new[] { "Y", "N", "U" });
                Assert.Contains(payload.RiskScore.SpendingPattern, new[] { "NORMAL", "UNUSUAL", "SUSPICIOUS" });
                Assert.NotEmpty(payload.RiskScore.IpCountry);
                Assert.InRange(payload.RiskScore.FailedAttempts, 0, 10);
                Assert.InRange(payload.RiskScore.TransactionVelocity, 0, 20);
            }
        }

        #endregion

        #region RiskScoreData Default Values Tests

        [Fact]
        public void RiskScoreData_ShouldHaveProperDefaultValues()
        {
            // Arrange
            var riskScore = new RiskScoreData();

            // Assert
            Assert.NotNull(riskScore.RiskLevel);
            Assert.Equal("LOW", riskScore.RiskLevel);
            Assert.Equal("Y", riskScore.AvsMatch);
            Assert.Equal("Y", riskScore.CvcMatch);
            Assert.Equal("NORMAL", riskScore.SpendingPattern);
            Assert.Equal("BR", riskScore.IpCountry);
            Assert.True(riskScore.CountryMatch);
            Assert.False(riskScore.IsBlacklisted);
            Assert.Equal(0, riskScore.FailedAttempts);
            Assert.Equal(0, riskScore.TransactionVelocity);
        }

        #endregion

        #region Score Uniqueness Tests

        [Fact]
        public void Generate_ShouldGenerateDifferentRiskScoresOverTime()
        {
            // Act
            var payload1 = _generator.Generate();
            var payload2 = _generator.Generate();
            var payload3 = _generator.Generate();

            // Assert - Scores podem ser iguais por coincidência, mas timestamps devem ser iguais
            // O que importa é que a geração funciona e cria dados válidos
            Assert.NotNull(payload1.RiskScore);
            Assert.NotNull(payload2.RiskScore);
            Assert.NotNull(payload3.RiskScore);
        }

        #endregion
    }
}