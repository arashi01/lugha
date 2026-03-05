# Lugha.WinUI

WinUI 3 integration for the Lugha typed localisation ecosystem. Provides a reactive locale host and an immutable locale registry, enabling runtime language switching with full compile-time safety.

## API

### `LocaleHost<TLocale>`

Reactive wrapper that bridges Lugha's pure, immutable locale model with WinUI's binding system. Implements `INotifyPropertyChanged` so that `x:Bind` with `Mode=OneWay` re-evaluates all text bindings when the active locale changes.

```csharp
var host = new LocaleHost<IAppLocale>(new EnGbLocale(), DispatcherQueue.GetForCurrentThread());

// Switch locale from any thread - notification is dispatched to the UI thread.
host.SetLocale(new ArSaLocale());
```

- Thread-safe. `SetLocale` may be called from any thread; `PropertyChanged` is dispatched via `DispatcherQueue`.
- `Current` property exposes the active locale for XAML binding.
- This is the single point of mutable state in the Lugha ecosystem - all text resolution remains pure.

### `LocaleRegistry<TLocale>`

Immutable dictionary mapping BCP 47 language tags to pre-constructed locale instances. Uses `FrozenDictionary` for optimal read performance with case-insensitive lookup.

```csharp
var registry = new LocaleRegistry<IAppLocale>([
    new EnGbLocale(),
    new ArSaLocale(),
]);

if (registry.Resolve("ar-SA") is { } locale)
    host.SetLocale(locale);
```

- `Languages` property exposes the set of registered language tags.
- `Resolve(string language)` returns `null` when the tag is not found - caller decides fallback policy.

## XAML usage

```xml
<Page x:Class="MyApp.Views.MainPage">
    <!-- Invariant text - re-evaluates on locale switch -->
    <TextBlock Text="{x:Bind Host.Current.Navigation.Dashboard, Mode=OneWay}" />

    <!-- Parameterised text via code-behind property -->
    <TextBlock Text="{x:Bind ConnectionStatus, Mode=OneWay}" />
</Page>
```

```csharp
public sealed partial class MainPage : Page, INotifyPropertyChanged
{
    private readonly LocaleHost<IAppLocale> Host;

    public MainPage(LocaleHost<IAppLocale> host)
    {
        Host = host;
        Host.PropertyChanged += (_, _) =>
            OnPropertyChanged(nameof(ConnectionStatus));
        InitializeComponent();
    }

    public string ConnectionStatus =>
        Host.Current.Connection.Connected("server-1");
}
```

## Dependencies

- `Lugha` - core runtime library.
- `Microsoft.WindowsAppSDK` 1.8 or later.

## Build

Targets `net10.0-windows10.0.19041.0`.

```
dotnet build integration/lugha-winui
```

## Licence

[Apache License 2.0](../../LICENSE)
