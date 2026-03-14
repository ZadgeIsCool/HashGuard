# Changelog

All notable changes to HashGuard will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-03-14

### Added

- Initial release of HashGuard
- GUI application built with WPF on .NET 8.0
- Hash calculation support: MD5, SHA-1, SHA-256, SHA-512
- Drag-and-drop file support
- Batch processing with progress reporting
- Hash comparison (paste expected hash to verify)
- Auto-detection of hash algorithm by length
- Export reports in TXT, JSON, and CSV formats
- File integrity baseline save and compare
- Dark and light theme support
- Keyboard shortcuts (Ctrl+O, Ctrl+R, Ctrl+Shift+C)
- Copy individual hash values to clipboard
- About dialog with project information
- Cancellation support for long-running operations
- Chunked file reading for efficient large file handling
- Comprehensive unit test suite
- CI/CD pipeline with GitHub Actions
- Publish profiles for win-x64 and win-x86 (self-contained, single file)
