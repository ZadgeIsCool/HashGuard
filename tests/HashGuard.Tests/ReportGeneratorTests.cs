using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HashGuard.Models;
using HashGuard.Services;
using Xunit;

namespace HashGuard.Tests
{
    /// <summary>
    /// Unit tests for <see cref="ReportGenerator"/>.
    /// </summary>
    public class ReportGeneratorTests : IDisposable
    {
        private readonly ReportGenerator _generator;
        private readonly string _testDir;

        public ReportGeneratorTests()
        {
            _generator = new ReportGenerator();
            _testDir = Path.Combine(Path.GetTempPath(), $"HashGuard_Tests_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }

        private List<FileHashResult> CreateSampleResults()
        {
            return new List<FileHashResult>
            {
                new()
                {
                    FileName = "test.txt",
                    FilePath = "/path/to/test.txt",
                    FileSizeBytes = 1024,
                    Success = true,
                    Hashes = new Dictionary<HashType, string>
                    {
                        { HashType.MD5, "d41d8cd98f00b204e9800998ecf8427e" },
                        { HashType.SHA256, "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855" }
                    }
                },
                new()
                {
                    FileName = "failed.txt",
                    FilePath = "/path/to/failed.txt",
                    FileSizeBytes = 0,
                    Success = false,
                    ErrorMessage = "Access denied"
                }
            };
        }

        [Fact]
        public async Task ExportToTXT_CreatesValidFile()
        {
            var path = Path.Combine(_testDir, "report.txt");
            await _generator.ExportToTXT(CreateSampleResults(), path);

            Assert.True(File.Exists(path));
            var content = await File.ReadAllTextAsync(path);
            Assert.Contains("HashGuard", content);
            Assert.Contains("test.txt", content);
            Assert.Contains("d41d8cd98f00b204e9800998ecf8427e", content);
            Assert.Contains("Access denied", content);
        }

        [Fact]
        public async Task ExportToJSON_CreatesValidJson()
        {
            var path = Path.Combine(_testDir, "report.json");
            await _generator.ExportToJSON(CreateSampleResults(), path);

            Assert.True(File.Exists(path));
            var content = await File.ReadAllTextAsync(path);
            Assert.Contains("\"Generator\": \"HashGuard\"", content);
            Assert.Contains("\"TotalFiles\": 2", content);
            Assert.Contains("\"SuccessCount\": 1", content);
        }

        [Fact]
        public async Task ExportToCSV_CreatesValidCsv()
        {
            var path = Path.Combine(_testDir, "report.csv");
            await _generator.ExportToCSV(CreateSampleResults(), path);

            Assert.True(File.Exists(path));
            var lines = await File.ReadAllLinesAsync(path);
            Assert.True(lines.Length >= 3); // Header + 2 data rows
            Assert.Contains("FileName", lines[0]);
            Assert.Contains("test.txt", lines[1]);
        }

        [Fact]
        public async Task ExportToTXT_EmptyResults_ThrowsArgumentException()
        {
            var path = Path.Combine(_testDir, "empty.txt");
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _generator.ExportToTXT(new List<FileHashResult>(), path));
        }

        [Fact]
        public async Task ExportToJSON_NullOutputPath_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _generator.ExportToJSON(CreateSampleResults(), ""));
        }
    }
}
