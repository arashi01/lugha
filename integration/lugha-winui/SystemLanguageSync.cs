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
  /// <exception cref="InvalidOperationException">
  /// The application does not have a package identity.
  /// <see cref="ApplicationLanguages.PrimaryLanguageOverride"/>
  /// is only supported in packaged (MSIX) applications.
  /// </exception>
  public static void Apply(ILocale locale)
  {
    ArgumentNullException.ThrowIfNull(locale);
    ApplicationLanguages.PrimaryLanguageOverride = locale.Culture.Name;
  }

  /// <summary>
  /// Updates <paramref name="rootElement"/>'s <see cref="FrameworkElement.FlowDirection"/>
  /// for right-to-left layout support and attempts to set
  /// <see cref="ApplicationLanguages.PrimaryLanguageOverride"/>.
  /// </summary>
  /// <remarks>
  /// <para><see cref="FrameworkElement.FlowDirection"/> is always updated, regardless
  /// of whether the application is packaged or unpackaged.</para>
  /// <para><see cref="ApplicationLanguages.PrimaryLanguageOverride"/> is updated only
  /// in packaged (MSIX) applications. In unpackaged applications the call is silently
  /// skipped - callers still receive the <see cref="FrameworkElement.FlowDirection"/>
  /// update.</para>
  /// <para>The single-parameter <see cref="Apply(ILocale)"/> overload does not catch
  /// the exception and will throw <see cref="InvalidOperationException"/> in unpackaged
  /// applications.</para>
  /// </remarks>
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
    rootElement.FlowDirection = locale.FlowDirection();
    try
    {
      ApplicationLanguages.PrimaryLanguageOverride = locale.Culture.Name;
    }
    catch (InvalidOperationException)
    {
      // PrimaryLanguageOverride requires a packaged application identity.
      // Unpackaged apps receive the FlowDirection update above but cannot
      // set the system language override. This is silently ignored.
    }
  }
}
