# Contributing to HashGuard

Thank you for your interest in contributing to HashGuard! This document provides guidelines and information for contributors.

## How to Contribute

### Reporting Bugs

1. Check existing [Issues](https://github.com/ZadgeIsCool/HashGuard/issues) to avoid duplicates
2. Use the bug report template
3. Include:
   - Steps to reproduce
   - Expected vs actual behavior
   - OS version and .NET version
   - Screenshots if applicable

### Suggesting Features

1. Open a [Feature Request](https://github.com/ZadgeIsCool/HashGuard/issues/new) issue
2. Describe the use case and why it would be valuable
3. Be open to discussion about implementation approach

### Submitting Code

1. **Fork** the repository
2. **Create a branch** from `main`:
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Make your changes** following the coding standards below
4. **Write tests** for new functionality
5. **Run all tests** to ensure nothing is broken:
   ```bash
   dotnet test src/HashGuard.sln
   ```
6. **Commit** with clear, descriptive messages
7. **Push** and open a **Pull Request**

## Development Setup

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Visual Studio 2022 (recommended) or VS Code with C# extension
- Windows 10/11 (required for WPF)

### Building

```bash
# Clone the repository
git clone https://github.com/ZadgeIsCool/HashGuard.git
cd HashGuard

# Restore and build
dotnet build src/HashGuard.sln

# Run tests
dotnet test src/HashGuard.sln
```

## Coding Standards

- Follow [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation comments on all public members
- Use `async/await` for I/O operations
- Handle exceptions appropriately — don't swallow errors silently
- Keep methods focused and under 50 lines where possible

## Code Review Process

1. All submissions require review before merging
2. Maintainers may request changes or improvements
3. CI must pass (build + tests) before merge
4. Squash commits when merging feature branches

## Security

If you discover a security vulnerability, please do **NOT** open a public issue. Instead, email the maintainers directly. See the README for contact information.

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
