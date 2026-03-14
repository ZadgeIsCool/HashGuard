using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using HashGuard.Models;

namespace HashGuard.Services
{
    /// <summary>
    /// Provides methods for computing cryptographic hashes of files.
    /// Uses chunked reading for efficient processing of large files.
    /// </summary>
    public class HashCalculator
    {
        /// <summary>
        /// Default buffer size (1 MB) for chunked file reading.
        /// </summary>
        private const int BufferSize = 1024 * 1024;

        /// <summary>
        /// Calculates the MD5 hash of a file asynchronously.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The MD5 hash as a lowercase hexadecimal string.</returns>
        public async Task<string> CalculateMD5(string filePath, CancellationToken cancellationToken = default)
        {
            using var algorithm = MD5.Create();
            return await ComputeHashAsync(filePath, algorithm, cancellationToken);
        }

        /// <summary>
        /// Calculates the SHA-1 hash of a file asynchronously.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The SHA-1 hash as a lowercase hexadecimal string.</returns>
        public async Task<string> CalculateSHA1(string filePath, CancellationToken cancellationToken = default)
        {
            using var algorithm = SHA1.Create();
            return await ComputeHashAsync(filePath, algorithm, cancellationToken);
        }

        /// <summary>
        /// Calculates the SHA-256 hash of a file asynchronously.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The SHA-256 hash as a lowercase hexadecimal string.</returns>
        public async Task<string> CalculateSHA256(string filePath, CancellationToken cancellationToken = default)
        {
            using var algorithm = SHA256.Create();
            return await ComputeHashAsync(filePath, algorithm, cancellationToken);
        }

        /// <summary>
        /// Calculates the SHA-512 hash of a file asynchronously.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The SHA-512 hash as a lowercase hexadecimal string.</returns>
        public async Task<string> CalculateSHA512(string filePath, CancellationToken cancellationToken = default)
        {
            using var algorithm = SHA512.Create();
            return await ComputeHashAsync(filePath, algorithm, cancellationToken);
        }

        /// <summary>
        /// Calculates the hash of a file for the specified hash type.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <param name="hashType">The hash algorithm to use.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The hash as a lowercase hexadecimal string.</returns>
        public Task<string> CalculateHash(string filePath, HashType hashType, CancellationToken cancellationToken = default)
        {
            return hashType switch
            {
                HashType.MD5 => CalculateMD5(filePath, cancellationToken),
                HashType.SHA1 => CalculateSHA1(filePath, cancellationToken),
                HashType.SHA256 => CalculateSHA256(filePath, cancellationToken),
                HashType.SHA512 => CalculateSHA512(filePath, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(hashType), $"Unsupported hash type: {hashType}")
            };
        }

        /// <summary>
        /// Compares a computed hash against an expected hash (case-insensitive).
        /// </summary>
        /// <param name="calculated">The computed hash value.</param>
        /// <param name="expected">The expected hash value to compare against.</param>
        /// <returns>A <see cref="HashComparisonResult"/> indicating whether the hashes match.</returns>
        public HashComparisonResult CompareHash(string calculated, string expected)
        {
            string cleanCalculated = calculated.Trim().ToLowerInvariant();
            string cleanExpected = expected.Trim().Replace(" ", "").Replace("-", "").ToLowerInvariant();

            return new HashComparisonResult
            {
                ComputedHash = cleanCalculated,
                ExpectedHash = cleanExpected,
                IsMatch = string.Equals(cleanCalculated, cleanExpected, StringComparison.OrdinalIgnoreCase),
                DetectedHashType = DetectHashType(cleanExpected)
            };
        }

        /// <summary>
        /// Attempts to detect the hash algorithm based on the hash string length.
        /// </summary>
        /// <param name="hash">The hash string to analyze.</param>
        /// <returns>The detected <see cref="HashType"/>, or null if unrecognized.</returns>
        public static HashType? DetectHashType(string hash)
        {
            return hash.Length switch
            {
                32 => HashType.MD5,
                40 => HashType.SHA1,
                64 => HashType.SHA256,
                128 => HashType.SHA512,
                _ => null
            };
        }

        /// <summary>
        /// Core method that reads a file in chunks and computes the hash incrementally.
        /// This approach keeps memory usage constant regardless of file size.
        /// </summary>
        private async Task<string> ComputeHashAsync(string filePath, HashAlgorithm algorithm, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("The specified file was not found.", filePath);

            var buffer = new byte[BufferSize];

            await using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                BufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, BufferSize), cancellationToken)) > 0)
            {
                algorithm.TransformBlock(buffer, 0, bytesRead, null, 0);
            }

            algorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

            return BitConverter.ToString(algorithm.Hash!).Replace("-", "").ToLowerInvariant();
        }
    }
}
