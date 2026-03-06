// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Globalization;
using Lugha.Rules.Cardinals;
using Lugha.Rules.Ordinals;

namespace Lugha.Samples.WinUI;

/// <summary>Simplified Chinese locale.</summary>
public sealed class ZhHansLocale : IAppLocale, ILocale<OtherOnlyCardinal, OtherOnlyOrdinal>
{
  /// <inheritdoc />
  public CultureInfo Culture { get; } = CultureInfo.GetCultureInfo("zh-Hans");

  /// <inheritdoc />
  public IConnectionText Connection { get; } = new ZhHansConnectionText();

  /// <inheritdoc />
  public INavigationText Navigation { get; } = new ZhHansNavigationText();

  /// <inheritdoc />
  public IStatusText Status { get; } = new ZhHansStatusText();
}
