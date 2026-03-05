# Lugha.Cli

.NET global tool for design-time translation import. Parses external translation files and emits typed `ITextScope` interfaces and sealed locale classes that compile under the normal Lugha contract system.

## Installation

```
dotnet tool install -g Lugha.Cli
```

After installation the `lugha` command is available globally.

## Commands

### `lugha import`

Imports a single translation file and writes generated C# source files to the output directory.

```
lugha import --format <po|resx> --source <file> --namespace <ns> --output <dir> [--contracts] [--language <tag>]
```

**Options:**

| Option | Required | Description |
|---|---|---|
| `--format` | Yes | Translation file format: `po` or `resx`. |
| `--source` | Yes | Path to the translation source file. |
| `--namespace` | Yes | Target C# namespace for generated code. |
| `--output` | Yes | Output directory for generated files. |
| `--contracts` | No | Emit `ITextScope` interfaces (contracts) from a reference locale. |
| `--language` | No | BCP 47 language tag. Required for `resx` when not emitting contracts. |

### Examples

**Emit contracts from a Gettext template:**

```
lugha import --format po --source translations/en-GB.pot --contracts --namespace MyApp.Text --output src/Generated/
```

**Emit a locale implementation from a `.po` file:**

```
lugha import --format po --source translations/es-ES.po --namespace MyApp.Text.EsEs --output src/Generated/
```

**Emit contracts from a `.resx` reference file:**

```
lugha import --format resx --source Resources/Strings.resx --contracts --namespace MyApp.Text --output src/Generated/
```

**Emit a locale implementation from a language-tagged `.resx`:**

```
lugha import --format resx --source Resources/Strings.ja.resx --language ja --namespace MyApp.Text.Ja --output src/Generated/
```

## How it works

The CLI is a thin orchestration layer: parse, emit, write. Each step is a pure function; the CLI handles file I/O. For implementations, the contract namespace is derived by convention as the parent of the target namespace (the portion before the last `.` segment).

The tool references `Lugha.Import` (consuming the `net10.0` build with span-based parser overloads) and delegates CLDR rule resolution to `LanguageRules.Resolve()` from `Lugha.Common`.

## Build

Targets `net10.0`. Packed as a .NET global tool (`PackAsTool=true`, `ToolCommandName=lugha`).

```
dotnet build import/lugha-cli
```

## Licence

[Apache License 2.0](../../LICENSE)
