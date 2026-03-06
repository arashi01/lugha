// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Globalization;
using Lugha.Rules.Cardinals;
using Lugha.Rules.Ordinals;

namespace Lugha.Samples.WinUI;

/// <summary>English (United Kingdom) locale.</summary>
public sealed class EnGbLocale : IAppLocale, ILocale<OneOtherCardinal, EnglishOrdinal>
{
  /// <inheritdoc />
  public CultureInfo Culture { get; } = CultureInfo.GetCultureInfo("en-GB");

  /// <inheritdoc />
  public IConnectionText Connection { get; } = new EnGbConnectionText();

  /// <inheritdoc />
  public INavigationText Navigation { get; } = new EnGbNavigationText();

  /// <inheritdoc />
  public IStatusText Status { get; } = new EnGbStatusText();
}
