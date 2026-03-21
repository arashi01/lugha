// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Microsoft.UI.Xaml;

namespace Lugha.WinUI;

/// <summary>
/// WinUI-specific extension methods for <see cref="ILocale"/>.
/// </summary>
public static class LocaleExtensions
{
  /// <summary>
  /// Returns the <see cref="Microsoft.UI.Xaml.FlowDirection"/> for this
  /// locale's writing system.
  /// </summary>
  public static FlowDirection FlowDirection(this ILocale locale) =>
      locale.IsRightToLeft
          ? Microsoft.UI.Xaml.FlowDirection.RightToLeft
          : Microsoft.UI.Xaml.FlowDirection.LeftToRight;
}
