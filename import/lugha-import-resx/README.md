
# Lugha.Import.Resx

Roslyn incremental source generator that converts `.resx` and `.resw` resource files into typed Lugha text scopes at compile time.

```sh
dotnet add package Lugha.Import.Resx
```

You also need the core `Lugha` package for the runtime types referenced by generated code:

```sh
dotnet add package Lugha
```

## How it works

The generator filters `AdditionalFiles` for `.resx` and `.resw` extensions and emits two kinds of output:

1. **Contracts** - `ITextScope` interfaces generated from the reference file. A file without a language segment in its name (e.g. `Strings.resx`) is treated as the reference.
2. **Implementations** - sealed locale classes generated from each language-tagged file (e.g. `Strings.es-ES.resx`), implementing the contract interfaces.

Language tags are extracted from the filename convention `Name.lang.resx`. Tags must start with two or more alphabetic characters and may include region subtags (e.g. `en-GB`, `zh-Hans`).

All parsing is delegated to `ResxParser` and code emission to `CodeEmitter` from the `Lugha.Import` library. The emitter resolves CLDR rule types via `LanguageRules.Resolve()` from `Lugha.Common`.

## Usage

Add `.resx`/`.resw` files as `AdditionalFiles` in the consuming project:

```xml
<ItemGroup>
  <AdditionalFiles Include="Resources\Strings.resx" />
  <AdditionalFiles Include="Resources\Strings.*.resx" />
</ItemGroup>
```

### Example input

A reference file (`Resources/Strings.resx`):

```xml
<root>
  <data name="Connection_Discovering" xml:space="preserve">
    <value>Discovering</value>
  </data>
  <data name="Connection_Connected" xml:space="preserve">
    <value>Connected to {0}</value>
    <comment>{0} = host</comment>
  </data>
</root>
```

A locale file (`Resources/Strings.es-ES.resx`):

```xml
<root>
  <data name="Connection_Discovering" xml:space="preserve">
    <value>Descubriendo</value>
  </data>
  <data name="Connection_Connected" xml:space="preserve">
    <value>Conectado a {0}</value>
    <comment>{0} = host</comment>
  </data>
</root>
```

### Generated output

The generator converts underscores to dots for scope grouping. `Connection_Discovering` becomes the `Discovering` member on `IConnectionText`:

```csharp
public interface IConnectionText : ITextScope
{
    string Discovering { get; }
    string Connected(string host);
}
```

```csharp
public sealed class EsEsConnectionText : IConnectionText
{
    public string Discovering => "Descubriendo";
    public string Connected(string host) => $"Conectado a {host}";
}
```

Format holes (`{0}`) are converted to named parameters. Use `<comment>` elements to provide parameter names: `{0} = host` or `{0}: host`. Without a comment, parameters are named `arg0`, `arg1`, etc.

## MSBuild properties

| Property | Default | Description |
|---|---|---|
| `LughaNamespace` | `Lugha.Generated` | Root namespace for all generated types. |
| `LughaDefaultLanguage` | `en` | Language tag assumed for the reference file (used for CLDR rule resolution). |

The package ships a `.targets` file that declares `CompilerVisibleProperty` for both properties automatically — NuGet consumers need only set the properties in their `.csproj`:

```xml
<PropertyGroup>
  <LughaNamespace>MyApp.Translations</LughaNamespace>
  <LughaDefaultLanguage>en-US</LughaDefaultLanguage>
</PropertyGroup>
```

## NuGet package layout

The package places only `Lugha.Import.Resx.dll` in `analyzers/dotnet/cs/`. Runtime dependencies (`Lugha.Import`, `Lugha.Common`) flow as NuGet package references so the Roslyn compiler host resolves them transitively.

## Build

Targets `netstandard2.0` with `Microsoft.CodeAnalysis.CSharp` 5.0.0. Implements `IIncrementalGenerator` for optimal IDE performance.

```
dotnet build import/lugha-import-resx
```

## Licence

[Apache License 2.0](../../LICENSE)
