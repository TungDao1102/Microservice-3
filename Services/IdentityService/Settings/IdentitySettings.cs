namespace IdentityService.Settings
{
    public class IdentitySettings
    {
        public string AdminUserEmail { get; init; } = string.Empty;
        public string AdminUserPassword { get; init; } = string.Empty;
        public decimal StartingGil { get; init; }
    }
}
