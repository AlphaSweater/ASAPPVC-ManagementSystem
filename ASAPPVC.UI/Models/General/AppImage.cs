namespace ASAPPVC.UI.Models.General
{
    /// <summary>Binary image stored in DB as part of an aggregate (owned by a parent row).</summary>
    public sealed class AppImage
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "image/jpeg";
        public int Width { get; set; }
        public int Height { get; set; }
        public long Length { get; set; }
        public string Sha256 { get; set; } = string.Empty;    // dedupe / caching
        public DateTime UploadedUtc { get; set; } = DateTime.UtcNow;

        // Optional small preview stored too (helps list pages):
        public byte[]? Thumb { get; set; }
    }
}