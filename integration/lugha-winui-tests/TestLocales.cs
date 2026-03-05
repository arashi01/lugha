// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Globalization;
using Lugha.Rules.Cardinals;
using Lugha.Rules.Ordinals;

namespace Lugha.WinUI.Tests;

/// <summary>
/// Minimal test locale for en-GB.
/// </summary>
internal sealed class TestEnGbLocale : ILocale<OneOtherCardinal, EnglishOrdinal>
{
  public CultureInfo Culture { get; } = CultureInfo.GetCultureInfo("en-GB");
}

/// <summary>
/// Minimal test locale for ar-SA.
/// </summary>
internal sealed class TestArSaLocale : ILocale<ArabicCardinal, OtherOnlyOrdinal>
{
  public CultureInfo Culture { get; } = CultureInfo.GetCultureInfo("ar-SA");
}

/// <summary>
/// Minimal test locale for es-ES.
/// </summary>
internal sealed class TestEsEsLocale : ILocale<LatinEuropeanCardinal, SpanishOrdinal>
{
  public CultureInfo Culture { get; } = CultureInfo.GetCultureInfo("es-ES");
}
