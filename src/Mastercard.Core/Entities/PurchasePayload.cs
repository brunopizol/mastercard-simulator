using System.Text.Json.Serialization;

namespace Mastercard.Core.Entities
{
    public class PurchasePayload
    {
        public string TransactionId { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string CardId { get; set; } = string.Empty;
        public string Pan { get; set; } = string.Empty;
        public string PanHash { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public string Merchant { get; set; } = string.Empty;
        public string MerchantCountry { get; set; } = string.Empty;
        public string EntryMode { get; set; } = string.Empty;
        public string Mcc { get; set; } = string.Empty;
        public string AcquirerCode { get; set; } = string.Empty;
        public string Nsu { get; set; } = string.Empty;
        public string AuthorizationCode { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string CardExpiration { get; set; } = string.Empty;
        public string CardHolder { get; set; } = string.Empty;
        public int Installments { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}