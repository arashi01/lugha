// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Globalization;
using Lugha.Rules.Cardinals;
using Lugha.Rules.Ordinals;

namespace Lugha.Samples.WinUI;

/// <summary>Arabic (Saudi Arabia) locale.</summary>
public sealed class ArSaLocale : IAppLocale, ILocale<ArabicCardinal, OtherOnlyOrdinal>
{
  /// <inheritdoc />
  public CultureInfo Culture { get; } = CultureInfo.GetCultureInfo("ar-SA");

  /// <inheritdoc />
  public IConnectionText Connection { get; } = new ArSaConnectionText();

  /// <inheritdoc />
  public INavigationText Navigation { get; } = new ArSaNavigationText();

  /// <inheritdoc />
  public IStatusText Status { get; } = new ArSaStatusText();
}
