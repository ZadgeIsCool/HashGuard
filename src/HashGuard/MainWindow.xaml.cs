using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using HashGuard.Models;
using HashGuard.Services;
using Microsoft.Win32;

namespace HashGuard
{
    /// <summary>
    /// Main application window for HashGuard.
    /// Handles UI interaction, file selection, hash computation, and report export.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HashCalculator _hashCalculator;
        private readonly FileProcessor _fileProcessor;
        private readonly ReportGenerator _reportGenerator;
        private readonly BaselineManager _baselineManager;

        private List<string> _selectedFiles = new();
        private List<FileHashResult> _lastResults = new();
        private CancellationTokenSource? _cancellationTokenSource;

        /// <summary>
        /// Routed command for opening files (Ctrl+O).
        /// </summary>
        public static RoutedCommand OpenFileCommand { get; } = new();

        /// <summary>
        /// Routed command for exporting reports (Ctrl+R).
        /// </summary>
        public static RoutedCommand ExportReportCommand { get; } = new();

        /// <summary>
        /// Routed command for clearing results (Ctrl+Shift+C).
        /// </summary>
        public static RoutedCommand ClearCommand { get; } = new();

        public MainWindow()
        {
            InitializeComponent();

            _hashCalculator = new HashCalculator();
            _fileProcessor = new FileProcessor(_hashCalculator);
            _reportGenerator = new ReportGenerator();
            _baselineManager = new BaselineManager();

            // Set up command bindings for keyboard shortcuts
            CommandBindings.Add(new CommandBinding(OpenFileCommand, (s, e) => BtnSelectFiles_Click(s, e)));
            CommandBindings.Add(new CommandBinding(ExportReportCommand, (s, e) => BtnExportReport_Click(s, e)));
            CommandBindings.Add(new CommandBinding(ClearCommand, (s, e) => BtnClear_Click(s, e)));

            DataContext = this;
        }

        /// <summary>
        /// Handles file selection via the Open File dialog.
        /// </summary>
        private void BtnSelectFiles_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Files to Verify",
                Multiselect = true,
                Filter = "All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                AddFiles(dialog.FileNames);
            }
        }

        /// <summary>
        /// Handles drag-over events to show the drop cursor.
        /// </summary>
        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                DropZone.BorderBrush = Brushes.LimeGreen;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Handles file drop events.
        /// </summary>
        private void Window_Drop(object sender, DragEventArgs e)
        {
            DropZone.BorderBrush = (Brush)FindResource("AccentBrush");

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null)
                {
                    AddFiles(files);
                }
            }
        }

        /// <summary>
        /// Adds files to the selection list and updates the UI.
        /// </summary>
        private void AddFiles(IEnumerable<string> files)
        {
            var newFiles = files.Where(f => !_selectedFiles.Contains(f)).ToList();
            _selectedFiles.AddRange(newFiles);

            UpdateFileList();
            BtnCalculate.IsEnabled = _selectedFiles.Count > 0;
            TxtStatus.Text = $"{_selectedFiles.Count} file(s) selected";
        }

        /// <summary>
        /// Updates the file list display.
        /// </summary>
        private void UpdateFileList()
        {
            FileListBox.ItemsSource = _selectedFiles.Select(System.IO.Path.GetFileName).ToList();
            FileListBox.Visibility = _selectedFiles.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Begins hash calculation for all selected files.
        /// </summary>
        private async void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFiles.Count == 0)
            {
                MessageBox.Show("Please select at least one file.", "No Files Selected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var hashTypes = GetSelectedHashTypes();
            if (hashTypes.Count == 0)
            {
                MessageBox.Show("Please select at least one hash algorithm.", "No Algorithm Selected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SetProcessingState(true);
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                var progress = new Progress<BatchProgressInfo>(info =>
                {
                    ProgressBar.Value = info.PercentComplete;
                    TxtProgress.Text = $"Processing: {info.CurrentFileName} ({info.FilesProcessed}/{info.TotalFiles})";
                });

                _lastResults = await _fileProcessor.ProcessBatch(
                    _selectedFiles, hashTypes, progress, _cancellationTokenSource.Token);

                DisplayResults(_lastResults);
                TxtStatus.Text = $"Completed: {_lastResults.Count(r => r.Success)} succeeded, " +
                                 $"{_lastResults.Count(r => !r.Success)} failed";
            }
            catch (OperationCanceledException)
            {
                TxtStatus.Text = "Operation cancelled.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TxtStatus.Text = "Error during processing.";
            }
            finally
            {
                SetProcessingState(false);
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Cancels the current hash calculation.
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            TxtStatus.Text = "Cancelling...";
        }

        /// <summary>
        /// Displays hash results in the UI text boxes.
        /// Shows the first file's results; batch results go to the report.
        /// </summary>
        private void DisplayResults(List<FileHashResult> results)
        {
            // Display first successful result in the hash text boxes
            var first = results.FirstOrDefault(r => r.Success);
            if (first == null) return;

            TxtMD5.Text = first.Hashes.GetValueOrDefault(HashType.MD5, "—");
            TxtSHA1.Text = first.Hashes.GetValueOrDefault(HashType.SHA1, "—");
            TxtSHA256.Text = first.Hashes.GetValueOrDefault(HashType.SHA256, "—");
            TxtSHA512.Text = first.Hashes.GetValueOrDefault(HashType.SHA512, "—");

            BtnExportReport.IsEnabled = true;
            BtnSaveBaseline.IsEnabled = true;
            BtnCompareBaseline.IsEnabled = true;
        }

        /// <summary>
        /// Compares the user-provided hash against all computed hashes.
        /// </summary>
        private void BtnCompare_Click(object sender, RoutedEventArgs e)
        {
            string expected = TxtExpectedHash.Text.Trim();
            if (string.IsNullOrEmpty(expected))
            {
                MessageBox.Show("Please enter a hash to compare.", "Empty Hash",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_lastResults.Count == 0 || !_lastResults.Any(r => r.Success))
            {
                MessageBox.Show("No hash results to compare against. Calculate hashes first.",
                    "No Results", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var first = _lastResults.First(r => r.Success);
            bool matched = false;
            string matchedType = "";

            foreach (var hash in first.Hashes)
            {
                var result = _hashCalculator.CompareHash(hash.Value, expected);
                if (result.IsMatch)
                {
                    matched = true;
                    matchedType = hash.Key.ToString();
                    break;
                }
            }

            ComparisonResultBorder.Visibility = Visibility.Visible;

            if (matched)
            {
                ComparisonResultBorder.Background = new SolidColorBrush(Color.FromRgb(34, 139, 34));
                TxtComparisonResult.Text = $"MATCH - Hash verified successfully ({matchedType})";
                TxtComparisonResult.Foreground = Brushes.White;
            }
            else
            {
                ComparisonResultBorder.Background = new SolidColorBrush(Color.FromRgb(220, 20, 60));
                TxtComparisonResult.Text = "MISMATCH - Hash does NOT match any computed value!";
                TxtComparisonResult.Foreground = Brushes.White;
            }
        }

        /// <summary>
        /// Copies a specific hash value to the clipboard.
        /// </summary>
        private void BtnCopyHash_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is string tag)
            {
                string? hash = tag switch
                {
                    "MD5" => TxtMD5.Text,
                    "SHA1" => TxtSHA1.Text,
                    "SHA256" => TxtSHA256.Text,
                    "SHA512" => TxtSHA512.Text,
                    _ => null
                };

                if (!string.IsNullOrEmpty(hash) && hash != "—")
                {
                    Clipboard.SetText(hash);
                    TxtStatus.Text = $"{tag} hash copied to clipboard.";
                }
            }
        }

        /// <summary>
        /// Exports the results to a file in the user's chosen format.
        /// </summary>
        private async void BtnExportReport_Click(object sender, RoutedEventArgs e)
        {
            if (_lastResults.Count == 0)
            {
                MessageBox.Show("No results to export.", "No Results",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Export Verification Report",
                Filter = "Text File (*.txt)|*.txt|JSON File (*.json)|*.json|CSV File (*.csv)|*.csv",
                DefaultExt = ".txt",
                FileName = $"HashGuard_Report_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    switch (dialog.FilterIndex)
                    {
                        case 1:
                            await _reportGenerator.ExportToTXT(_lastResults, dialog.FileName);
                            break;
                        case 2:
                            await _reportGenerator.ExportToJSON(_lastResults, dialog.FileName);
                            break;
                        case 3:
                            await _reportGenerator.ExportToCSV(_lastResults, dialog.FileName);
                            break;
                    }

                    TxtStatus.Text = $"Report exported to {dialog.FileName}";
                    MessageBox.Show("Report exported successfully!", "Export Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export report: {ex.Message}", "Export Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Saves the current results as a baseline file.
        /// </summary>
        private async void BtnSaveBaseline_Click(object sender, RoutedEventArgs e)
        {
            if (_lastResults.Count == 0)
            {
                MessageBox.Show("No results to save as baseline.", "No Results",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Save Hash Baseline",
                Filter = "HashGuard Baseline (*.hgb)|*.hgb|JSON File (*.json)|*.json",
                DefaultExt = ".hgb",
                FileName = $"baseline_{DateTime.Now:yyyyMMdd}"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await _baselineManager.SaveBaseline(_lastResults, dialog.FileName);
                    TxtStatus.Text = $"Baseline saved to {dialog.FileName}";
                    MessageBox.Show("Baseline saved successfully!", "Baseline Saved",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save baseline: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Compares current results against a saved baseline.
        /// </summary>
        private async void BtnCompareBaseline_Click(object sender, RoutedEventArgs e)
        {
            if (_lastResults.Count == 0)
            {
                MessageBox.Show("Calculate hashes first, then compare against a baseline.",
                    "No Results", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = "Select Baseline File",
                Filter = "HashGuard Baseline (*.hgb)|*.hgb|JSON File (*.json)|*.json|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var baseline = await _baselineManager.LoadBaseline(dialog.FileName);
                    var comparisons = _baselineManager.CompareWithBaseline(_lastResults, baseline);

                    int matched = comparisons.Count(c => c.IsMatch);
                    int changed = comparisons.Count(c => !c.IsMatch && c.ExistsInBaseline);
                    int newFiles = comparisons.Count(c => !c.ExistsInBaseline);

                    string summary = $"Baseline Comparison Results:\n\n" +
                                     $"  Matched: {matched} file(s)\n" +
                                     $"  Changed: {changed} file(s)\n" +
                                     $"  New (not in baseline): {newFiles} file(s)\n\n";

                    if (changed > 0)
                    {
                        summary += "Changed files:\n";
                        foreach (var c in comparisons.Where(c => !c.IsMatch && c.ExistsInBaseline))
                        {
                            summary += $"  - {c.FileName} ({string.Join(", ", c.ChangedHashTypes)})\n";
                        }
                    }

                    var icon = changed > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information;
                    MessageBox.Show(summary, "Baseline Comparison", MessageBoxButton.OK, icon);

                    TxtStatus.Text = $"Baseline comparison: {matched} matched, {changed} changed, {newFiles} new";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to compare baseline: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Clears all results and resets the UI.
        /// </summary>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            _selectedFiles.Clear();
            _lastResults.Clear();

            FileListBox.ItemsSource = null;
            FileListBox.Visibility = Visibility.Collapsed;

            TxtMD5.Text = string.Empty;
            TxtSHA1.Text = string.Empty;
            TxtSHA256.Text = string.Empty;
            TxtSHA512.Text = string.Empty;
            TxtExpectedHash.Text = string.Empty;

            ComparisonResultBorder.Visibility = Visibility.Collapsed;
            BtnCalculate.IsEnabled = false;
            BtnExportReport.IsEnabled = false;
            BtnSaveBaseline.IsEnabled = false;
            BtnCompareBaseline.IsEnabled = false;

            TxtStatus.Text = "Ready";
        }

        /// <summary>
        /// Returns the list of hash types selected by the user via checkboxes.
        /// </summary>
        private List<HashType> GetSelectedHashTypes()
        {
            var types = new List<HashType>();
            if (ChkMD5.IsChecked == true) types.Add(HashType.MD5);
            if (ChkSHA1.IsChecked == true) types.Add(HashType.SHA1);
            if (ChkSHA256.IsChecked == true) types.Add(HashType.SHA256);
            if (ChkSHA512.IsChecked == true) types.Add(HashType.SHA512);
            return types;
        }

        /// <summary>
        /// Toggles UI elements between processing and idle states.
        /// </summary>
        private void SetProcessingState(bool isProcessing)
        {
            BtnCalculate.IsEnabled = !isProcessing;
            BtnSelectFiles.IsEnabled = !isProcessing;
            BtnCancel.IsEnabled = isProcessing;
            BtnCancel.Visibility = isProcessing ? Visibility.Visible : Visibility.Collapsed;
            ProgressPanel.Visibility = isProcessing ? Visibility.Visible : Visibility.Collapsed;

            if (isProcessing)
            {
                ProgressBar.Value = 0;
            }
        }

        /// <summary>
        /// Toggles between dark and light themes.
        /// </summary>
        private void MenuToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            app.ToggleTheme();
        }

        /// <summary>
        /// Opens the About dialog.
        /// </summary>
        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow { Owner = this };
            aboutWindow.ShowDialog();
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
