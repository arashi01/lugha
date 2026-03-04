# Lugha

**لغة** · Arabic and Swahili for _language_: lugha

Typed localisation for .NET 10 - compile-time enforced text contracts with CLDR pluralisation and bidirectional text support.

```sh
dotnet add package Lugha
```

The package includes the runtime library, Roslyn analysers, and source generators. No additional packages are required for core functionality.

Requires .NET 10 (SDK 10.0.100 or later).

## Quick Start

### 1. Define text scopes

A text scope is an interface defining the text surface for a bounded domain. Properties return invariant text; methods return parameterised text. All members must return `string`.

```csharp
using Lugha;

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
    string Settings { get; }
}
```

### 2. Define a composite locale interface

A locale composes all text scope implementations for a specific language/region using property composition - each scope is independently referenceable and name collisions are impossible.

```csharp
public interface IAppLocale : ILocale
{
    IConnectionText Connection { get; }
    INavigationText Navigation { get; }
}
```

### 3. Implement locales

`ILocale<TCardinal, TOrdinal>` binds the correct CLDR plural rules. Cardinal and ordinal rules are independent type parameters because languages that share cardinal rules frequently diverge on ordinals.

```csharp
using System.Globalization;
using Lugha.Rules.Cardinal;
using Lugha.Rules.Ordinal;

public sealed class EnGbLocale
    : IAppLocale, ILocale<OneOtherCardinal, EnglishOrdinal>
{
    private static readonly CultureInfo EnGb =
        CultureInfo.GetCultureInfo("en-GB");

    public CultureInfo Culture => EnGb;
    public IConnectionText Connection { get; } = new EnGbConnectionText();
    public INavigationText Navigation { get; } = new EnGbNavigationText();
}

public sealed class EnGbConnectionText : IConnectionText
{
    public string Discovering => "Discovering\u2026";
    public string Connecting(string host) => $"Connecting to {host}\u2026";
    public string Connected(string host) => $"Connected to {host}";
    public string Unavailable(string reason) => $"Disconnected: {reason}";
}

public sealed class EnGbNavigationText : INavigationText
{
    public string Dashboard => "Dashboard";
    public string Directory => "Directory";
    public string Settings => "Settings";
}
```

### 4. Use it

```csharp
IAppLocale locale = new EnGbLocale();

Console.WriteLine(locale.Connection.Discovering);       // Discovering...
Console.WriteLine(locale.Connection.Connected("srv"));  // Connected to srv
Console.WriteLine(locale.Navigation.Dashboard);         // Dashboard
```

Add a new member to any `ITextScope` interface and every locale that does not implement it becomes a compile error. Remove a member and every locale still providing it becomes a compile error. Change a parameter signature and every locale must match.

## Pluralisation

Lugha provides typed CLDR cardinal and ordinal plural resolution. Rules are implemented as zero-size structs with `static abstract` methods - the JIT monomorphises generic call sites for zero virtual dispatch and zero allocation.

### Plural Forms

`PluralForms` and `OrdinalForms` are `readonly record struct` types carrying up to six CLDR category slots. Only `Other` is required; all remaining slots fall back to `Other` when unset.

```csharp
private static readonly PluralForms PersonForms = new()
{
    Other = "people",
    One   = "person",
};

private static readonly OrdinalForms OrdinalSuffixes = new()
{
    Other = "th",
    One   = "st",
    Two   = "nd",
    Few   = "rd",
};
```

### Static Functions

The `Plural` and `Ordinal` static classes offer two paths:

- **Generic path:** use when the rule type is statically known (zero virtual dispatch):

```csharp
// Select the plural form for a count
string form = Plural.Select<OneOtherCardinal>(1, PersonForms);  // "person"

// Format count + form with culture-aware number formatting
string text = Plural.Format<OneOtherCardinal>(3, PersonForms, culture);  // "3 people"

// Zero-allocation span formatting
Span<char> buffer = stackalloc char[64];
bool ok = Plural.TryFormat<OneOtherCardinal>(3, PersonForms, culture, buffer, out int written);
```

- **Locale path:** use in locale-agnostic components receiving `ILocale`:

```csharp
string form = Plural.Select(1, PersonForms, locale);
string text = Plural.Format(3, PersonForms, locale);
```

Ordinal formatting concatenates without a space (e.g. "1st", "22nd"):

```csharp
string position = Ordinal.Format<EnglishOrdinal>(1, OrdinalSuffixes, culture);  // "1st"
string position = Ordinal.Format<EnglishOrdinal>(22, OrdinalSuffixes, culture); // "22nd"
```

Convenience extensions are available on `ILocale`:

```csharp
string text = locale.PluralFormat(5, PersonForms);       // "5 people"
string text = locale.OrdinalFormat(3, OrdinalSuffixes);  // "3rd"
```

### Pluralisation in Text Scopes

```csharp
public sealed class EnGbDirectoryText(CultureInfo culture) : IDirectoryText
{
    private static readonly PluralForms PersonForms = new()
    {
        Other = "people",
        One   = "person",
    };

    private static readonly OrdinalForms OrdinalSuffixes = new()
    {
        Other = "th",
        One   = "st",
        Two   = "nd",
        Few   = "rd",
    };

    public string PeopleFound(int count) =>
        $"{Plural.Format<OneOtherCardinal>(count, PersonForms, culture)} found";

    public string Position(int rank) =>
        Ordinal.Format<EnglishOrdinal>(rank, OrdinalSuffixes, culture);
}
```

### CLDR Rule Coverage

All rules are verified against [CLDR commit `f7e8edb`](https://github.com/unicode-org/cldr/commit/f7e8edbd1838e518932531fd24d0cd7025d77bf4) and tested with the CLDR `@integer` sample vectors.

#### Cardinal Rules

| Rule struct | Languages | Categories |
|---|---|---|
| `OneOtherCardinal` | en, de, nl, sv, it, es, pt-PT | one, other |
| `FrenchCardinal` | fr, pt (BR) | one (0 or 1), other |
| `RomanianCardinal` | ro | one, few, other |
| `EastSlavicCardinal` | ru, uk | one, few, many, other |
| `PolishCardinal` | pl | one, few, many |
| `CzechSlovakCardinal` | cs, sk | one, few, other |
| `ArabicCardinal` | ar | zero, one, two, few, many, other |
| `HebrewCardinal` | he | one, two, other |
| `WelshCardinal` | cy | zero, one, two, few, many, other |
| `OtherOnlyCardinal` | zh, ja, ko | other |

#### Ordinal Rules

| Rule struct | Languages | Categories |
|---|---|---|
| `OtherOnlyOrdinal` | de, nl, da, nb, ar, cs, he, pl, pt, ru, sk, zh, ja, ko, ... | other |
| `EnglishOrdinal` | en | one, two, few, other |
| `SwedishOrdinal` | sv | one, other |
| `SpanishOrdinal` | es | one, other |
| `OneOnlyOrdinal` | fr, ro | one, other |
| `ItalianOrdinal` | it | many, other |
| `UkrainianOrdinal` | uk | few, other |
| `WelshOrdinal` | cy | zero, one, two, few, many, other |

#### Language Rules Reference

`LanguageRules` in `Lugha.Rules` documents the correct cardinal/ordinal pairing for each supported language tag:

| Language | Cardinal | Ordinal |
|---|---|---|
| en | `OneOtherCardinal` | `EnglishOrdinal` |
| de | `OneOtherCardinal` | `OtherOnlyOrdinal` |
| nl | `OneOtherCardinal` | `OtherOnlyOrdinal` |
| sv | `OneOtherCardinal` | `SwedishOrdinal` |
| fr | `FrenchCardinal` | `OneOnlyOrdinal` |
| es | `OneOtherCardinal` | `SpanishOrdinal` |
| it | `OneOtherCardinal` | `ItalianOrdinal` |
| pt | `FrenchCardinal` | `OtherOnlyOrdinal` |
| pt-PT | `OneOtherCardinal` | `OtherOnlyOrdinal` |
| ro | `RomanianCardinal` | `OneOnlyOrdinal` |
| ru | `EastSlavicCardinal` | `OtherOnlyOrdinal` |
| uk | `EastSlavicCardinal` | `UkrainianOrdinal` |
| pl | `PolishCardinal` | `OtherOnlyOrdinal` |
| cs | `CzechSlovakCardinal` | `OtherOnlyOrdinal` |
| sk | `CzechSlovakCardinal` | `OtherOnlyOrdinal` |
| ar | `ArabicCardinal` | `OtherOnlyOrdinal` |
| he | `HebrewCardinal` | `OtherOnlyOrdinal` |
| cy | `WelshCardinal` | `WelshOrdinal` |
| zh | `OtherOnlyCardinal` | `OtherOnlyOrdinal` |
| ja | `OtherOnlyCardinal` | `OtherOnlyOrdinal` |
| ko | `OtherOnlyCardinal` | `OtherOnlyOrdinal` |

## Bidirectional Text

For RTL locales, interpolated LTR values (hostnames, numbers, identifiers) must be isolated to prevent visual reordering. The `Bidi` class provides Unicode isolate wrapping per [UAX #9](https://unicode.org/reports/tr9/).

```csharp
// String methods - allocate one string
string isolated = Bidi.IsolateLtr(host);     // U+2066 + host + U+2069
string isolated = Bidi.IsolateRtl(value);    // U+2067 + value + U+2069
string isolated = Bidi.Isolate(value);       // U+2068 + value + U+2069

// In an RTL text scope implementation:
public string Connecting(string host) =>
    $"\u062C\u0627\u0631\u064D \u0627\u0644\u0627\u062A\u0635\u0627\u0644 \u0628\u0640{Bidi.IsolateLtr(host)}\u2026";
```

Zero-allocation span variants return `false` when the destination buffer is too small (requires `value.Length + 2` characters):

```csharp
Span<char> buffer = stackalloc char[64];
bool ok = Bidi.TryIsolateLtr(value, buffer, out int written);
```

Constants `Bidi.Lri`, `Bidi.Rli`, `Bidi.Fsi`, `Bidi.Pdi`, `Bidi.Lrm`, and `Bidi.Rlm` are also available for manual composition.

## Analyser Diagnostics

The package includes Roslyn analysers that enforce text scope correctness at compile time.

| ID | Severity | Description |
|---|---|---|
| LGH001 | Error | Text scope member does not return `string`. |
| LGH002 | Error | Locale implementation does not expose all `ITextScope` interfaces declared on its composite locale interface. |
| LGH003 | Error | Text scope implementation returns `null`, `null!`, `default`, `default!`, `string.Empty`, or `""`. |
| LGH004 | Warning | Parameterised text scope method does not use all parameters in its return expression. |
| LGH005 | Info | Text scope interface has no implementations (opt-in). |
| LGH006 | Info | `PluralForms` sets only Other + One for a language needing more categories (opt-in). |
| LGH007 | Info | Text scope member defined but unreferenced (opt-in). |
| LGH008 | Warning | Text scope implementation body contains side-effecting calls (heuristic). |

## Applied Example

A complete multi-locale example demonstrating English, Arabic, and Ukrainian - including pluralisation, ordinals, bidi isolation, and the cardinal/ordinal rule split.

```csharp
using System.Globalization;
using Lugha;
using Lugha.Rules.Cardinal;
using Lugha.Rules.Ordinal;

// ---- Contracts ----

public interface IConnectionText : ITextScope
{
    string Discovering { get; }
    string Connecting(string host);
    string Connected(string host);
    string Unavailable(string reason);
}

public interface IDirectoryText : ITextScope
{
    string PeopleFound(int count);
    string Position(int rank);
}

public interface IAppLocale : ILocale
{
    IConnectionText Connection { get; }
    IDirectoryText Directory { get; }
}

// ---- English (en-GB) ----

public sealed class EnGbConnectionText : IConnectionText
{
    public string Discovering => "Discovering\u2026";
    public string Connecting(string host) => $"Connecting to {host}\u2026";
    public string Connected(string host) => $"Connected to {host}";
    public string Unavailable(string reason) => $"Disconnected: {reason}";
}

public sealed class EnGbDirectoryText(CultureInfo culture) : IDirectoryText
{
    private static readonly PluralForms PersonForms = new()
    {
        Other = "people",
        One   = "person",
    };

    private static readonly OrdinalForms OrdinalSuffixes = new()
    {
        Other = "th",
        One   = "st",
        Two   = "nd",
        Few   = "rd",
    };

    public string PeopleFound(int count) =>
        $"{Plural.Format<OneOtherCardinal>(count, PersonForms, culture)} found";

    public string Position(int rank) =>
        Ordinal.Format<EnglishOrdinal>(rank, OrdinalSuffixes, culture);
}

public sealed class EnGbLocale
    : IAppLocale, ILocale<OneOtherCardinal, EnglishOrdinal>
{
    private static readonly CultureInfo EnGb =
        CultureInfo.GetCultureInfo("en-GB");

    public CultureInfo Culture => EnGb;
    public IConnectionText Connection { get; } = new EnGbConnectionText();
    public IDirectoryText Directory { get; } = new EnGbDirectoryText(EnGb);
}

// ---- Arabic (ar-SA) ----

public sealed class ArSaConnectionText : IConnectionText
{
    public string Discovering => "\u062C\u0627\u0631\u064D \u0627\u0644\u0627\u0643\u062A\u0634\u0627\u0641\u2026";
    public string Connecting(string host) =>
        $"\u062C\u0627\u0631\u064D \u0627\u0644\u0627\u062A\u0635\u0627\u0644 \u0628\u0640{Bidi.IsolateLtr(host)}\u2026";
    public string Connected(string host) =>
        $"\u0645\u062A\u0635\u0644 \u0628\u0640{Bidi.IsolateLtr(host)}";
    public string Unavailable(string reason) =>
        $"\u063A\u064A\u0631 \u0645\u062A\u0635\u0644: {Bidi.Isolate(reason)}";
}

public sealed class ArSaDirectoryText(CultureInfo culture) : IDirectoryText
{
    private static readonly PluralForms PersonForms = new()
    {
        Other = "\u0623\u0634\u062E\u0627\u0635",
        Zero  = "\u0644\u0627 \u0623\u0634\u062E\u0627\u0635",
        One   = "\u0634\u062E\u0635",
        Two   = "\u0634\u062E\u0635\u0627\u0646",
        Few   = "\u0623\u0634\u062E\u0627\u0635",
        Many  = "\u0634\u062E\u0635\u064B\u0627",
    };

    public string PeopleFound(int count) =>
        $"\u062A\u0645 \u0627\u0644\u0639\u062B\u0648\u0631 \u0639\u0644\u0649 {Plural.Format<ArabicCardinal>(count, PersonForms, culture)}";

    public string Position(int rank) =>
        $"\u0627\u0644\u0645\u0631\u0643\u0632 {rank.ToString(culture)}";
}

public sealed class ArSaLocale
    : IAppLocale, ILocale<ArabicCardinal, OtherOnlyOrdinal>
{
    private static readonly CultureInfo ArSa =
        CultureInfo.GetCultureInfo("ar-SA");

    public CultureInfo Culture => ArSa;
    public IConnectionText Connection { get; } = new ArSaConnectionText();
    public IDirectoryText Directory { get; } = new ArSaDirectoryText(ArSa);
}

// ---- Usage ----

IAppLocale locale = new EnGbLocale();

Console.WriteLine(locale.Connection.Discovering);       // Discovering...
Console.WriteLine(locale.Connection.Connected("srv"));  // Connected to srv
Console.WriteLine(locale.Directory.PeopleFound(1));     // 1 person found
Console.WriteLine(locale.Directory.PeopleFound(3));     // 3 people found
Console.WriteLine(locale.Directory.Position(1));        // 1st
Console.WriteLine(locale.Directory.Position(22));       // 22nd

// Locale-path for generic components:
Console.WriteLine(locale.PluralFormat(5, new PluralForms
{
    Other = "items",
    One   = "item",
})); // 5 items
```

## Allocation Profile

| Access pattern | Allocation |
|---|---|
| Invariant property (`Navigation.Dashboard`) | Zero - compiler-interned literal |
| Parameterised method (`Connecting(host)`) | One string |
| `Plural.Format<TRules>(count, forms, culture)` | One string |
| `Plural.TryFormat<TRules>(...)` | Minimal - `MemoryExtensions.TryWrite` |
| `Plural.Select<TRules>(count, forms)` | Zero - returns existing string from `PluralForms` |
| `ICardinalRules<TSelf>.Cardinal(count)` | Zero - returns enum, static dispatch |
| `ILocale.Cardinal(count)` | Zero - returns enum, virtual dispatch |
| `Bidi.IsolateLtr(value)` | One string |
| `Bidi.TryIsolateLtr(value, span, ...)` | Zero |

## Licence

[Apache License 2.0](../LICENSE)
