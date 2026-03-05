# Lugha

**لغة** - Arabic and Swahili for _language_: lugha

Typed localisation for .NET 10 - compile-time enforced text contracts with CLDR pluralisation and bidirectional text support.

Zero string keys. Zero resource files. Zero runtime lookups. Zero ambient state.

## The Problem

Most .NET localisation approaches - `.resx`/`ResourceManager`, `.resw`/`ResourceLoader`, `IStringLocalizer` - rely on string keys. Nothing connects definition site and usage site at compile time.

| Failure mode | Cause | Detection |
|--|--|--|
| Missing translation | Key absent in target locale | Runtime (silent fallback) |
| Orphaned resource | Key removed from code, retained in file | Never |
| Key typo | `"StausDiscovering"` vs `"StatusDiscovering"` | Runtime (silent) |
| Parameter mismatch | `"Connected to {0}"` in EN, `"Connecte"` (missing `{0}`) in FR | Runtime |
| Missing string | New feature, no resource entry | Runtime (empty string) |
| Ambient culture | `ResourceManager` reads `CultureInfo.CurrentUICulture` | Non-deterministic |

Lugha eliminates the entire category.

## Core Principle

**Text contracts are interfaces. Locales are implementations. The compiler enforces exhaustiveness.**

```
Interface member added       ->  Every locale fails to compile until implemented.
Interface member removed     ->  Every locale providing it fails to compile.
Parameter signature changed  ->  Every locale fails to compile until matched.
```

## Architecture

### **Text Scope:** Domain-Segmented Contracts

A **text scope** is an interface defining the text surface for a bounded domain. Properties return invariant text (labels, titles, static messages). Methods return parameterised text (formatted messages, interpolated values). All members must return `string` - the Roslyn analyser `LGH001` (severity: Error) enforces this.

```csharp
public interface IConnectionText : ITextScope
{
    string Discovering { get; }
    string Connecting(string host);
    string Connected(string host);
    string Unavailable(string reason);
}

public interface INavigationText : ITextScope
{
    string Dashboard { get; }
    string Directory { get; }
    string Catalogue { get; }
    string Settings { get; }
}
```

`ITextScope` is a marker interface. Its purpose:
1. The Lugha analyser and source generator scan for `ITextScope` derivatives.
2. Generic constraints: `where TScope : ITextScope`.
3. Self-documenting: any interface extending `ITextScope` is recognisable as a text contract.

### **Locale:** Typed Composite

A **locale** composes all text scope implementations for a specific language/region. Locale instances are constructed once and reused. Text scope implementations must be stateless and pure.

```csharp
public interface IAppLocale : ILocale
{
    IConnectionText Connection { get; }
    INavigationText Navigation { get; }
    IDirectoryText Directory { get; }
    ICommandText Commands { get; }
}
```

`ILocale<TCardinal, TOrdinal>` binds independent cardinal and ordinal rule sets with default interface methods that enforce non-negative counts. Cardinal and ordinal rules are separate type parameters because languages that share cardinal rules frequently diverge on ordinals (e.g. Russian and Ukrainian share `EastSlavicCardinal` but have different ordinal systems).

```csharp
public sealed class EnGbLocale : IAppLocale, ILocale<OneOtherCardinal, EnglishOrdinal>
{
    private static readonly CultureInfo EnGb =
        CultureInfo.GetCultureInfo("en-GB");

    public CultureInfo Culture => EnGb;

    public IConnectionText Connection { get; } = new EnGbConnectionText();
    public INavigationText Navigation { get; } = new EnGbNavigationText();
    public IDirectoryText Directory { get; } = new EnGbDirectoryText(EnGb);
    public ICommandText Commands { get; } = new EnGbCommandText();
}
```

#### Why Two Type Parameters

CLDR evidence (from [`ordinals.xml`](https://github.com/unicode-org/cldr/blob/main/common/supplemental/ordinals.xml)):

| Language | Cardinal rule | Ordinal rule | Can share single struct? |
|--|--|--|--|
| English (en) | one/other | one/two/few/other | No - unique ordinals |
| German (de) | one/other | other-only | No - different ordinal than English |
| Swedish (sv) | one/other | one/other (n%10=1,2 and n%100!=11,12) | No - different ordinal than German |
| Italian (it) | one/other | many/other (8,11,80,800) | No - unique ordinals |
| Russian (ru) | one/few/many/other | other-only | No - different ordinal than Ukrainian |
| Ukrainian (uk) | one/few/many/other | few/other (n%10=3 and n%100!=13) | No - different ordinal than Russian |
| French (fr) | one/other | one/other (n=1) | No - different ordinal than Spanish |

A single `IPluralRules<TSelf>` forced incorrect groupings. The split eliminates this entirely.

### Composition over Flat Inheritance

`IAppLocale` uses **property composition**, not interface inheritance.

| Flat inheritance | Property composition |
|--|--|
| `locale.Dashboard` - ambiguous origin | `locale.Navigation.Dashboard` - unambiguous |
| All members on one surface - polluted autocomplete | Nested access - scoped autocomplete |
| Name collisions require disambiguation | Impossible - each scope is a separate type |
| Cannot pass a single scope to a component | `IConnectionText` is independently referenceable |

### Why Interfaces, Not Abstract Records

1. Locale text implementations carry only behaviour (returning strings), not data.
2. Interface implementation is enforced unconditionally by the compiler.
3. Interfaces allow locale implementations to spread across files/classes naturally.
4. The JIT devirtualises single-implementation interface calls - zero overhead for single-locale applications. For multi-locale applications, the cost is dwarfed by string formatting. PGO further assists monomorphic sites.

## Comparison

| Aspect | `.resx` / `ResourceManager` | `IStringLocalizer` | Lugha |
|--|--|--|--|
| Missing key | Runtime empty string | `ResourceNotFound` | Compile error |
| Missing locale coverage | Silent | Silent | Compile error |
| Parameter mismatch | Runtime `FormatException` | Runtime `FormatException` | Compile error |
| Ambient culture | `CurrentUICulture` | `CurrentUICulture` | None (explicit) |
| Runtime lookup cost | Hashtable + assembly probe | Hashtable + assembly probe | Zero (direct call) |
| Pluralisation | Manual | None | CLDR-typed (cardinal + ordinal) |
| RTL support | None | None | Bidi isolation (string + span) |
| Framework coupling | `ResourceManager` | `IServiceCollection` | None |
| Hot-path allocation | String per lookup | String per lookup | Zero (`TryFormat` + `TryIsolate`) |

## Getting Started

**Hand-authored locales (recommended starting point):** install `Lugha` alone and follow the [quick start guide](lugha/README.md).

**Existing `.resx` or `.resw` files:** install `Lugha` and `Lugha.Import.Resx`. The source generator converts resource files to typed text scopes at compile time. See [Lugha.Import.Resx](import/lugha-import-resx/README.md).

**Existing Gettext `.po`/`.pot` files:** install `Lugha` and `Lugha.Import.Gettext`. See [Lugha.Import.Gettext](import/lugha-import-gettext/README.md).

**Design-time CLI import:** install the `Lugha.Cli` global tool for one-off file conversion. See [Lugha.Cli](import/lugha-cli/README.md).

**WinUI 3 runtime language switching:** install `Lugha.WinUI` for the reactive locale host and registry. See [Lugha.WinUI](integration/lugha-winui/README.md).

## Packages

| Package | Description |
|--|--|
| [`Lugha`](lugha/) | Core runtime library, Roslyn analysers, and source generators. [API documentation](lugha/README.md). |
| [`Lugha.Analysers`](lugha-analysers/) | Roslyn diagnostic analysers (LGH001, LGH003-LGH008). Packed into `Lugha`. [Documentation](lugha-analysers/README.md). |
| [`Lugha.Generators`](lugha-generators/) | Incremental source generators (LGH002, LocaleManifest). Packed into `Lugha`. [Documentation](lugha-generators/README.md). |
| [`Lugha.Common`](lugha-common/) | Shared types (language-to-CLDR-rule mapping) for the import ecosystem. [Documentation](lugha-common/README.md). |
| [`Lugha.Import`](import/lugha-import/) | Shared import library - parsers and code emitter for converting translation files to typed source. [Documentation](import/lugha-import/README.md). |
| [`Lugha.Import.Gettext`](import/lugha-import-gettext/) | Source generator for GNU Gettext `.po`/`.pot` files. [Documentation](import/lugha-import-gettext/README.md). |
| [`Lugha.Import.Resx`](import/lugha-import-resx/) | Source generator for `.resx`/`.resw` resource files. [Documentation](import/lugha-import-resx/README.md). |
| [`Lugha.Cli`](import/lugha-cli/) | .NET global tool for design-time translation import. [Documentation](import/lugha-cli/README.md). |
| [`Lugha.WinUI`](integration/lugha-winui/) | WinUI 3 integration - reactive locale host and registry. [Documentation](integration/lugha-winui/README.md). |

## Design Properties

- **Pure functions.** Every Lugha API is a pure function. No I/O, no ambient state. `CultureInfo` is always an explicit parameter, never read from `CultureInfo.CurrentUICulture`.
- **Thread-safe.** All Lugha types are thread-safe. Locale instances may be shared freely across threads.
- **Zero runtime dependencies.** All CLDR rules are hand-implemented as pure functions. The only dependency is the .NET 10 BCL.
- **Framework-agnostic.** No framework coupling. WinUI, WPF, Blazor, console - any host works. For WinUI/WPF, `x:Bind` with `Mode=OneTime` evaluates text scope properties once at load; missing members fail the XAML codegen build.
- **Non-negative count invariant.** All entry points (`Plural.*`, `Ordinal.*`, `ILocale.Cardinal`, `ILocale.Ordinal`) throw `ArgumentOutOfRangeException` for negative `count` values.

## Requirements

- .NET 10 SDK 10.0.100 or later
- C# 14 (uses `field` keyword, extension blocks, `static abstract` interface members, default interface methods)

## Building

```
dotnet build
dotnet test
```

## Licence

[Apache License 2.0](LICENSE)
