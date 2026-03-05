# Lugha.Common

Shared types for the Lugha localisation ecosystem. Published as an independent NuGet package referenced by import libraries.

## API

### `LanguageRules`

Static class that maps BCP 47 language tags to their correct CLDR cardinal and ordinal rule pair. Used by the import code emitter to resolve plural rule types at generation time.

```csharp
RulePair? pair = LanguageRules.Resolve("en");
// pair.Value.Cardinal == "OneOtherCardinal"
// pair.Value.Ordinal  == "EnglishOrdinal"
```

Supports 23 languages: en, de, nl, nb, da, sv, fr, es, it, pt, pt-PT, ca, ro, ru, uk, pl, cs, sk, ar, he, cy, zh, ja, ko. Returns `null` for unrecognised tags.

Region subtags are significant only where they change the rule - `pt` (Brazilian) uses `FrenchCardinal`, while `pt-PT` uses `LatinEuropeanCardinal`.

### `RulePair`

Readonly record struct holding `Cardinal` and `Ordinal` string properties - the unqualified type names of the corresponding rule implementations.

## Design

- **Switch expression** for lookup - no dictionary allocation, no hashing overhead.
- Verified against CLDR commit hashes linked in the XML doc comments.
- No dependency on `Lugha` - this package stands alone so that import generators can reference it without pulling in the full runtime.

## Build

Multi-targets `netstandard2.0` and `net10.0` with `PolySharp` 1.15.0 for C# 14 polyfills on older targets.

```
dotnet build lugha-common
```

## Licence

[Apache License 2.0](../LICENSE)
