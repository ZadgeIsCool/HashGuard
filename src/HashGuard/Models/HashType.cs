namespace HashGuard.Models
{
    /// <summary>
    /// Supported hash algorithm types.
    /// </summary>
    public enum HashType
    {
        /// <summary>MD5 (128-bit) - Fast but not collision-resistant.</summary>
        MD5,

        /// <summary>SHA-1 (160-bit) - Deprecated for security use.</summary>
        SHA1,

        /// <summary>SHA-256 (256-bit) - Recommended for most use cases.</summary>
        SHA256,

        /// <summary>SHA-512 (512-bit) - Strongest option available.</summary>
        SHA512
    }
}
