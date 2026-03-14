# HashGuard User Guide

This guide covers every feature of HashGuard in detail.

---

## Getting Started

### System Requirements

- Windows 10 or later (x64 or x86)
- No additional runtime needed (self-contained build includes .NET)

### Launching HashGuard

Simply double-click `HashGuard.exe`. No installation or configuration is required.

---

## Selecting Files

### Method 1: Drag and Drop

Drag one or more files from Windows Explorer directly onto the HashGuard window. The drop zone will highlight when files are detected.

### Method 2: File Dialog

Click the **Select Files** button or press **Ctrl+O** to open a standard file selection dialog. You can select multiple files by holding Ctrl or Shift.

### Method 3: Menu

Use **File → Open Files...** from the menu bar.

Selected files appear in a list below the drop zone.

---

## Choosing Hash Algorithms

Before calculating, select which algorithms to use with the checkboxes:

- **MD5** — 128-bit hash (32 hex characters). Fast but cryptographically weak.
- **SHA-1** — 160-bit hash (40 hex characters). Deprecated for security use.
- **SHA-256** — 256-bit hash (64 hex characters). Recommended for most purposes.
- **SHA-512** — 512-bit hash (128 hex characters). Maximum security.

By default, MD5, SHA-1, and SHA-256 are selected. Select or deselect as needed.

---

## Calculating Hashes

Click **Calculate Hashes** (or use the keyboard shortcut). For multiple files, a progress bar shows completion status.

Results appear in the results section with one text field per algorithm. Each hash can be copied to the clipboard using the copy button next to it.

### Cancelling

For large batches, click **Cancel** to stop the operation. Files already processed will retain their results.

---

## Comparing Hashes

After calculating, use the **Compare Hash** section:

1. Paste the expected hash into the text field
2. Click **Compare**
3. Results:
   - **Green banner**: The hash matches one of the computed values
   - **Red banner**: No match found — the file may be corrupted or tampered with

HashGuard automatically detects which algorithm the expected hash corresponds to based on its length.

---

## Exporting Reports

Click **Export Report** or press **Ctrl+R** to save results. Choose from three formats:

### Text Report (.txt)
A human-readable report with a header, file details, and all computed hashes. Good for printing or sharing.

### JSON Report (.json)
Machine-readable structured data. Ideal for integration with other tools or scripts.

### CSV Report (.csv)
Comma-separated values suitable for spreadsheet applications like Excel.

---

## File Integrity Baselines

Baselines let you save a snapshot of file hashes and compare later to detect changes.

### Saving a Baseline

1. Select and hash the files you want to monitor
2. Click **Save Baseline** (or use **Baseline → Save Baseline** menu)
3. Choose a save location (`.hgb` extension)

### Comparing Against a Baseline

1. Select the same files and calculate their current hashes
2. Click **Compare Baseline** (or use **Baseline → Compare with Baseline** menu)
3. Select the previously saved `.hgb` file
4. A summary dialog shows:
   - **Matched** files (unchanged since baseline)
   - **Changed** files (hashes differ from baseline)
   - **New** files (not present in the baseline)

---

## Themes

Toggle between dark and light themes via **View → Toggle Dark/Light Theme**.

---

## Keyboard Shortcuts

| Shortcut | Action |
|---|---|
| Ctrl+O | Open file selection dialog |
| Ctrl+R | Export report |
| Ctrl+Shift+C | Clear all results and reset |

---

## Troubleshooting

### "Access denied" error for a file
The file may be locked by another process or require administrator privileges. Try closing other programs that might be using the file, or run HashGuard as administrator.

### Hash doesn't match but file seems correct
- Ensure you're comparing the right hash type (e.g., don't compare an MD5 against a SHA-256)
- Check for extra whitespace or characters in the pasted hash
- Re-download the file — the original download may have been corrupted

### Application shows SmartScreen warning
This is expected for unsigned executables. See the Security Notice in the README for details.
