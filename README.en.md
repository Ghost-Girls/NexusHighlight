# NexusHighlight

[![Visual Studio](https://img.shields.io/badge/Visual%20Studio-2022%2B-5C2D91.svg)](https://visualstudio.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

**中文: [中文](README.md)**

---

**NexusHighlight** is a Visual Studio extension that gives you complete control over how comments look in the editor. Customize font, size, opacity, foreground colors, background styles — all independently of the editor's default settings.

---

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [General Options](#general-options)
- [Custom Rules](#custom-rules)
  - [Foreground Styles](#foreground-styles)
  - [Background Styles](#background-styles)
  - [Matching Options](#matching-options)
- [Rule Management](#rule-management)
  - [Global Rules vs Solution Rules](#global-rules-vs-solution-rules)
  - [Drag & Drop Reordering](#drag--drop-reordering)
  - [Import & Export](#import--export)
- [Supported Languages](#supported-languages)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

NexusHighlight lets you transform plain code comments into visually meaningful annotations. Whether you want to highlight important notes, mark tasks, distinguish questions, or simply make comments more readable, NexusHighlight gives you the tools to do it.

<!-- TODO: [Screenshot] Editor showing various styled comments — different colors, background highlights, bold/italic text -->
<!-- ![Overview Example](screenshots/overview.png) -->

---

## Getting Started

1. Install the extension from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/)
2. Open **Tools → Options → NexusHighlight**
3. Start with the **General** page to set global comment appearance
4. Move to the **Rules** page to create custom matching rules

---

## General Options

The General page provides global settings that affect all comments in the editor.

<!-- TODO: [Screenshot] General Options page showing all controls -->
<!-- ![General Options Page](screenshots/general-options.png) -->

### UI Language

NexusHighlight supports both **Chinese** and **English** UI. Switch between them at any time from the General page.

### Font & Size

- **Font** — Choose any font installed on your system. Comments will use this font regardless of the editor's font setting.
- **Size Offset** — Adjust comment font size relative to the default editor font size. The slider ranges from **-3** (smaller) to **+3** (larger), with 0.5 increments.

### Opacity

Control comment transparency with a slider from **0%** (fully transparent) to **100%** (fully opaque). This is useful for reducing visual noise while keeping comments readable.

<!-- TODO: [Screenshot] Opacity comparison — same code with different opacity levels -->
<!-- ![Opacity Comparison](screenshots/opacity-comparison.png) -->

### Style Toggles

- **Italic** — Toggle italic style for all comments globally
- **Color only the 'Todo' keyword** — When enabled, only the word "Todo" (case-insensitive) in task comments gets colored, rather than the entire comment line
- **Underline important comments** — Automatically underline comments that start with `!`
- **Strikethrough double comments** — Apply strikethrough to double-comment lines (e.g., `// // text` in C#)

---

## Custom Rules

The Rules page is where NexusHighlight truly shines. You can create unlimited rules, each with its own matching criteria and visual style.

<!-- TODO: [Screenshot] Rules page showing the full rule list with various rules configured -->
<!-- ![Rules Page](screenshots/rules-page.png) -->

Each rule consists of a **Criteria** (the text pattern to match) and a set of **Foreground** and **Background** style options.

### Foreground Styles

Control how the matched text looks:

| Option | Description |
|--------|-------------|
| **Color** | Pick from a rich color palette or enter a custom hex value |
| **Bold** | Make matched text bold |
| **Italic** | Make matched text italic |
| **Underline** | Add underline to matched text |
| **Strikethrough** | Add strikethrough to matched text |

Each rule has an **Enable Foreground** toggle, so you can turn the foreground styling on or off independently.

<!-- TODO: [Screenshot] Foreground style examples — different colors and font styles applied to comments -->
<!-- ![Foreground Examples](screenshots/foreground-examples.png) -->

### Background Styles

Add background highlights to make comments stand out even more.

#### Background Shapes

| Shape | Visual Effect |
|-------|---------------|
| **Tag** | A rounded rectangle behind the matched text, like a label |
| **Tag Under** | Same as Tag, but with an additional underline |
| **Line** | Highlights the entire line containing the match |
| **Line Under** | Same as Line, but with an additional underline |

<!-- TODO: [Screenshot] Four background shapes applied to the same text for comparison -->
<!-- ![Background Shapes](screenshots/background-shapes.png) -->

#### Background Blur

Control how sharp or soft the background edge looks:

| Level | Effect |
|-------|--------|
| None | Sharp, solid background |
| Low | Slightly soft edges |
| Medium | Noticeable blur |
| High | Strong blur effect |
| Ultra | Maximum blur, almost glow-like |

#### Background Transparency

10 levels of transparency from **0/10** (fully transparent / invisible) to **10/10** (fully opaque / solid).

<!-- TODO: [Screenshot] Same background color with different blur and transparency levels -->
<!-- ![Blur and Transparency](screenshots/blur-transparency.png) -->

### Matching Options

Fine-tune how rules match text in comments:

- **Case Sensitive** — When enabled, the match is case-sensitive (e.g., "Todo" will not match "todo")
- **Partial Match** — When enabled, the rule matches text anywhere within a comment (e.g., criteria "note" will match "important notes")

---

## Rule Management

### Global Rules vs Solution Rules

NexusHighlight supports two levels of rule scoping:

| Scope | Description |
|-------|-------------|
| **Global Rules** | Apply to all projects you open in Visual Studio |
| **Solution Rules** | Apply only to the currently open solution |

This is useful when you want different teams or projects to have different comment conventions.

<!-- TODO: [Screenshot] Global Rules tab vs Solution Rules tab -->
<!-- ![Rule Scopes](screenshots/rule-scopes.png) -->

### Solution Rule Operations

| Operation | Description |
|-----------|-------------|
| **Add Solution** | Add a new rule to the solution scope |
| **Copy from Global** | Copy all global rules into solution rules (overwrites existing) |
| **Import from Global** | Selectively choose which global rules to import into solution rules |
| **Clear** | Remove all solution rules |
| **Export** | Export solution rules to a JSON file |
| **Import** | Import solution rules from a JSON file |

### Drag & Drop Reordering

Rules are evaluated from top to bottom — the first matching rule wins. Drag the **☰** handle to reorder rules and control priority.

<!-- TODO: [Screenshot] Drag handle being used to reorder rules -->
<!-- ![Drag to Reorder](screenshots/drag-reorder.png) -->

### Import & Export

Share your rule configurations with your team or across machines:

- Export rules as **JSON** files
- Import with two modes:
  - **Overwrite** — Replace existing rules with the imported ones
  - **Merge** — Add imported rules alongside existing ones

---

## Supported Languages

NexusHighlight works with the following languages:

| Language | File Extensions |
|----------|----------------|
| C# | `.cs` |
| F# | `.fs` |
| VB.NET | `.vb` |
| C/C++ | `.c`, `.cpp`, `.h`, `.hpp` |
| JavaScript / TypeScript | `.js`, `.jsx`, `.ts`, `.tsx` |
| Python | `.py` |
| HTML / XAML | `.html`, `.xaml`, `.xml` |

<!-- TODO: [Screenshot] Same comment pattern styled across different languages -->
<!-- ![Multi-language Support](screenshots/multi-language.png) -->

---

## Contributing

Contributions are welcome! Feel free to submit issues and pull requests on the project repository.

## License

[Apache 2.0](LICENSE)

**Publisher**: Ghost-Girls