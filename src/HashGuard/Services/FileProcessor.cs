using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HashGuard.Models;

namespace HashGuard.Services
{
    /// <summary>
    /// Handles batch processing of multiple files for hash computation.
    /// Supports parallel processing with progress reporting.
    /// </summary>
    public class FileProcessor
    {
        private readonly HashCalculator _hashCalculator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileProcessor"/> class.
        /// </summary>
        /// <param name="hashCalculator">The hash calculator to use for computation.</param>
        public FileProcessor(HashCalculator hashCalculator)
        {
            _hashCalculator = hashCalculator ?? throw new ArgumentNullException(nameof(hashCalculator));
        }

        /// <summary>
        /// Processes a batch of files, computing the specified hash types for each.
        /// Reports progress through the provided callback.
        /// </summary>
        /// <param name="filePaths">List of file paths to process.</param>
        /// <param name="hashTypes">Hash algorithms to compute for each file.</param>
        /// <param name="progress">Optional progress reporter for UI updates.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A list of hash results, one per file.</returns>
        public async Task<List<FileHashResult>> ProcessBatch(
            List<string> filePaths,
            List<HashType> hashTypes,
            IProgress<BatchProgressInfo>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (filePaths == null || filePaths.Count == 0)
                throw new ArgumentException("File list cannot be null or empty.", nameof(filePaths));

            if (hashTypes == null || hashTypes.Count == 0)
                throw new ArgumentException("At least one hash type must be selected.", nameof(hashTypes));

            var results = new List<FileHashResult>();
            int processed = 0;

            foreach (var filePath in filePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await ProcessSingleFile(filePath, hashTypes, cancellationToken);
                results.Add(result);

                processed++;
                progress?.Report(new BatchProgressInfo
                {
                    CurrentFileName = Path.GetFileName(filePath),
                    FilesProcessed = processed,
                    TotalFiles = filePaths.Count
                });
            }

            return results;
        }

        /// <summary>
        /// Processes a single file, computing all requested hash types.
        /// Handles errors gracefully so one failed file doesn't stop the batch.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="hashTypes">The hash types to compute.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The hash result for this file.</returns>
        public async Task<FileHashResult> ProcessSingleFile(
            string filePath,
            List<HashType> hashTypes,
            CancellationToken cancellationToken = default)
        {
            var result = new FileHashResult
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath)
            };

            try
            {
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                {
                    result.Success = false;
                    result.ErrorMessage = "File not found.";
                    return result;
                }

                result.FileSizeBytes = fileInfo.Length;

                // Compute each requested hash type sequentially per file
                // (parallel per-algorithm on the same file would thrash disk I/O)
                foreach (var hashType in hashTypes)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string hash = await _hashCalculator.CalculateHash(filePath, hashType, cancellationToken);
                    result.Hashes[hashType] = hash;
                }
            }
            catch (OperationCanceledException)
            {
                throw; // Propagate cancellation
            }
            catch (UnauthorizedAccessException)
            {
                result.Success = false;
                result.ErrorMessage = "Access denied. The file may be locked or require elevated permissions.";
            }
            catch (IOException ex)
            {
                result.Success = false;
                result.ErrorMessage = $"I/O error: {ex.Message}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Unexpected error: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Validates a list of file paths and returns only those that exist and are accessible.
        /// </summary>
        /// <param name="filePaths">The file paths to validate.</param>
        /// <returns>A tuple containing valid paths and a list of error messages for invalid paths.</returns>
        public static (List<string> ValidPaths, List<string> Errors) ValidateFiles(IEnumerable<string> filePaths)
        {
            var valid = new List<string>();
            var errors = new List<string>();

            foreach (var path in filePaths)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    errors.Add("Empty file path encountered.");
                    continue;
                }

                if (!File.Exists(path))
                {
                    errors.Add($"File not found: {path}");
                    continue;
                }

                try
                {
                    // Verify we can at least open the file for reading
                    using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    valid.Add(path);
                }
                catch (Exception ex)
                {
                    errors.Add($"Cannot access '{Path.GetFileName(path)}': {ex.Message}");
                }
            }

            return (valid, errors);
        }
    }
}
