# Lugha.Import.Gettext

Roslyn incremental source generator that converts GNU Gettext `.po` and `.pot` files into typed Lugha text scopes at compile time. Not published independently - consumed as a project reference or packed into a host NuGet package.

## How it works

The generator filters `AdditionalFiles` for `.po` and `.pot` extensions and emits two kinds of output:

1. **Contracts** - `ITextScope` interfaces generated from the reference locale. A `.pot` file is used as the reference; if none exists, the first `.po` file serves as the reference.
2. **Implementations** - sealed locale classes generated from each `.po` file, implementing the contract interfaces.

All parsing is delegated to `GettextParser` and code emission to `CodeEmitter` from the `Lugha.Import` library.

## MSBuild properties

| Property | Default | Description |
|---|---|---|
| `LughaNamespace` | `Lugha.Generated` | Root namespace for all generated types. |

## Usage

Add `.po`/`.pot` files as `AdditionalFiles` in the consuming project:

```xml
<ItemGroup>
  <AdditionalFiles Include="Translations\*.pot" />
  <AdditionalFiles Include="Translations\*.po" />
</ItemGroup>
```

## Build

Targets `netstandard2.0` with `Microsoft.CodeAnalysis.CSharp` 4.12.0. Implements `IIncrementalGenerator` for optimal IDE performance.

```
dotnet build import/lugha-import-gettext
```

## Licence

[Apache License 2.0](../../LICENSE)
