namespace RevisiaAPI.Models
{
    public class RefreshToken
    {
        public string Token { get; set; } = null!;
        public string HashedValue { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public int UserId { get; set; }
        public string? LastTokenHash { get; set; }
        public bool IsInvalidated { get; set; } = false;
        public DateTime? InvalidatedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}
