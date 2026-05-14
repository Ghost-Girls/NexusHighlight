# Changelog
All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [1.0.0] - 2026-03-20 (Merged Release)

### This is a major merged release combining features from two projects:
- [Better Comments](https://github.com/omsharp/BetterComments) by Omar Rwemi
- [Highlighter](https://github.com/daxpandhi/Highlighter) by Dax Pandhi

### Added
- Unified rule system supporting both foreground and background styling
- Background highlighting support (Tag, Tag Under, Line, Line Under shapes)
- Solution-specific rules in addition to global rules
- Unlimited custom comment rules with flexible matching criteria
- Drag and drop rule reordering with instant persistence
- Rule import/export functionality
- Case-sensitive and partial match options for background rules
- Blur and transparency effects for background styles
- Dual right-click menu entries (Foreground Styles Rule / Background Styles Rule)
- Global and Solution rule tabs in Options page
- Deferred UI rendering for improved performance

### Changed
- Rules now store both Foreground and Background configurations
- Rule order determined by JSON array position (no separate Order field)
- File I/O moved to background threads to prevent UI freeze
- Configuration changes apply asynchronously
- Updated plugin description in VSIX manifest

### Fixed
- 4-5 second UI freeze when creating new rules
- Rule drag-and-drop ordering not persisting to disk

### Credits
- Original Better Comments: Omar Rwemi
- Original Highlighter: Dax Pandhi
- Merged project: Ghost-Girls

## [Unreleased] - Better Comments Original

### Features
- Customize comment font, opacity, and size independently of editor settings
- Four additional comment classifications (Important, Question, Remove, Task)
- Per-classification customizable foreground colors
- Bold, italic, underline, and strikethrough options
