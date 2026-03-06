// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Microsoft.UI.Xaml;
using Windows.Globalization;

namespace Lugha.WinUI;

/// <summary>
/// Synchronises platform language settings with a Lugha locale.
/// </summary>
/// <remarks>
/// <para>This class is deliberately separate from <see cref="LocaleHost{TLocale}"/>
/// because <see cref="ApplicationLanguages.PrimaryLanguageOverride"/> is a global,
/// persistent side effect - it is stored in the application's local settings and
/// survives application restarts. Callers must opt in explicitly.</para>
/// </remarks>
public static class SystemLanguageSync
{
  /// <summary>
  /// Sets <see cref="ApplicationLanguages.PrimaryLanguageOverride"/>
  /// to the culture name of the specified locale.
  /// </summary>
  /// <param name="locale">The locale whose culture name to apply.</param>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="locale"/> is <see langword="null"/>.
  /// </exception>
  public static void Apply(ILocale locale)
  {
    ArgumentNullException.ThrowIfNull(locale);
    ApplicationLanguages.PrimaryLanguageOverride = locale.Culture.Name;
  }

  /// <summary>
  /// Sets <see cref="ApplicationLanguages.PrimaryLanguageOverride"/> and updates
  /// <paramref name="rootElement"/>'s <see cref="FrameworkElement.FlowDirection"/>
  /// for right-to-left layout support.
  /// </summary>
  /// <param name="locale">The locale whose culture name and flow direction to apply.</param>
  /// <param name="rootElement">
  /// The root element to update. Typically the window's content frame.
  /// </param>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="locale"/> or <paramref name="rootElement"/> is <see langword="null"/>.
  /// </exception>
  public static void Apply(ILocale locale, FrameworkElement rootElement)
  {
    ArgumentNullException.ThrowIfNull(locale);
    ArgumentNullException.ThrowIfNull(rootElement);
    ApplicationLanguages.PrimaryLanguageOverride = locale.Culture.Name;
    rootElement.FlowDirection = locale.FlowDirection();
  }
}
