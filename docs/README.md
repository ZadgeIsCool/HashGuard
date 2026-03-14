# HashGuard

**A professional, open-source file integrity verification tool for Windows.**

---

## Why HashGuard Exists

Every day, millions of files are downloaded from the internet — software installers, firmware updates, documents, and more. How do you know the file you downloaded is exactly what the publisher intended? A single flipped bit from a corrupted download, or worse, a malicious modification by an attacker, can have serious consequences.

**Cryptographic hash verification** is the standard answer. Publishers provide hash values (checksums) alongside their downloads. By computing the hash of your downloaded file and comparing it to the published value, you can confirm the file's integrity.

The problem? Most hash verification tools on Windows are either:
- Command-line only (intimidating for many users)
- Bloated with unnecessary features
- Closed-source (ironic for a security tool)
- Outdated and unmaintained

**HashGuard** solves this by providing a clean, modern, open-source GUI tool that makes file integrity verification fast, easy, and accessible to everyone.

---

## Features

- **Multiple Hash Algorithms** — MD5, SHA-1, SHA-256, and SHA-512
- **Drag & Drop** — Simply drag files onto the window
- **Batch Processing** — Verify multiple files at once with progress tracking
- **Hash Comparison** — Paste an expected hash and instantly see if it matches
- **Auto-Detection** — Automatically identifies hash type by length
- **Export Reports** — Save results as TXT, JSON, or CSV
- **Integrity Baselines** — Save hash snapshots and compare later to detect changes
- **Dark/Light Themes** — Easy on the eyes, day or night
- **Keyboard Shortcuts** — Ctrl+O (open), Ctrl+R (report), Ctrl+Shift+C (clear)
- **Portable** — Single executable, no installation required
- **Open Source** — MIT licensed, fully auditable code

---

## Installation

### Option 1: Download Pre-built Release (Recommended)

1. Go to the [Releases](https://github.com/ZadgeIsCool/HashGuard/releases) page
2. Download `HashGuard-win-x64.zip` (or `win-x86` for 32-bit systems)
3. Extract and run `HashGuard.exe` — no installation needed

### Option 2: Build from Source

**Prerequisites:** [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

```bash
git clone https://github.com/ZadgeIsCool/HashGuard.git
cd HashGuard
dotnet build src/HashGuard.sln --configuration Release
```

To create a self-contained single executable:

```bash
dotnet publish src/HashGuard/HashGuard.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

The output will be in `src/HashGuard/bin/publish/win-x64/`.

---

## Usage

### Basic Hash Verification

1. Launch `HashGuard.exe`
2. Drag a file onto the drop zone (or click **Select Files**)
3. Choose the hash algorithms you need (SHA-256 is selected by default)
4. Click **Calculate Hashes**
5. View the computed hashes in the results area

### Comparing Against a Known Hash

1. Calculate the file's hashes (steps above)
2. In the **Compare Hash** section, paste the expected hash
3. Click **Compare** — a green banner means MATCH, red means MISMATCH

### Batch Processing

1. Select or drag multiple files at once
2. Click **Calculate Hashes** — progress is shown in real time
3. Export results via **Export Report** (TXT, JSON, or CSV)

### File Integrity Monitoring

1. Calculate hashes for your important files
2. Click **Save Baseline** to create a snapshot
3. Later, recalculate and click **Compare Baseline** to detect any changes

### Keyboard Shortcuts

| Shortcut | Action |
|---|---|
| `Ctrl+O` | Open file selection dialog |
| `Ctrl+R` | Export report |
| `Ctrl+Shift+C` | Clear all results |

---

## Screenshots

*Screenshots will be added after the first release build.*

---

## Security Notice

### Code Signing

HashGuard is a security tool, so it's important to verify the integrity of HashGuard itself. Official releases will be code-signed when a certificate is available.

**Why this matters:** An unsigned executable will show a Windows SmartScreen warning. This is expected for unsigned software and does not indicate malware. However, because HashGuard is a tool specifically designed for verifying file integrity, we strongly recommend:

1. **Download only from the official GitHub repository**
2. **Verify the release hash** against the values published in the release notes
3. **Build from source** if you want maximum assurance — the code is fully open

### Hash Algorithm Recommendations

| Algorithm | Security Level | Recommended Use |
|---|---|---|
| MD5 | Weak | Legacy compatibility only |
| SHA-1 | Deprecated | Legacy compatibility only |
| **SHA-256** | **Strong** | **General purpose — recommended** |
| SHA-512 | Strong | When maximum security is needed |

MD5 and SHA-1 are included for compatibility (many publishers still provide these), but **SHA-256 should be your default choice** for security verification.

---

## FAQ

**Q: Is HashGuard safe to use?**
A: Yes. HashGuard is open source — you can audit every line of code. It uses .NET's built-in `System.Security.Cryptography` library, which wraps the operating system's certified cryptographic implementations.

**Q: Why does Windows SmartScreen warn about HashGuard?**
A: SmartScreen warns about any unsigned executable. This is normal behavior. See the Security Notice above for guidance on verifying HashGuard's integrity.

**Q: Can HashGuard handle very large files?**
A: Yes. HashGuard reads files in 1 MB chunks, so memory usage stays constant regardless of file size. Files of any size (including multi-GB ISOs) are supported.

**Q: What's the difference between hash verification and antivirus scanning?**
A: Hash verification confirms a file matches an expected version — it proves the file hasn't been modified. Antivirus scanning checks for known malware signatures. They serve different purposes and complement each other.

**Q: Can I use HashGuard commercially?**
A: Yes. HashGuard is MIT licensed, which permits commercial use with no restrictions.

---

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](../CONTRIBUTING.md) for guidelines.

---

## License

HashGuard is released under the [MIT License](../LICENSE).

---

## Acknowledgments

Built with:
- [.NET 8.0](https://dotnet.microsoft.com/) and WPF
- [Newtonsoft.Json](https://www.newtonsoft.com/json) for JSON serialization
- [xUnit](https://xunit.net/) for testing
