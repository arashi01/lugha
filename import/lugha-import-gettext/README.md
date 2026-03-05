# Lugha.Import.Gettext

Roslyn incremental source generator that converts GNU Gettext `.po` and `.pot` files into typed Lugha text scopes at compile time.

```sh
dotnet add package Lugha.Import.Gettext
```

You also need the core `Lugha` package for the runtime types referenced by generated code:

```sh
dotnet add package Lugha
```

## How it works

The generator filters `AdditionalFiles` for `.po` and `.pot` extensions and emits two kinds of output:

1. **Contracts** - `ITextScope` interfaces generated from the reference locale. A `.pot` file is used as the reference; if none exists, the first `.po` file serves as the reference.
2. **Implementations** - sealed locale classes generated from each `.po` file, implementing the contract interfaces.

All parsing is delegated to `GettextParser` and code emission to `CodeEmitter` from the `Lugha.Import` library. The emitter resolves CLDR rule types via `LanguageRules.Resolve()` from `Lugha.Common`.

## Usage

Add `.po`/`.pot` files as `AdditionalFiles` in the consuming project:

```xml
<ItemGroup>
  <AdditionalFiles Include="Translations\*.pot" />
  <AdditionalFiles Include="Translations\*.po" />
</ItemGroup>
```

### Example input

A reference template (`Translations/messages.pot`):

```gettext
msgctxt "Connection"
msgid "Discovering"
msgstr ""

msgctxt "Connection"
msgid "Connected to {host}"
msgstr ""
```

A locale file (`Translations/es-ES.po`):

```gettext
msgctxt "Connection"
msgid "Discovering"
msgstr "Descubriendo"

msgctxt "Connection"
msgid "Connected to {host}"
msgstr "Conectado a {host}"
```

### Generated output

The generator emits an `ITextScope` interface from the template:

```csharp
public interface IConnectionText : ITextScope
{
    string Discovering { get; }
    string Connected(string host);
}
```

And a sealed implementation from each `.po` file:

```csharp
public sealed class EsEsConnectionText : IConnectionText
{
    public string Discovering => "Descubriendo";
    public string Connected(string host) => $"Conectado a {host}";
}
```

Parameters named `count` are typed as `int` and generate `Plural.Select` calls. All others are `string`.

## MSBuild properties

| Property | Default | Description |
|---|---|---|
| `LughaNamespace` | `Lugha.Generated` | Root namespace for all generated types. |

## NuGet package layout

The package places only `Lugha.Import.Gettext.dll` in `analyzers/dotnet/cs/`. Runtime dependencies (`Lugha.Import`, `Lugha.Common`) flow as NuGet package references so the Roslyn compiler host resolves them transitively.

## Build

Targets `netstandard2.0` with `Microsoft.CodeAnalysis.CSharp` 5.0.0. Implements `IIncrementalGenerator` for optimal IDE performance.

```sh
dotnet build import/lugha-import-gettext
```

## Licence

[Apache License 2.0](../../LICENSE)
