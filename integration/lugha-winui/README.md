# Lugha.WinUI

WinUI 3 integration for the Lugha typed localisation ecosystem. Provides a reactive locale host, an immutable locale registry with BCP 47 fallback, system language synchronisation, and RTL flow direction support - enabling runtime language switching with full compile-time safety.

```sh
dotnet add package Lugha.WinUI
```

Requires .NET 10 and Windows App SDK 1.8 or later.

## Quick Start

### 1. Create a locale registry

`LocaleRegistry<TLocale>` is an immutable, thread-safe dictionary mapping BCP 47 language tags to pre-constructed locale instances. It uses `FrozenDictionary` with case-insensitive lookup. Create it once at application startup.

```csharp
if (LocaleRegistry<IAppLocale>.Create(
        new EnGbLocale(), new ArSaLocale(), new EsEsLocale())
    is not Result<LocaleRegistry<IAppLocale>, DuplicateLanguageTag>.Ok(var registry))
{
    return; // duplicate language tag
}
```

Each locale's `Culture.Name` is used as the lookup key. `Create` returns a `Result` — `Ok` with the registry, or `Err` with the first duplicate tag found.

### 2. Create a locale host

`WinUILocaleHost<TLocale>` extends the core `LocaleHost<TLocale>` with a reactive `FlowDirection` property. It implements `INotifyPropertyChanged` so that `x:Bind` with `Mode=OneWay` re-evaluates all text bindings and layout direction when the active locale changes.

```csharp
var host = LocaleHostFactory.Create(registry.Default, MainWindow.DispatcherQueue);
```

`LocaleHostFactory.Create` returns a `WinUILocaleHost` that wraps the `DispatcherQueue` dispatch logic. The host is the single point of mutable state in the Lugha ecosystem — all text resolution remains pure. Only the selection of which locale is active is mutable.

### 3. Store as a singleton

Both `LocaleRegistry` and `LocaleHost` should be created once and shared across the application. In `App.xaml.cs`:

```csharp
public sealed partial class App : Application
{
    public static LocaleRegistry<IAppLocale>? Registry { get; private set; }
    public static WinUILocaleHost<IAppLocale>? Host { get; private set; }
    public static Window? MainWindow { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        EnGbLocale defaultLocale = new();

        if (LocaleRegistry<IAppLocale>.Create(
                defaultLocale, new ArSaLocale(), new EsEsLocale())
            is not Result<LocaleRegistry<IAppLocale>, DuplicateLanguageTag>.Ok(var registry))
        {
            return;
        }

        Registry = registry;
        MainWindow = new MainWindow();
        Host = LocaleHostFactory.Create(registry.Default, MainWindow.DispatcherQueue);
        MainWindow.Activate();
    }
}
```

### 4. Bind in XAML

Invariant text (labels, titles) binds directly to the locale host's `Current` property. RTL layout binds to `Host.FlowDirection` — a reactive property on `WinUILocaleHost` that updates automatically when the locale changes:

```xml
<Page x:Class="MyApp.Views.MainPage">
    <Grid FlowDirection="{x:Bind Host.FlowDirection, Mode=OneWay}">
        <!-- Invariant text - re-evaluates on locale switch -->
        <TextBlock Text="{x:Bind Host.Current.Navigation.Dashboard, Mode=OneWay}" />
        <TextBlock Text="{x:Bind Host.Current.Navigation.Settings, Mode=OneWay}" />
    </Grid>
</Page>
```

Parameterised text (methods with arguments) can use `x:Bind` path-to-function syntax. `x:Bind` re-evaluates the entire path when `Host.PropertyChanged` fires for `Current`:

```xml
<TextBlock Text="{x:Bind Host.Current.Connection.Connected(ViewModel.ServerName), Mode=OneWay}" />
```

This eliminates the need for code-behind bridge properties for parameterised strings. The `x:Bind` compiler generates the method call and re-evaluates it whenever either `Host.Current` or `ViewModel.ServerName` changes.

### 5. Switch locale at runtime

Use the registry to resolve a locale by tag, then set it on the host. This can be called from any thread - the property change notification is dispatched to the UI thread via `DispatcherQueue`.

```csharp
private void OnLanguageSelected(string languageTag)
{
    if (App.Registry is not { } registry) return;
    IAppLocale locale = registry.Resolve(languageTag);
    App.Host?.SetLocale(locale);
    SystemLanguageSync.TryApply(locale);
}
```

## BCP 47 Subtag Fallback

`Resolve(string)` strips subtags right-to-left until a match is found. This enables registering a base locale (e.g. `es`) and matching regional variants (e.g. `es-419`, `es-MX`) without registering each explicitly:

```csharp
// Assuming registry created with EsLocale (es) and EsEsLocale (es-ES):
registry.Resolve("es-ES");  // exact match -> EsEsLocale
registry.Resolve("es-419"); // strips to es -> EsLocale
registry.Resolve("es-MX");  // strips to es -> EsLocale
registry.Resolve("fr-FR");  // no match -> returns Default
```

`Resolve` is a total function — it always returns a locale, falling back to `Default` when no match is found. Use `TryResolve` if you need `null` for unregistered tags.

Exact matches use the original `string` with no allocation. The fallback loop allocates one `string` per subtag stripped - acceptable for this cold path (called once per locale switch).

## System Language and RTL

### `SystemLanguageSync`

`SystemLanguageSync` synchronises the Windows App SDK language setting with the active locale. It is deliberately separate from `LocaleHost` because `ApplicationLanguages.PrimaryLanguageOverride` is a global, persistent side effect that survives application restarts.

> **Packaged apps only.** `ApplicationLanguages.PrimaryLanguageOverride` requires a packaged (MSIX) application identity. In unpackaged apps (`WindowsPackageType=None`), `TryApply` returns `false` and the override is silently skipped. Use `FlowDirection()` directly for RTL support in unpackaged apps.

```csharp
// Set the platform language override only (returns false in unpackaged apps)
SystemLanguageSync.TryApply(locale);

// Set the platform language override and update RTL flow direction
SystemLanguageSync.TryApply(locale, rootElement);
```

### `FlowDirection()` extension

The `FlowDirection()` extension method on `ILocale` returns the WinUI `FlowDirection` enum value based on `CultureInfo.TextInfo.IsRightToLeft`:

```csharp
FlowDirection direction = locale.FlowDirection();
// en-GB -> LeftToRight
// ar-SA -> RightToLeft
```

Use this to update layout when switching locales:

```csharp
rootElement.FlowDirection = locale.FlowDirection();
```

### Full locale switch

Combine registry resolution, host update, and system sync for a complete locale switch:

```csharp
private void OnLanguageSelected(string languageTag)
{
    if (App.Registry is not { } registry) return;
    IAppLocale locale = registry.Resolve(languageTag);
    App.Host?.SetLocale(locale);
    SystemLanguageSync.TryApply(locale);
}
```

## Multi-Window Locale

Each `LocaleHost<TLocale>` is bound to the `DispatcherQueue` of the thread that owns its UI elements. For multi-window applications, create a separate `LocaleHost` per window, sharing the same `LocaleRegistry`:

```csharp
// Main window
var mainHost = LocaleHostFactory.Create(locale, mainWindow.DispatcherQueue);

// Secondary window (different thread)
var secondaryHost = LocaleHostFactory.Create(locale, secondaryWindow.DispatcherQueue);
```

Both hosts share the immutable registry and locale instances. Switching locale on one host does not affect the other - coordinate explicitly if desired.

## API Reference

### `LocaleRegistry<TLocale>`

| Member | Description |
|---|---|
| `static Result<..., DuplicateLanguageTag> Create(TLocale default, params ReadOnlySpan<TLocale>)` | Factory. Returns `Ok` with the registry or `Err` with the duplicate tag. |
| `TLocale Default` | The default locale, guaranteed registered. |
| `int Count` | Number of registered locales. |
| `IEnumerable<string> Languages` | The set of registered language tags. |
| `IEnumerable<TLocale> Locales` | All registered locale instances. |
| `TLocale Resolve(string language)` | BCP 47 lookup with subtag fallback. Returns `Default` if no match. |
| `TLocale? TryResolve(string language)` | BCP 47 lookup with subtag fallback. Returns `null` if no match. |
| `bool Contains(string language)` | Whether a locale matching the tag is registered. |

### `LocaleHost<TLocale>` (core Lugha)

| Member | Description |
|---|---|
| `LocaleHost(TLocale initial, Action<Action> dispatch)` | Creates a host with the initial locale and a dispatch delegate. |
| `TLocale Current` | The active locale. Bind to this in XAML. |
| `void SetLocale(TLocale locale)` | Switches the active locale. Thread-safe. |
| `event PropertyChangedEventHandler? PropertyChanged` | Raised when `Current` changes. Dispatched via the delegate. |
| `protected virtual void OnCurrentChanged()` | Override in subclasses to raise `PropertyChanged` for derived properties. |
| `protected void OnPropertyChanged(PropertyChangedEventArgs)` | Raises `PropertyChanged`. |

### `WinUILocaleHost<TLocale>` (Lugha.WinUI)

| Member | Description |
|---|---|
| `FlowDirection FlowDirection` | Reactive WinUI `FlowDirection` derived from the active locale. Re-evaluated when `Current` changes. |

### `LocaleHostFactory` (Lugha.WinUI)

| Member | Description |
|---|---|
| `static WinUILocaleHost<TLocale> Create(TLocale initial, DispatcherQueue dispatcher)` | Creates a `WinUILocaleHost` that dispatches via `DispatcherQueue`. |

### `SystemLanguageSync`

| Member | Description |
|---|---|
| `static bool TryApply(ILocale locale)` | Sets `PrimaryLanguageOverride`. Returns `false` in unpackaged apps. |
| `static bool TryApply(ILocale locale, FrameworkElement rootElement)` | Sets `PrimaryLanguageOverride` and updates `FlowDirection`. Returns `false` if override was not set. |

### `LocaleExtensions`

| Member | Description |
|---|---|
| `static FlowDirection FlowDirection(this ILocale locale)` | Returns `LeftToRight` or `RightToLeft` based on the locale's text info. |

## Thread Safety

- `LocaleRegistry<TLocale>` is immutable after construction. Safe to read from any thread.
- `LocaleHost<TLocale>.SetLocale` may be called from any thread. If called off the UI thread, the property update is dispatched via `DispatcherQueue.TryEnqueue`. The `PropertyChanged` event always fires on the UI thread.
- Locale instances themselves are immutable and pure - sharing them across threads is safe.
- `SystemLanguageSync.TryApply` must be called from the UI thread (it accesses `FrameworkElement` properties).

## Sample Application

See [Lugha.Samples.WinUI](../../samples/lugha-samples-winui/README.md) for a complete packaged WinUI 3 app demonstrating registry setup, locale switching, RTL layout, `x:Bind` bindings, and Gettext source generation with four locales.

## Dependencies

- `Lugha` - core runtime library.
- `Microsoft.WindowsAppSDK` 1.8 or later.

## Licence

[Apache License 2.0](../../LICENSE)
