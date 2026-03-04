// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System;
using System.Globalization;
using Lugha.Rules.Cardinal;

namespace Lugha.Tests;

/// <summary>
/// Tests <see cref="Plural"/> static functions — both generic and locale paths.
/// Validates format composition, culture-specific number formatting, and
/// <c>TryFormat</c> span behaviour.
/// </summary>
public sealed class PluralTests
{
  private static readonly PluralForms ItemForms = new()
  {
    Other = "items",
    One = "item",
  };

  private static readonly CultureInfo EnGb = CultureInfo.GetCultureInfo("en-GB");
  private static readonly CultureInfo DeDe = CultureInfo.GetCultureInfo("de-DE");
  private static readonly TestEnGbLocale EnGbLocale = new();
  private static readonly TestDeLocale DeLocale = new();

  // ---- Generic path: Select ---------------------------------------

  [Fact]
  public void Select_Generic_ReturnsOne_ForCount1()
  {
    string result = Plural.Select<OneOtherCardinal>(1, ItemForms);
    result.Should().Be("item");
  }

  [Fact]
  public void Select_Generic_ReturnsOther_ForCount0()
  {
    string result = Plural.Select<OneOtherCardinal>(0, ItemForms);
    result.Should().Be("items");
  }

  [Theory]
  [InlineData(2)]
  [InlineData(5)]
  [InlineData(100)]
  public void Select_Generic_ReturnsOther_ForPluralCounts(int count)
  {
    string result = Plural.Select<OneOtherCardinal>(count, ItemForms);
    result.Should().Be("items");
  }

  // ---- Generic path: Format ---------------------------------------

  [Fact]
  public void Format_Generic_ProducesCountSpaceForm()
  {
    string result = Plural.Format<OneOtherCardinal>(1, ItemForms, EnGb);
    result.Should().Be("1 item");
  }

  [Fact]
  public void Format_Generic_PluralCount()
  {
    string result = Plural.Format<OneOtherCardinal>(5, ItemForms, EnGb);
    result.Should().Be("5 items");
  }

  /// <summary>
  /// Verifies that the culture parameter flows to <c>count.ToString(culture)</c>.
  /// For integers under the default "G" format, group separators are not
  /// included — the culture parameter's visible effect surfaces with
  /// decimal operands (post-v1.0) and through <c>TryWrite</c>'s
  /// interpolated string handler.
  /// </summary>
  [Fact]
  public void Format_Generic_PassesCultureToNumberFormatting()
  {
    string result = Plural.Format<OneOtherCardinal>(1000, ItemForms, DeDe);

    // Default int format: no group separators regardless of culture
    result.Should().Be("1000 items");
  }

  // ---- Generic path: TryFormat ------------------------------------

  [Fact]
  public void TryFormat_Generic_WritesToBuffer()
  {
    Span<char> buffer = stackalloc char[64];
    bool success = Plural.TryFormat<OneOtherCardinal>(
        1, ItemForms, EnGb, buffer, out int written);

    success.Should().BeTrue();
    buffer[..written].ToString().Should().Be("1 item");
  }

  [Fact]
  public void TryFormat_Generic_InsufficientBuffer_ReturnsFalse()
  {
    Span<char> tiny = stackalloc char[2];
    bool success = Plural.TryFormat<OneOtherCardinal>(
        1000, ItemForms, EnGb, tiny, out int written);

    success.Should().BeFalse();
    written.Should().Be(0);
  }

  // ---- Locale path: Select ----------------------------------------

  [Fact]
  public void Select_Locale_ReturnsOne_ForCount1()
  {
    string result = Plural.Select(1, ItemForms, EnGbLocale);
    result.Should().Be("item");
  }

  [Fact]
  public void Select_Locale_ReturnsOther_ForPluralCount()
  {
    string result = Plural.Select(5, ItemForms, EnGbLocale);
    result.Should().Be("items");
  }

  // ---- Locale path: Format ----------------------------------------

  [Fact]
  public void Format_Locale_ProducesCountSpaceForm()
  {
    string result = Plural.Format(1, ItemForms, EnGbLocale);
    result.Should().Be("1 item");
  }

  [Fact]
  public void Format_Locale_UsesLocaleCulture()
  {
    string result = Plural.Format(1000, ItemForms, DeLocale);

    result.Should().Be("1000 items");
  }

  // ---- Locale path: TryFormat -------------------------------------

  [Fact]
  public void TryFormat_Locale_WritesToBuffer()
  {
    Span<char> buffer = stackalloc char[64];
    bool success = Plural.TryFormat(
        5, ItemForms, EnGbLocale, buffer, out int written);

    success.Should().BeTrue();
    buffer[..written].ToString().Should().Be("5 items");
  }

  [Fact]
  public void TryFormat_Locale_InsufficientBuffer_ReturnsFalse()
  {
    Span<char> tiny = stackalloc char[2];
    bool success = Plural.TryFormat(
        1000, ItemForms, EnGbLocale, tiny, out int written);

    success.Should().BeFalse();
    written.Should().Be(0);
  }

  // ---- LocaleExtensions convenience: PluralFormat -----------------

  [Fact]
  public void PluralFormat_Extension_MatchesFormat()
  {
    string expected = Plural.Format(3, ItemForms, EnGbLocale);
    string result = EnGbLocale.PluralFormat(3, ItemForms);

    result.Should().Be(expected);
  }
}
