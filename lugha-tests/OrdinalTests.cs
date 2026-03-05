// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Globalization;
using Lugha.Rules.Ordinals;

namespace Lugha.Tests;

/// <summary>
/// Tests <see cref="Ordinal"/> static functions - both generic and locale paths.
/// Validates ordinal format composition (no space between count and suffix),
/// culture-specific number formatting, and <c>TryFormat</c> span behaviour.
/// </summary>
public sealed class OrdinalTests
{
  private static readonly OrdinalForms EnglishSuffixes = new()
  {
    Other = "th",
    One = "st",
    Two = "nd",
    Few = "rd",
  };

  private static readonly CultureInfo EnGb = CultureInfo.GetCultureInfo("en-GB");
  private static readonly CultureInfo DeDe = CultureInfo.GetCultureInfo("de-DE");
  private static readonly TestEnGbLocale EnGbLocale = new();
  private static readonly TestDeLocale DeLocale = new();

  // ---- Generic path: Select ---------------------------------------

  [Fact]
  public void Select_Generic_ReturnsCorrectSuffix()
  {
    Lugha.Ordinal.Select<EnglishOrdinal>(1, EnglishSuffixes).Should().Be("st");
    Lugha.Ordinal.Select<EnglishOrdinal>(2, EnglishSuffixes).Should().Be("nd");
    Lugha.Ordinal.Select<EnglishOrdinal>(3, EnglishSuffixes).Should().Be("rd");
    Lugha.Ordinal.Select<EnglishOrdinal>(4, EnglishSuffixes).Should().Be("th");
  }

  // ---- Generic path: Format (no space) ----------------------------

  [Fact]
  public void Format_Generic_ConcatenatesWithoutSpace()
  {
    string result = Lugha.Ordinal.Format<EnglishOrdinal>(1, EnglishSuffixes, EnGb);
    result.Should().Be("1st");
  }

  [Fact]
  public void Format_Generic_VariousSuffixes()
  {
    Lugha.Ordinal.Format<EnglishOrdinal>(2, EnglishSuffixes, EnGb).Should().Be("2nd");
    Lugha.Ordinal.Format<EnglishOrdinal>(3, EnglishSuffixes, EnGb).Should().Be("3rd");
    Lugha.Ordinal.Format<EnglishOrdinal>(11, EnglishSuffixes, EnGb).Should().Be("11th");
    Lugha.Ordinal.Format<EnglishOrdinal>(21, EnglishSuffixes, EnGb).Should().Be("21st");
  }

  /// <summary>
  /// Verifies that the culture parameter flows through to number formatting.
  /// German uses period as thousands separator.
  /// </summary>
  [Fact]
  public void Format_Generic_PassesCultureToNumberFormatting()
  {
    string result = Lugha.Ordinal.Format<EnglishOrdinal>(1001, EnglishSuffixes, DeDe);

    result.Should().Be("1.001st");
  }

  // ---- Generic path: TryFormat ------------------------------------

  [Fact]
  public void TryFormat_Generic_WritesToBuffer()
  {
    Span<char> buffer = stackalloc char[64];
    bool success = Lugha.Ordinal.TryFormat<EnglishOrdinal>(
        1, EnglishSuffixes, EnGb, buffer, out int written);

    success.Should().BeTrue();
    buffer[..written].ToString().Should().Be("1st");
  }

  [Fact]
  public void TryFormat_Generic_InsufficientBuffer_ReturnsFalse()
  {
    Span<char> tiny = stackalloc char[2];
    bool success = Lugha.Ordinal.TryFormat<EnglishOrdinal>(
        1000, EnglishSuffixes, EnGb, tiny, out int written);

    success.Should().BeFalse();
    written.Should().Be(0);
  }

  // ---- Locale path: Select ----------------------------------------

  [Fact]
  public void Select_Locale_ReturnsCorrectSuffix()
  {
    Lugha.Ordinal.Select(1, EnglishSuffixes, EnGbLocale).Should().Be("st");
    Lugha.Ordinal.Select(4, EnglishSuffixes, EnGbLocale).Should().Be("th");
  }

  // ---- Locale path: Format (no space) -----------------------------

  [Fact]
  public void Format_Locale_ConcatenatesWithoutSpace()
  {
    string result = Lugha.Ordinal.Format(1, EnglishSuffixes, EnGbLocale);
    result.Should().Be("1st");
  }

  [Fact]
  public void Format_Locale_UsesLocaleCulture()
  {
    string result = Lugha.Ordinal.Format(1001, EnglishSuffixes, DeLocale);

    result.Should().Be("1.001th");
    result.Should().NotContain(" ");
  }

  // ---- Locale path: TryFormat -------------------------------------

  [Fact]
  public void TryFormat_Locale_WritesToBuffer()
  {
    Span<char> buffer = stackalloc char[64];
    bool success = Lugha.Ordinal.TryFormat(
        3, EnglishSuffixes, EnGbLocale, buffer, out int written);

    success.Should().BeTrue();
    buffer[..written].ToString().Should().Be("3rd");
  }

  [Fact]
  public void TryFormat_Locale_InsufficientBuffer_ReturnsFalse()
  {
    Span<char> tiny = stackalloc char[2];
    bool success = Lugha.Ordinal.TryFormat(
        1000, EnglishSuffixes, EnGbLocale, tiny, out int written);

    success.Should().BeFalse();
    written.Should().Be(0);
  }

  // ---- LocaleExtensions convenience ---------------------------------

  [Fact]
  public void OrdinalSelect_Extension_MatchesSelect()
  {
    string expected = Lugha.Ordinal.Select(21, EnglishSuffixes, EnGbLocale);
    string result = EnGbLocale.OrdinalSelect(21, EnglishSuffixes);

    result.Should().Be(expected);
  }

  [Fact]
  public void OrdinalFormat_Extension_MatchesFormat()
  {
    string expected = Lugha.Ordinal.Format(21, EnglishSuffixes, EnGbLocale);
    string result = EnGbLocale.OrdinalFormat(21, EnglishSuffixes);

    result.Should().Be(expected);
  }
}
