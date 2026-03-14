using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HashGuard.Models;
using Newtonsoft.Json;

namespace HashGuard.Services
{
    /// <summary>
    /// Manages file integrity baselines - saved hash snapshots that can be
    /// compared against later to detect modifications.
    /// </summary>
    public class BaselineManager
    {
        /// <summary>
        /// Saves a set of hash results as a baseline file.
        /// </summary>
        /// <param name="results">The hash results to save as baseline.</param>
        /// <param name="baselinePath">The path where the baseline file will be saved.</param>
        public async Task SaveBaseline(List<FileHashResult> results, string baselinePath)
        {
            if (results == null || results.Count == 0)
                throw new ArgumentException("Results cannot be null or empty.", nameof(results));

            var baseline = new BaselineData
            {
                CreatedAt = DateTime.UtcNow,
                Files = results
            };

            string json = JsonConvert.SerializeObject(baseline, Formatting.Indented);
            await File.WriteAllTextAsync(baselinePath, json);
        }

        /// <summary>
        /// Loads a previously saved baseline from disk.
        /// </summary>
        /// <param name="baselinePath">The path to the baseline file.</param>
        /// <returns>The loaded baseline data.</returns>
        public async Task<BaselineData> LoadBaseline(string baselinePath)
        {
            if (!File.Exists(baselinePath))
                throw new FileNotFoundException("Baseline file not found.", baselinePath);

            string json = await File.ReadAllTextAsync(baselinePath);
            var baseline = JsonConvert.DeserializeObject<BaselineData>(json);

            return baseline ?? throw new InvalidOperationException("Failed to deserialize baseline file.");
        }

        /// <summary>
        /// Compares current hash results against a saved baseline and returns differences.
        /// </summary>
        /// <param name="currentResults">The current hash results.</param>
        /// <param name="baseline">The saved baseline to compare against.</param>
        /// <returns>A list of comparison entries indicating matches and mismatches.</returns>
        public List<BaselineComparisonEntry> CompareWithBaseline(
            List<FileHashResult> currentResults,
            BaselineData baseline)
        {
            var entries = new List<BaselineComparisonEntry>();
            var baselineMap = new Dictionary<string, FileHashResult>();

            foreach (var file in baseline.Files)
            {
                baselineMap[file.FilePath] = file;
            }

            foreach (var current in currentResults)
            {
                var entry = new BaselineComparisonEntry
                {
                    FilePath = current.FilePath,
                    FileName = current.FileName
                };

                if (baselineMap.TryGetValue(current.FilePath, out var baselineFile))
                {
                    entry.ExistsInBaseline = true;
                    entry.IsMatch = true;

                    foreach (var hash in current.Hashes)
                    {
                        if (baselineFile.Hashes.TryGetValue(hash.Key, out var baselineHash))
                        {
                            if (!string.Equals(hash.Value, baselineHash, StringComparison.OrdinalIgnoreCase))
                            {
                                entry.IsMatch = false;
                                entry.ChangedHashTypes.Add(hash.Key);
                            }
                        }
                    }
                }
                else
                {
                    entry.ExistsInBaseline = false;
                    entry.IsMatch = false;
                }

                entries.Add(entry);
            }

            return entries;
        }
    }

    /// <summary>
    /// Container for baseline data stored on disk.
    /// </summary>
    public class BaselineData
    {
        /// <summary>Gets or sets when the baseline was created.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Gets or sets the list of file hash results in the baseline.</summary>
        public List<FileHashResult> Files { get; set; } = new();
    }

    /// <summary>
    /// Represents the result of comparing one file against its baseline.
    /// </summary>
    public class BaselineComparisonEntry
    {
        /// <summary>Gets or sets the file path.</summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>Gets or sets the file name.</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>Gets or sets whether the file existed in the baseline.</summary>
        public bool ExistsInBaseline { get; set; }

        /// <summary>Gets or sets whether all hashes match the baseline.</summary>
        public bool IsMatch { get; set; }

        /// <summary>Gets or sets the hash types that have changed since the baseline.</summary>
        public List<HashType> ChangedHashTypes { get; set; } = new();
    }
}
