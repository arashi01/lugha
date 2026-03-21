# Lugha.Samples.WinUI

A packaged WinUI 3 (MSIX) application demonstrating Lugha's typed localisation with runtime language switching, CLDR pluralisation, RTL layout, and Gettext `.po` source generation.

## What it demonstrates

- **Gettext import** — four `.po` files under `Translations/` are converted to typed text scopes at compile time by `Lugha.Import.Gettext`.
- **Composite locale** — `IAppLocale` composes three generated scopes: `IConnectionText`, `INavigationText`, and `IStatusText`.
- **Four locales** — English (`en-GB`), Arabic (`ar-SA`), Spanish (`es-ES`), and Simplified Chinese (`zh-Hans`), each with the correct CLDR cardinal rules.
- **Result-based registry** — `LocaleRegistry<IAppLocale>.Create` returns `Result<..., DuplicateLanguageTag>`, pattern-matched in `App.OnLaunched`.
- **Total resolution** — `registry.Resolve(tag)` always returns a locale (falls back to `Default`), eliminating null checks at the call site.
- **Reactive bindings** — `LocaleHostFactory.Create` returns a `WinUILocaleHost` that dispatches `PropertyChanged` via `DispatcherQueue`, driving `x:Bind Mode=OneWay` re-evaluation for both text and layout direction.
- **Reactive RTL** — `Host.FlowDirection` is a reactive property on `WinUILocaleHost`, bound directly via `x:Bind` — no imperative code-behind needed. `SystemLanguageSync.TryApply` sets `PrimaryLanguageOverride` for packaged apps.
- **Parameterised text** — `Connection.Connected(host)` demonstrates `x:Bind` path-to-function syntax.
- **Pluralisation** — `Status.OnlineUsers(count)` shows CLDR-aware plural forms in the UI (one/other for English, six forms for Arabic).
- **Data-driven language switcher** — buttons carry `Tag` attributes with BCP 47 tags, handled by a single `OnLanguageSelected` method.

## Project structure

```text
samples/lugha-samples-winui/
  Translations/           .po files (one per locale)
    en-GB.po
    ar-SA.po
    es-ES.po
    zh-Hans.po
  IAppLocale.cs           Composite locale interface
  EnGbLocale.cs           en-GB locale implementation
  ArSaLocale.cs           ar-SA locale implementation
  EsEsLocale.cs           es-ES locale implementation
  ZhHansLocale.cs         zh-Hans locale implementation
  App.xaml.cs             Registry + host initialisation
  MainWindow.xaml         x:Bind bindings to locale host
  MainWindow.xaml.cs      Language switch + RTL handlers
  Package.appxmanifest    MSIX identity and capabilities
```

## Building and running

The sample targets x64 only and requires a packaged (MSIX) deployment for `SystemLanguageSync`.

```sh
dotnet build samples/lugha-samples-winui
```

To run from Visual Studio, select the **Lugha.Samples.WinUI (Package)** launch profile and press F5.

## Tests

The companion test project (`lugha-samples-winui-tests`) verifies the generated translation scopes without launching the application:

```sh
dotnet test samples/lugha-samples-winui-tests
```

## Multi-window applications

This example binds `FlowDirection` reactively via `Host.FlowDirection` on `WinUILocaleHost`. Each window creates its own host sharing the same registry. `SystemLanguageSync.TryApply(locale)` sets the global `PrimaryLanguageOverride` once per locale switch:

```csharp
// Per window — each window gets its own host (different DispatcherQueue)
var host = LocaleHostFactory.Create(locale, window.DispatcherQueue);
// Bind FlowDirection="{x:Bind Host.FlowDirection, Mode=OneWay}" in each window's XAML

// Once globally — set PrimaryLanguageOverride for the process
SystemLanguageSync.TryApply(locale);
```

## Licence

[Apache License 2.0](../../LICENSE)
