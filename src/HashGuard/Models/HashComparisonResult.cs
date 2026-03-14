namespace HashGuard.Models
{
    /// <summary>
    /// Represents the result of comparing a computed hash against an expected hash.
    /// </summary>
    public class HashComparisonResult
    {
        /// <summary>
        /// Gets or sets the computed hash value.
        /// </summary>
        public string ComputedHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the expected hash value provided by the user.
        /// </summary>
        public string ExpectedHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the hashes match.
        /// </summary>
        public bool IsMatch { get; set; }

        /// <summary>
        /// Gets or sets the hash algorithm that was detected or used for comparison.
        /// </summary>
        public HashType? DetectedHashType { get; set; }
    }
}
