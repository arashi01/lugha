// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.WinUI;
using Microsoft.UI.Xaml;

namespace Lugha.Samples.WinUI;

/// <summary>
/// Main application window demonstrating locale-aware bindings.
/// </summary>
public sealed partial class MainWindow : Window
{
  /// <summary>
  /// Initialises the main window.
  /// </summary>
  public MainWindow()
  {
    InitializeComponent();
  }

#pragma warning disable CA1822 // x:Bind targets must be instance members
  /// <summary>The locale host for XAML bindings.</summary>
  public LocaleHost<IAppLocale>? Host => App.Host;

  /// <summary>Sample server name for parameterised text demonstration.</summary>
  public string ServerName => "server-1";
#pragma warning restore CA1822

  private void OnEnglishClick(object sender, RoutedEventArgs e) =>
      SwitchLocale("en-GB");

  private void OnArabicClick(object sender, RoutedEventArgs e) =>
      SwitchLocale("ar-SA");

  private void OnSpanishClick(object sender, RoutedEventArgs e) =>
      SwitchLocale("es-ES");

  private void OnMandarinClick(object sender, RoutedEventArgs e) =>
      SwitchLocale("zh-Hans");

  private void SwitchLocale(string languageTag)
  {
    if (App.Registry is not { } registry)
    {
      return;
    }

    if (registry.Resolve(languageTag) is not { } locale)
    {
      return;
    }

    if (App.Host is { } host)
    {
      host.SetLocale(locale);
    }

    if (Content is FrameworkElement root)
    {
      SystemLanguageSync.Apply(locale, root);
    }
  }
}
