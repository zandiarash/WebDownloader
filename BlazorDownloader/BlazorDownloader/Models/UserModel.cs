namespace BlazorDownloader.Models
{
    public class UserModel
    {
        public byte wrongAttempts { get; set; }
        public string? ip { get; set; }
        public bool canGenerateCaptcha { get; set; }
        public bool canUse { get; set; }
        public DateTime? cannotUseUntil { get; set; }
    }
}
