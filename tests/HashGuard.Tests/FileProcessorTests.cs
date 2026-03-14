using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HashGuard.Models;
using HashGuard.Services;
using Xunit;

namespace HashGuard.Tests
{
    /// <summary>
    /// Unit tests for <see cref="FileProcessor"/>.
    /// </summary>
    public class FileProcessorTests : IDisposable
    {
        private readonly FileProcessor _processor;
        private readonly string _testDir;

        public FileProcessorTests()
        {
            _processor = new FileProcessor(new HashCalculator());
            _testDir = Path.Combine(Path.GetTempPath(), $"HashGuard_Tests_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }

        private string CreateTestFile(string name, string content)
        {
            var path = Path.Combine(_testDir, name);
            File.WriteAllText(path, content);
            return path;
        }

        [Fact]
        public async Task ProcessBatch_SingleFile_ReturnsOneResult()
        {
            var path = CreateTestFile("test.txt", "hello");
            var results = await _processor.ProcessBatch(
                new List<string> { path },
                new List<HashType> { HashType.SHA256 });

            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.Equal("test.txt", results[0].FileName);
            Assert.True(results[0].Hashes.ContainsKey(HashType.SHA256));
        }

        [Fact]
        public async Task ProcessBatch_MultipleFiles_ReturnsAllResults()
        {
            var paths = new List<string>
            {
                CreateTestFile("a.txt", "aaa"),
                CreateTestFile("b.txt", "bbb"),
                CreateTestFile("c.txt", "ccc")
            };

            var results = await _processor.ProcessBatch(
                paths,
                new List<HashType> { HashType.MD5, HashType.SHA256 });

            Assert.Equal(3, results.Count);
            Assert.All(results, r => Assert.True(r.Success));
            Assert.All(results, r => Assert.Equal(2, r.Hashes.Count));
        }

        [Fact]
        public async Task ProcessBatch_ReportsProgress()
        {
            var paths = new List<string>
            {
                CreateTestFile("1.txt", "one"),
                CreateTestFile("2.txt", "two")
            };

            var progressUpdates = new List<BatchProgressInfo>();
            var progress = new Progress<BatchProgressInfo>(info =>
                progressUpdates.Add(new BatchProgressInfo
                {
                    CurrentFileName = info.CurrentFileName,
                    FilesProcessed = info.FilesProcessed,
                    TotalFiles = info.TotalFiles
                }));

            await _processor.ProcessBatch(
                paths,
                new List<HashType> { HashType.MD5 },
                progress);

            // Give progress callbacks time to fire (they're async)
            await Task.Delay(100);

            Assert.True(progressUpdates.Count >= 1);
        }

        [Fact]
        public async Task ProcessBatch_NonexistentFile_ReturnsErrorResult()
        {
            var results = await _processor.ProcessBatch(
                new List<string> { "/nonexistent/file.txt" },
                new List<HashType> { HashType.SHA256 });

            Assert.Single(results);
            Assert.False(results[0].Success);
            Assert.NotNull(results[0].ErrorMessage);
        }

        [Fact]
        public async Task ProcessBatch_MixedValidAndInvalid_ProcessesAll()
        {
            var validPath = CreateTestFile("valid.txt", "valid");
            var paths = new List<string> { validPath, "/nonexistent.txt" };

            var results = await _processor.ProcessBatch(
                paths,
                new List<HashType> { HashType.MD5 });

            Assert.Equal(2, results.Count);
            Assert.True(results[0].Success);
            Assert.False(results[1].Success);
        }

        [Fact]
        public async Task ProcessBatch_Cancellation_ThrowsOperationCanceled()
        {
            // Create many files to increase chance of catching cancellation
            var paths = Enumerable.Range(0, 50)
                .Select(i => CreateTestFile($"cancel_{i}.txt", new string('x', 1000)))
                .ToList();

            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel immediately

            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _processor.ProcessBatch(paths, new List<HashType> { HashType.SHA512 },
                    cancellationToken: cts.Token));
        }

        [Fact]
        public async Task ProcessBatch_EmptyList_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _processor.ProcessBatch(
                    new List<string>(),
                    new List<HashType> { HashType.MD5 }));
        }

        [Fact]
        public async Task ProcessBatch_NoHashTypes_ThrowsArgumentException()
        {
            var path = CreateTestFile("test.txt", "data");
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _processor.ProcessBatch(
                    new List<string> { path },
                    new List<HashType>()));
        }

        [Fact]
        public void ValidateFiles_AllValid_ReturnsAllValid()
        {
            var paths = new List<string>
            {
                CreateTestFile("v1.txt", "a"),
                CreateTestFile("v2.txt", "b")
            };

            var (valid, errors) = FileProcessor.ValidateFiles(paths);
            Assert.Equal(2, valid.Count);
            Assert.Empty(errors);
        }

        [Fact]
        public void ValidateFiles_MixedFiles_SeparatesCorrectly()
        {
            var validPath = CreateTestFile("exists.txt", "data");
            var paths = new List<string> { validPath, "/no/such/file.txt", "" };

            var (valid, errors) = FileProcessor.ValidateFiles(paths);
            Assert.Single(valid);
            Assert.Equal(2, errors.Count);
        }

        [Fact]
        public async Task ProcessSingleFile_SetsFileSizeCorrectly()
        {
            var content = "test content for size check";
            var path = CreateTestFile("size.txt", content);
            var fileInfo = new FileInfo(path);

            var result = await _processor.ProcessSingleFile(
                path, new List<HashType> { HashType.MD5 });

            Assert.Equal(fileInfo.Length, result.FileSizeBytes);
        }
    }
}
