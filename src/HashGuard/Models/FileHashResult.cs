using System;
using System.Collections.Generic;

namespace HashGuard.Models
{
    /// <summary>
    /// Represents the hash computation results for a single file.
    /// </summary>
    public class FileHashResult
    {
        /// <summary>
        /// Gets or sets the full path to the file that was hashed.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file name without the directory path.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file size in bytes.
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of hash type to computed hash value.
        /// </summary>
        public Dictionary<HashType, string> Hashes { get; set; } = new();

        /// <summary>
        /// Gets or sets the timestamp when the hash was computed.
        /// </summary>
        public DateTime ComputedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets a value indicating whether the computation succeeded.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Gets or sets an error message if the computation failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets a human-readable representation of the file size.
        /// </summary>
        public string FileSizeFormatted
        {
            get
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = FileSizeBytes;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len /= 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
        }
    }
}
