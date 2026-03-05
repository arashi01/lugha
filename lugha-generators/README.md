# Lugha.Generators

Roslyn incremental source generators that provide compile-time locale verification and metadata emission. Packed into the `Lugha` NuGet package - not published independently.

## Generators

### LGH002 - `LocaleCompletenessGenerator`

**Severity:** Error

Every concrete class implementing `ILocale` must provide all `ITextScope` properties declared on its composite locale interface. The generator inspects each locale class, finds the composite locale interface (the `ILocale`-derived interface), and uses `FindImplementationForInterfaceMember` to verify that every `ITextScope`-typed property has a concrete implementation. Missing scope properties produce an `LGH002` error diagnostic.

```
Locale 'EnGbLocale' does not implement text scope 'IDirectoryText' required by 'IAppLocale'
```

### `LocaleManifestGenerator`

Emits a `partial class LocaleManifest` providing compile-time metadata about the assembly's text scopes and locale implementations:

```csharp
public static partial class LocaleManifest
{
    public static ReadOnlySpan<string> Scopes => ["IConnectionText", "INavigationText"];
    public static ReadOnlySpan<string> Locales => ["EnGbLocale", "ArSaLocale"];
    public static int MemberCount<TScope>() where TScope : Lugha.ITextScope => ...;
}
```

- `Scopes` - names of all `ITextScope`-derived interfaces in the assembly.
- `Locales` - names of all concrete `ILocale` classes in the assembly.
- `MemberCount<TScope>()` - returns the number of declared members (properties and methods) for a given scope type. Returns `0` for unrecognised types.

The manifest is emitted as `LocaleManifest.g.cs` and is only generated when at least one scope or locale exists.

## Architecture

Both generators share a common `GeneratorTextScopeHelper` that resolves `ITextScope`-derived interfaces and `ILocale` implementations via `ToDisplayString()` comparison against fully-qualified metadata names.

All generators implement `IIncrementalGenerator` for optimal IDE performance.

## Build

Targets `netstandard2.0` with `Microsoft.CodeAnalysis.CSharp` 5.0.0 and `PolySharp` 1.15.0 for C# 10+ polyfills.

```
dotnet build lugha-generators
```

## Licence

[Apache License 2.0](../LICENSE)
