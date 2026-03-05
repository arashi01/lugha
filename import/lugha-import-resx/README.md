# Lugha.Import.Resx

Roslyn incremental source generator that converts `.resx` and `.resw` resource files into typed Lugha text scopes at compile time. Published as an independent NuGet package with NuGet dependencies on `Lugha.Import` and `Lugha.Common`.

## How it works

The generator filters `AdditionalFiles` for `.resx` and `.resw` extensions and emits two kinds of output:

1. **Contracts** - `ITextScope` interfaces generated from the reference file. A file without a language segment in its name (e.g. `Strings.resx`) is treated as the reference.
2. **Implementations** - sealed locale classes generated from each language-tagged file (e.g. `Strings.es-ES.resx`), implementing the contract interfaces.

Language tags are extracted from the filename convention `Name.lang.resx` and validated as two-or-more-letter codes starting with alphabetic characters.

All parsing is delegated to `ResxParser` and code emission to `CodeEmitter` from the `Lugha.Import` library. The emitter resolves CLDR rule types via `LanguageRules.Resolve()` from `Lugha.Common`.

## NuGet package layout

The package places only `Lugha.Import.Resx.dll` in `analyzers/dotnet/cs/`. Runtime dependencies (`Lugha.Import`, `Lugha.Common`) flow as NuGet package references so the Roslyn compiler host resolves them transitively.

## MSBuild properties

| Property | Default | Description |
|---|---|---|
| `LughaNamespace` | `Lugha.Generated` | Root namespace for all generated types. |
| `LughaDefaultLanguage` | `en` | Language tag assumed for the reference file. |

## Usage

Add `.resx`/`.resw` files as `AdditionalFiles` in the consuming project:

```xml
<ItemGroup>
  <AdditionalFiles Include="Resources\Strings.resx" />
  <AdditionalFiles Include="Resources\Strings.*.resx" />
</ItemGroup>
```

## Build

Targets `netstandard2.0` with `Microsoft.CodeAnalysis.CSharp` 5.0.0. Implements `IIncrementalGenerator` for optimal IDE performance.

```
dotnet build import/lugha-import-resx
```

## Licence

[Apache License 2.0](../../LICENSE)
