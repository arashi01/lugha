# Lugha.Import

Shared import library that converts external translation files into typed Lugha source code. Format-specific parsers produce a common intermediate representation which a single code emitter transforms into `ITextScope` interfaces and sealed locale classes.

## Architecture

```
External files (.po / .pot / .resx / .resw)
        |
        v
  Format-specific parsers
  (GettextParser / ResxParser)
        |
        v
  TranslationSet              <-- format-neutral IR
  +-- TranslationEntry[]
        |
        v
    CodeEmitter
    |-- EmitContracts()        --> ITextScope interfaces
    +-- EmitImplementations()  --> sealed locale classes
        |
        v
  EmittedFile[]               --> C# source strings
```

## API

### Data types

| Type | Kind | Purpose |
|---|---|---|
| `TranslationEntry` | sealed record | One translated text: `Key` (dot-delimited scope.member), `Value`, `Parameters` (ordered names), `PluralForms` (CLDR category to text, nullable). |
| `TranslationSet` | sealed record | A complete locale: `Language` (BCP 47 tag) and `Entries` (list of `TranslationEntry`). |
| `EmittedFile` | sealed record | A generated source file: `FileName` and `Content`. |

### Parsers

**`GettextParser`** (static class in `Lugha.Import.Gettext`) - parses `.po` and `.pot` files. Handles `msgctxt` as scope prefix, plural forms via `msgid_plural`/`msgstr[N]`, and both named (`{name}`) and positional (`%s`/`%d`) placeholders.

- `Parse(string content)` - locale file with language header.
- `ParseTemplate(string content)` - template file (reference locale).

**`ResxParser`** (static class in `Lugha.Import.Resx`) - parses `.resx` and `.resw` XML files. Converts `_` to `.` in resource keys, extracts `{0}`-style format holes, and reads `<comment>` elements for named parameter hints.

- `Parse(string content, string language)` - returns a `TranslationSet`.

### Code emitter

**`CodeEmitter`** (static class) - transforms translation sets into C# source.

- `EmitContracts(TranslationSet reference, string ns)` - generates `ITextScope` interfaces from the reference locale.
- `EmitImplementations(TranslationSet locale, string contractNs, string targetNs)` - generates sealed classes implementing the scope interfaces.
- `ToInterfaceName(string scopeName)` - converts a scope name to its interface name (e.g. `"Connection"` to `"IConnectionText"`).
- `ToClassPrefix(string languageTag)` - converts a language tag to a class name prefix (e.g. `"en-GB"` to `"EnGb"`).

## Design

- **Two-phase emission**: contracts from a reference locale, implementations from each target. A missing translation becomes an unimplemented interface member - a compile error, not a runtime surprise.
- **Format-neutral IR**: both parsers produce the same `TranslationSet`/`TranslationEntry` types so the emitter is format-agnostic.
- **Plural support**: the emitter resolves CLDR rules via `LanguageRules.Resolve()` from `Lugha.Common` and emits `Plural.Select<TCardinal>(count, new PluralForms { ... })` calls.
- **Parameter typing**: parameters named `"count"` receive `int` type; all others receive `string`.

## Build

Multi-targets `netstandard2.0` and `net10.0`. References `Lugha.Common`.

```
dotnet build import/lugha-import
```

## Licence

[Apache License 2.0](../../LICENSE)
