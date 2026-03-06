// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Microsoft.UI.Xaml;

namespace Lugha.WinUI;

/// <summary>
/// Extension methods for <see cref="ILocale"/> in WinUI 3 contexts.
/// </summary>
public static class LocaleExtensions
{
  /// <summary>
  /// Returns the <see cref="Microsoft.UI.Xaml.FlowDirection"/> for this locale's
  /// writing system, derived from
  /// <see cref="System.Globalization.TextInfo.IsRightToLeft"/>.
  /// </summary>
  /// <param name="locale">The locale to query.</param>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="locale"/> is <see langword="null"/>.
  /// </exception>
  public static FlowDirection FlowDirection(this ILocale locale)
  {
    ArgumentNullException.ThrowIfNull(locale);
    return locale.Culture.TextInfo.IsRightToLeft
        ? Microsoft.UI.Xaml.FlowDirection.RightToLeft
        : Microsoft.UI.Xaml.FlowDirection.LeftToRight;
  }
}
