namespace IdentityService.Exceptions
{
    [Serializable]
    internal class UnknownUserException(Guid userId) : Exception($"Unknown user '{userId}'")
    {
        public Guid UserId { get; } = userId;
    }
}
