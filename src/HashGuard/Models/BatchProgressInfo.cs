namespace HashGuard.Models
{
    /// <summary>
    /// Provides progress information during batch hash operations.
    /// </summary>
    public class BatchProgressInfo
    {
        /// <summary>
        /// Gets or sets the name of the file currently being processed.
        /// </summary>
        public string CurrentFileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of files processed so far.
        /// </summary>
        public int FilesProcessed { get; set; }

        /// <summary>
        /// Gets or sets the total number of files in the batch.
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// Gets the completion percentage (0-100).
        /// </summary>
        public double PercentComplete =>
            TotalFiles > 0 ? (double)FilesProcessed / TotalFiles * 100 : 0;
    }
}
