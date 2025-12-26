namespace Mastercard.Core.Exceptions
{
    public class InvalidForwardRequestException : DomainException
    {
        public InvalidForwardRequestException(string message) : base(message)
        {
        }
    }
}