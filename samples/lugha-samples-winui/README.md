# Lugha.Samples.WinUI

A packaged WinUI 3 (MSIX) application demonstrating Lugha's typed localisation with runtime language switching, CLDR pluralisation, RTL layout, and Gettext `.po` source generation.

## What it demonstrates

- **Gettext import** - four `.po` files under `Translations/` are converted to typed text scopes at compile time by `Lugha.Import.Gettext`.
- **Composite locale** - `IAppLocale` composes three generated scopes: `IConnectionText`, `INavigationText`, and `IStatusText`.
- **Four locales** - English (`en-GB`), Arabic (`ar-SA`), Spanish (`es-ES`), and Simplified Chinese (`zh-Hans`), each with the correct CLDR cardinal rules.
- **Runtime language switching** - `LocaleRegistry` + `LocaleHost` from `Lugha.WinUI` provide reactive `x:Bind` bindings that update all text when the locale changes.
- **RTL support** - `SystemLanguageSync.Apply` sets `PrimaryLanguageOverride` and flips `FlowDirection` for Arabic.
- **Parameterised text** - `Connection.Connected(host)` demonstrates `x:Bind` path-to-function syntax.
- **Pluralisation** - `Status.OnlineUsers` is a plural scope with per-locale CLDR forms (one/other for English, zero/one/two/few/many/other for Arabic).

## Project structure

```
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
  MainWindow.xaml.cs      Language switch handlers
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

42 tests cover all four locales across every text scope, including plural correctness for English and Arabic.

## Licence

[Apache License 2.0](../../LICENSE)
