// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.WinUI;
using Microsoft.UI.Xaml;

namespace Lugha.Samples.WinUI;

/// <summary>
/// Application entry point. Initialises the locale registry and host.
/// </summary>
public sealed partial class App : Application
{
  /// <summary>The locale registry for the application.</summary>
  public static LocaleRegistry<IAppLocale>? Registry { get; private set; }

  /// <summary>The reactive locale host for XAML bindings.</summary>
  public static LocaleHost<IAppLocale>? Host { get; private set; }

  /// <summary>The main application window.</summary>
  public static Window? MainWindow { get; private set; }

  /// <summary>
  /// Initialises the application.
  /// </summary>
  public App()
  {
    InitializeComponent();
  }

  /// <inheritdoc />
  protected override void OnLaunched(LaunchActivatedEventArgs args)
  {
    EnGbLocale defaultLocale = new();
    Registry = new LocaleRegistry<IAppLocale>([defaultLocale, new ArSaLocale(), new EsEsLocale(), new ZhHansLocale()]);
    MainWindow = new MainWindow();
    Host = new LocaleHost<IAppLocale>(Registry.Resolve("en-GB", defaultLocale), MainWindow.DispatcherQueue);
    MainWindow.Activate();
  }
}
