// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Microsoft.UI.Xaml;
using Windows.Globalization;

namespace Lugha.WinUI;

/// <summary>
/// Synchronises platform language settings with a Lugha locale.
/// </summary>
/// <remarks>
/// <see cref="ApplicationLanguages.PrimaryLanguageOverride"/> is a global,
/// persistent side effect stored in the application's local settings.
/// Callers must opt in explicitly.
/// </remarks>
public static class SystemLanguageSync
{
  /// <summary>
  /// Attempts to set <see cref="ApplicationLanguages.PrimaryLanguageOverride"/>
  /// to the culture name of <paramref name="locale"/>.
  /// </summary>
  /// <returns>
  /// <see langword="true"/> if the override was set;
  /// <see langword="false"/> if the application is unpackaged and the
  /// override is not supported.
  /// </returns>
  public static bool TryApply(ILocale locale)
  {
    try
    {
      ApplicationLanguages.PrimaryLanguageOverride = locale.Culture.Name;
      return true;
    }
    catch (InvalidOperationException)
    {
      return false;
    }
  }

  /// <summary>
  /// Updates <paramref name="rootElement"/>'s <see cref="FrameworkElement.FlowDirection"/>
  /// and attempts to set <see cref="ApplicationLanguages.PrimaryLanguageOverride"/>.
  /// </summary>
  /// <remarks>
  /// <see cref="FrameworkElement.FlowDirection"/> is always updated.
  /// <see cref="ApplicationLanguages.PrimaryLanguageOverride"/> is set only
  /// in packaged (MSIX) applications; in unpackaged applications, the call
  /// is silently skipped.
  /// </remarks>
  /// <returns>
  /// <see langword="true"/> if the language override was set;
  /// <see langword="false"/> if only the flow direction was updated.
  /// </returns>
  public static bool TryApply(ILocale locale, FrameworkElement rootElement)
  {
    rootElement.FlowDirection = locale.FlowDirection();
    return TryApply(locale);
  }
}
