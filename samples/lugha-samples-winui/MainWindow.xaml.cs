// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.WinUI;
using Microsoft.UI.Xaml;

namespace Lugha.Samples.WinUI;

/// <summary>
/// Main application window demonstrating locale-aware bindings,
/// pluralisation, and RTL layout.
/// </summary>
public sealed partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();
  }

#pragma warning disable CA1822 // x:Bind targets must be instance members
  /// <summary>The locale host for XAML bindings.</summary>
  public WinUILocaleHost<IAppLocale>? Host => App.Host;

  /// <summary>Sample server name for parameterised text.</summary>
  public string ServerName => "server-1";

  /// <summary>Singular count for plural demonstration.</summary>
  public int SingleUser => 1;

  /// <summary>Plural count for plural demonstration.</summary>
  public int UserCount => 42;
#pragma warning restore CA1822

  private void OnLanguageSelected(object sender, RoutedEventArgs e)
  {
    if (sender is not FrameworkElement { Tag: string languageTag })
    {
      return;
    }

    SwitchLocale(languageTag);
  }

  private static void SwitchLocale(string languageTag)
  {
    if (App.Registry is not { } registry)
    {
      return;
    }

    IAppLocale locale = registry.Resolve(languageTag);

    App.Host?.SetLocale(locale);
    SystemLanguageSync.TryApply(locale);
  }
}
