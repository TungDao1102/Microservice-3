namespace IdentityService.Exceptions
{
    [Serializable]
    internal class InsufficientFundsException(Guid userId, decimal gilToDebit) : Exception($"Not enough gil to debit {gilToDebit} from user '{userId}'")
    {
        public Guid UserId { get; } = userId;

        public decimal GilToDebit { get; } = gilToDebit;
    }
}
