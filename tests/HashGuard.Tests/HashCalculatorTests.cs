using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HashGuard.Models;
using HashGuard.Services;
using Xunit;

namespace HashGuard.Tests
{
    /// <summary>
    /// Unit tests for <see cref="HashCalculator"/>.
    /// Verifies hash accuracy against known values and tests edge cases.
    /// </summary>
    public class HashCalculatorTests : IDisposable
    {
        private readonly HashCalculator _calculator;
        private readonly string _testDir;

        public HashCalculatorTests()
        {
            _calculator = new HashCalculator();
            _testDir = Path.Combine(Path.GetTempPath(), $"HashGuard_Tests_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        private string CreateTestFile(string content)
        {
            var path = Path.Combine(_testDir, $"test_{Guid.NewGuid():N}.txt");
            File.WriteAllText(path, content, new UTF8Encoding(false));
            return path;
        }

        private string CreateTestFileBytes(byte[] data)
        {
            var path = Path.Combine(_testDir, $"test_{Guid.NewGuid():N}.bin");
            File.WriteAllBytes(path, data);
            return path;
        }

        // Known hash values for empty string (empty file)
        // These are well-established reference values.

        [Fact]
        public async Task CalculateMD5_EmptyFile_ReturnsKnownHash()
        {
            var path = CreateTestFileBytes(Array.Empty<byte>());
            var hash = await _calculator.CalculateMD5(path);
            Assert.Equal("d41d8cd98f00b204e9800998ecf8427e", hash);
        }

        [Fact]
        public async Task CalculateSHA1_EmptyFile_ReturnsKnownHash()
        {
            var path = CreateTestFileBytes(Array.Empty<byte>());
            var hash = await _calculator.CalculateSHA1(path);
            Assert.Equal("da39a3ee5e6b4b0d3255bfef95601890afd80709", hash);
        }

        [Fact]
        public async Task CalculateSHA256_EmptyFile_ReturnsKnownHash()
        {
            var path = CreateTestFileBytes(Array.Empty<byte>());
            var hash = await _calculator.CalculateSHA256(path);
            Assert.Equal("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", hash);
        }

        [Fact]
        public async Task CalculateSHA512_EmptyFile_ReturnsKnownHash()
        {
            var path = CreateTestFileBytes(Array.Empty<byte>());
            var hash = await _calculator.CalculateSHA512(path);
            Assert.Equal(
                "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce" +
                "47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e",
                hash);
        }

        // Known hash for "hello" (UTF-8, no BOM)

        [Fact]
        public async Task CalculateMD5_HelloString_ReturnsKnownHash()
        {
            var path = CreateTestFile("hello");
            var hash = await _calculator.CalculateMD5(path);
            Assert.Equal("5d41402abc4b2a76b9719d911017c592", hash);
        }

        [Fact]
        public async Task CalculateSHA256_HelloString_ReturnsKnownHash()
        {
            var path = CreateTestFile("hello");
            var hash = await _calculator.CalculateSHA256(path);
            Assert.Equal("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824", hash);
        }

        // Consistency tests - same file should always produce same hash

        [Fact]
        public async Task CalculateHash_SameFileTwice_ReturnsSameResult()
        {
            var path = CreateTestFile("consistency test data");
            var hash1 = await _calculator.CalculateSHA256(path);
            var hash2 = await _calculator.CalculateSHA256(path);
            Assert.Equal(hash1, hash2);
        }

        // Different files produce different hashes

        [Fact]
        public async Task CalculateHash_DifferentFiles_ReturnsDifferentHashes()
        {
            var path1 = CreateTestFile("file one content");
            var path2 = CreateTestFile("file two content");
            var hash1 = await _calculator.CalculateSHA256(path1);
            var hash2 = await _calculator.CalculateSHA256(path2);
            Assert.NotEqual(hash1, hash2);
        }

        // Hash output format tests

        [Fact]
        public async Task CalculateMD5_ReturnsLowercaseHex32Chars()
        {
            var path = CreateTestFile("format test");
            var hash = await _calculator.CalculateMD5(path);
            Assert.Equal(32, hash.Length);
            Assert.Matches("^[0-9a-f]+$", hash);
        }

        [Fact]
        public async Task CalculateSHA256_ReturnsLowercaseHex64Chars()
        {
            var path = CreateTestFile("format test");
            var hash = await _calculator.CalculateSHA256(path);
            Assert.Equal(64, hash.Length);
            Assert.Matches("^[0-9a-f]+$", hash);
        }

        // Large file test (> 1 MB to test chunked reading)

        [Fact]
        public async Task CalculateSHA256_LargeFile_CompletesSuccessfully()
        {
            // Create a 2 MB test file
            var data = new byte[2 * 1024 * 1024];
            new Random(42).NextBytes(data);
            var path = CreateTestFileBytes(data);

            var hash = await _calculator.CalculateSHA256(path);
            Assert.Equal(64, hash.Length);
            Assert.Matches("^[0-9a-f]+$", hash);
        }

        // Error handling tests

        [Fact]
        public async Task CalculateHash_NonexistentFile_ThrowsFileNotFoundException()
        {
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                _calculator.CalculateSHA256("/nonexistent/path/file.txt"));
        }

        [Fact]
        public async Task CalculateHash_NullPath_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _calculator.CalculateSHA256(null!));
        }

        [Fact]
        public async Task CalculateHash_EmptyPath_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _calculator.CalculateSHA256(""));
        }

        // CompareHash tests

        [Fact]
        public void CompareHash_IdenticalHashes_ReturnsMatch()
        {
            var result = _calculator.CompareHash(
                "2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824",
                "2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824");
            Assert.True(result.IsMatch);
        }

        [Fact]
        public void CompareHash_CaseInsensitive_ReturnsMatch()
        {
            var result = _calculator.CompareHash(
                "2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824",
                "2CF24DBA5FB0A30E26E83B2AC5B9E29E1B161E5C1FA7425E73043362938B9824");
            Assert.True(result.IsMatch);
        }

        [Fact]
        public void CompareHash_DifferentHashes_ReturnsMismatch()
        {
            var result = _calculator.CompareHash(
                "2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824",
                "0000000000000000000000000000000000000000000000000000000000000000");
            Assert.False(result.IsMatch);
        }

        [Fact]
        public void CompareHash_WithSpacesAndDashes_ReturnsMatch()
        {
            var result = _calculator.CompareHash(
                "d41d8cd98f00b204e9800998ecf8427e",
                "d41d8cd9-8f00b204-e9800998-ecf8427e");
            Assert.True(result.IsMatch);
        }

        // DetectHashType tests

        [Fact]
        public void DetectHashType_32Chars_ReturnsMD5()
        {
            Assert.Equal(HashType.MD5, HashCalculator.DetectHashType("d41d8cd98f00b204e9800998ecf8427e"));
        }

        [Fact]
        public void DetectHashType_40Chars_ReturnsSHA1()
        {
            Assert.Equal(HashType.SHA1, HashCalculator.DetectHashType("da39a3ee5e6b4b0d3255bfef95601890afd80709"));
        }

        [Fact]
        public void DetectHashType_64Chars_ReturnsSHA256()
        {
            Assert.Equal(HashType.SHA256,
                HashCalculator.DetectHashType("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"));
        }

        [Fact]
        public void DetectHashType_UnknownLength_ReturnsNull()
        {
            Assert.Null(HashCalculator.DetectHashType("abc123"));
        }

        // CalculateHash dispatch tests

        [Fact]
        public async Task CalculateHash_MD5Type_UsesMD5()
        {
            var path = CreateTestFileBytes(Array.Empty<byte>());
            var hash = await _calculator.CalculateHash(path, HashType.MD5);
            Assert.Equal("d41d8cd98f00b204e9800998ecf8427e", hash);
        }

        [Fact]
        public async Task CalculateHash_InvalidType_ThrowsArgumentOutOfRange()
        {
            var path = CreateTestFile("test");
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _calculator.CalculateHash(path, (HashType)99));
        }
    }
}
