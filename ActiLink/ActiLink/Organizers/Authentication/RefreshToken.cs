namespace ActiLink.Organizers.Authentication
{
    /// <summary>
    /// Represents a refresh token.
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; } = null!;

        public Organizer TokenOwner { get; set; } = null!;

        public DateTime ExpiryTimeUtc { get; set; }
    }
}
