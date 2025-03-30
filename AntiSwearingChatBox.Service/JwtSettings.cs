namespace AntiSwearingChatBox.Service
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = "your-super-secret-key-with-minimum-32-characters";
        public string Issuer { get; set; } = "AntiSwearingChatBox";
        public string Audience { get; set; } = "AntiSwearingChatBox";
        public int ExpirationInMinutes { get; set; } = 60;
        public int RefreshTokenExpirationInDays { get; set; } = 7;
    }
}