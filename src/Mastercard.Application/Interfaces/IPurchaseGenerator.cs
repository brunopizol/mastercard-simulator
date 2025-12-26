using Mastercard.Core.Entities;

namespace Mastercard.Application.Interfaces
{
    public interface IPurchaseGenerator
    {
        PurchasePayload Generate();
    }
}