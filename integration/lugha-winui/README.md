# Lugha.WinUI

WinUI 3 integration for the Lugha typed localisation ecosystem. Provides a reactive locale host and an immutable locale registry, enabling runtime language switching with full compile-time safety.

```sh
dotnet add package Lugha.WinUI
```

Requires .NET 10 and Windows App SDK 1.8 or later.

## Quick Start

### 1. Create a locale registry

`LocaleRegistry<TLocale>` is an immutable, thread-safe dictionary mapping BCP 47 language tags to pre-constructed locale instances. It uses `FrozenDictionary` with case-insensitive lookup. Create it once at application startup.

```csharp
var registry = new LocaleRegistry<IAppLocale>([
    new EnGbLocale(),
    new ArSaLocale(),
    new EsEsLocale(),
]);
```

Each locale's `Culture.Name` is used as the lookup key.

### 2. Create a locale host

`LocaleHost<TLocale>` is the reactive bridge between Lugha's immutable locale model and WinUI's binding system. It implements `INotifyPropertyChanged` so that `x:Bind` with `Mode=OneWay` re-evaluates all text bindings when the active locale changes.

```csharp
var host = new LocaleHost<IAppLocale>(
    registry.Resolve("en-GB")!,
    DispatcherQueue.GetForCurrentThread());
```

The host is the single point of mutable state in the Lugha ecosystem - all text resolution remains pure. Only the selection of which locale is active is mutable.

### 3. Store as a singleton

Both `LocaleRegistry` and `LocaleHost` should be created once and shared across the application. In `App.xaml.cs`:

```csharp
public sealed partial class App : Application
{
    public static LocaleRegistry<IAppLocale> Registry { get; private set; } = null!;
    public static LocaleHost<IAppLocale> Host { get; private set; } = null!;

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Registry = new LocaleRegistry<IAppLocale>([
            new EnGbLocale(),
            new ArSaLocale(),
            new EsEsLocale(),
        ]);

        Host = new LocaleHost<IAppLocale>(
            Registry.Resolve("en-GB")!,
            DispatcherQueue.GetForCurrentThread());

        // ...
    }
}
```

### 4. Bind in XAML

Invariant text (labels, titles) can be bound directly to the locale host's `Current` property:

```xml
<Page x:Class="MyApp.Views.MainPage">
    <StackPanel>
        <!-- Invariant text - re-evaluates on locale switch -->
        <TextBlock Text="{x:Bind Host.Current.Navigation.Dashboard, Mode=OneWay}" />
        <TextBlock Text="{x:Bind Host.Current.Navigation.Settings, Mode=OneWay}" />

        <!-- Parameterised text via code-behind property -->
        <TextBlock Text="{x:Bind ConnectionStatus, Mode=OneWay}" />
    </StackPanel>
</Page>
```

Parameterised text (methods with arguments) cannot be called directly from `x:Bind`. Expose it as a code-behind property and re-evaluate when the locale changes:

```csharp
public sealed partial class MainPage : Page, INotifyPropertyChanged
{
    private LocaleHost<IAppLocale> Host => App.Host;

    public MainPage()
    {
        InitializeComponent();
        Host.PropertyChanged += (_, _) => OnPropertyChanged(nameof(ConnectionStatus));
    }

    public string ConnectionStatus =>
        Host.Current.Connection.Connected("server-1");

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### 5. Switch locale at runtime

Use the registry to resolve a locale by tag, then set it on the host. This can be called from any thread - the property change notification is dispatched to the UI thread via `DispatcherQueue`.

```csharp
private void OnLanguageSelected(string languageTag)
{
    if (App.Registry.Resolve(languageTag) is { } locale)
    {
        App.Host.SetLocale(locale);
    }
    // Resolve returns null for unregistered tags - caller decides fallback.
}
```

## API Reference

### `LocaleRegistry<TLocale>`

| Member | Description |
|---|---|
| `LocaleRegistry(IEnumerable<TLocale> locales)` | Creates a registry from locale instances, keyed by `Culture.Name`. |
| `IEnumerable<string> Languages` | The set of registered language tags. |
| `TLocale? Resolve(string language)` | Returns the locale for the tag, or `null` if not found. |

### `LocaleHost<TLocale>`

| Member | Description |
|---|---|
| `LocaleHost(TLocale initial, DispatcherQueue dispatcher)` | Creates a host with the initial locale and UI dispatcher. |
| `TLocale Current` | The active locale. Bind to this in XAML. |
| `void SetLocale(TLocale locale)` | Switches the active locale. Thread-safe. |
| `event PropertyChangedEventHandler? PropertyChanged` | Raised when `Current` changes. Always fires on the UI thread. |

## Thread Safety

- `LocaleRegistry<TLocale>` is immutable after construction. Safe to read from any thread.
- `LocaleHost<TLocale>.SetLocale` may be called from any thread. If called off the UI thread, the property update is dispatched via `DispatcherQueue.TryEnqueue`. The `PropertyChanged` event always fires on the UI thread.
- Locale instances themselves are immutable and pure - sharing them across threads is safe.

## Dependencies

- `Lugha` - core runtime library.
- `Microsoft.WindowsAppSDK` 1.8 or later.

## Licence

[Apache License 2.0](../../LICENSE)
