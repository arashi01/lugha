// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Globalization;
using Lugha.Rules.Cardinals;
using Lugha.Rules.Ordinals;

namespace Lugha.Tests;

/// <summary>
/// Minimal locale for testing locale-path APIs. Uses en-GB rules
/// (one/other cardinal, one/two/few/other ordinal).
/// </summary>
internal sealed class TestEnGbLocale : ILocale<OneOtherCardinal, EnglishOrdinal>
{
  public CultureInfo Culture { get; } = CultureInfo.GetCultureInfo("en-GB");
}

/// <summary>
/// Locale using German culture for testing culture-specific number formatting
/// (period as thousands separator). Uses one/other cardinal and other-only ordinal.
/// </summary>
internal sealed class TestDeLocale : ILocale<OneOtherCardinal, OtherOnlyOrdinal>
{
  public CultureInfo Culture { get; } = CultureInfo.GetCultureInfo("de-DE");
}
