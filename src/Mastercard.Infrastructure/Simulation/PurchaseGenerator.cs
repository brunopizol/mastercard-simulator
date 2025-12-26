using System.Security.Cryptography;
using System.Text;
using Mastercard.Application.Interfaces;
using Mastercard.Core.Entities;

namespace Mastercard.Infrastructure.Simulation
{
    public class PurchaseGenerator : IPurchaseGenerator
    {
        private static long _correlationCounter = DateTime.UtcNow.Ticks;
        private static long _nsuCounter = DateTime.UtcNow.Ticks;
        private static readonly Random _rnd = new Random();
        private static readonly string[] _currencies = new[] { "BRL", "USD", "EUR", "GBP", "ARS" };
        private static readonly string[] _merchants = new[] { "Loja Exemplo", "Mercado Central", "E-commerce XYZ", "Restaurante Bom Sabor" };
        private static readonly string[] _riskLevels = new[] { "LOW", "MEDIUM", "HIGH", "CRITICAL" };
        private static readonly string[] _spendingPatterns = new[] { "NORMAL", "UNUSUAL", "SUSPICIOUS" };

        public PurchasePayload Generate()
        {
            var pan = GeneratePan();
            var panHash = Sha256Hex(pan);
            var amount = Math.Round((decimal)(_rnd.NextDouble() * 9999.0 + 1.0), 2);
            var currency = _currencies[_rnd.Next(_currencies.Length)];

            var correlationId = Interlocked.Increment(ref _correlationCounter).ToString();
            var nsu = Interlocked.Increment(ref _nsuCounter).ToString();

            return new PurchasePayload
            {
                TransactionId = Guid.NewGuid().ToString(),
                CorrelationId = correlationId,
                Amount = amount,
                Currency = currency,
                CardId = GeneratePan(),
                Pan = pan,
                PanHash = panHash,
                Account = GenerateAccountNumber(),
                Merchant = _merchants[_rnd.Next(_merchants.Length)],
                MerchantCountry = "BR",
                EntryMode = "CHIP",
                Mcc = _rnd.Next(3000, 9999).ToString(),
                AcquirerCode = _rnd.Next(100000, 999999).ToString(),
                Nsu = nsu,
                AuthorizationCode = _rnd.Next(100000, 999999).ToString(),
                ReferenceNumber = _rnd.Next(100000000, 999999999).ToString(),
                Timestamp = DateTime.UtcNow,
                CardExpiration = GenerateExpiry(),
                CardHolder = GenerateCardHolderName(),
                Installments = _rnd.Next(1, 12),
                RiskScore = GenerateRiskScore(amount),
                AdditionalData = new Dictionary<string, object>
                {
                    { "processor", "MastercardSimulator" },
                    { "testMode", "true" }
                }
            };
        }

        /// <summary>
        /// Gera dados de risco baseados em ISO 8583
        /// </summary>
        private static RiskScoreData GenerateRiskScore(decimal amount)
        {
            // Calcula score baseado em múltiplos fatores
            int baseScore = _rnd.Next(0, 100);
            
            // Aumenta risco para valores muito altos
            if (amount > 5000) baseScore += 150;
            else if (amount > 2000) baseScore += 75;
            else if (amount > 1000) baseScore += 25;

            // Fator aleatório de variação (simulando fraude ocasional)
            if (_rnd.NextDouble() < 0.15) baseScore += _rnd.Next(200, 400);

            var finalScore = Math.Min(baseScore, 999); // Limita a 999

            // Determina nível de risco
            string riskLevel = finalScore switch
            {
                >= 750 => "CRITICAL",
                >= 500 => "HIGH",
                >= 250 => "MEDIUM",
                _ => "LOW"
            };

            // Determina padrão de gasto
            string spendingPattern = finalScore switch
            {
                >= 500 => "SUSPICIOUS",
                >= 250 => "UNUSUAL",
                _ => "NORMAL"
            };

            return new RiskScoreData
            {
                Score = finalScore,
                RiskLevel = riskLevel,
                TransactionVelocity = _rnd.Next(0, 10), // Número de transações no período
                AvsMatch = new[] { "Y", "N", "U" }[_rnd.Next(3)],
                CvcMatch = new[] { "Y", "N", "U" }[_rnd.Next(3)],
                SpendingPattern = spendingPattern,
                IpCountry = _rnd.NextDouble() < 0.9 ? "BR" : new[] { "US", "CN", "RU", "NG" }[_rnd.Next(4)],
                CountryMatch = _rnd.NextDouble() < 0.95, // 95% de correspondência
                FailedAttempts = _rnd.Next(0, 5),
                IsBlacklisted = _rnd.NextDouble() < 0.02, // 2% de chance de estar na lista negra
                ScoreGeneratedAt = DateTime.UtcNow
            };
        }

        private static string GenerateAccountNumber()
        {
            return _rnd.Next(1000000000, int.MaxValue).ToString().Substring(0, 10);
        }

        private static string GenerateCardHolderName()
        {
            var first = new[] { "João", "Maria", "Carlos", "Ana", "Pedro", "Laura" }[_rnd.Next(6)];
            var last = new[] { "Silva", "Souza", "Pereira", "Oliveira", "Costa", "Almeida" }[_rnd.Next(6)];
            return $"{first} {last}";
        }

        private static string GenerateExpiry()
        {
            var dt = DateTime.UtcNow.AddMonths(_rnd.Next(6, 60));
            return dt.ToString("MM/yy");
        }

        private static string GeneratePan()
        {
            var digits = new int[16];
            digits[0] = 5;
            digits[1] = _rnd.Next(1, 6);
            for (int i = 2; i < 15; i++)
                digits[i] = _rnd.Next(0, 10);
            digits[15] = CalculateLuhnCheckDigit(digits);
            var sb = new StringBuilder();
            foreach (var d in digits) sb.Append(d);
            return sb.ToString();
        }

        private static int CalculateLuhnCheckDigit(int[] digitsWithoutCheck)
        {
            int sum = 0;
            for (int i = 0; i < 15; i++)
            {
                int digit = digitsWithoutCheck[i];
                int positionFromRight = 15 - i;
                if (positionFromRight % 2 == 1)
                {
                    int doubled = digit * 2;
                    if (doubled > 9) doubled -= 9;
                    sum += doubled;
                }
                else
                {
                    sum += digit;
                }
            }
            int check = (10 - (sum % 10)) % 10;
            return check;
        }

        private static string Sha256Hex(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}