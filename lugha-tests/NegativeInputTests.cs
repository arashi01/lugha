// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Globalization;
using Lugha.Rules.Cardinals;
using Lugha.Rules.Ordinals;

namespace Lugha.Tests;

/// <summary>
/// Verifies that all public entry points accepting a <c>count</c> parameter
/// clamp negative values to zero rather than throwing.
/// </summary>
public sealed class NegativeInputTests
{
  private static readonly PluralForms ItemForms = new() { Other = "items", One = "item" };
  private static readonly OrdinalForms Suffixes = new() { Other = "th", One = "st" };
  private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
  private static readonly TestEnGbLocale Locale = new();

  // ---- Plural - generic path --------------------------------------

  [Fact]
  public void Plural_Select_Generic_ClampsNegativeToZero() =>
      Plural.Select<OneOtherCardinal>(-1, ItemForms).Should().Be("items");

  [Fact]
  public void Plural_Format_Generic_ClampsNegativeToZero() =>
      Plural.Format<OneOtherCardinal>(-1, ItemForms, Culture).Should().StartWith("0");

  [Fact]
  public void Plural_TryFormat_Generic_ClampsNegativeToZero()
  {
    char[] buffer = new char[64];
    Plural.TryFormat<OneOtherCardinal>(-1, ItemForms, Culture, buffer, out int written)
        .Should().BeTrue();
    new string(buffer, 0, written).Should().StartWith("0");
  }

  // ---- Plural - locale path ---------------------------------------

  [Fact]
  public void Plural_Select_Locale_ClampsNegativeToZero() =>
      Plural.Select(-1, ItemForms, Locale).Should().Be("items");

  [Fact]
  public void Plural_Format_Locale_ClampsNegativeToZero() =>
      Plural.Format(-1, ItemForms, Locale).Should().StartWith("0");

  [Fact]
  public void Plural_TryFormat_Locale_ClampsNegativeToZero()
  {
    char[] buffer = new char[64];
    Plural.TryFormat(-1, ItemForms, Locale, buffer, out int written)
        .Should().BeTrue();
    new string(buffer, 0, written).Should().StartWith("0");
  }

  // ---- Ordinal - generic path -------------------------------------

  [Fact]
  public void Ordinal_Select_Generic_ClampsNegativeToZero() =>
      Lugha.Ordinal.Select<EnglishOrdinal>(-1, Suffixes).Should().Be("th");

  [Fact]
  public void Ordinal_Format_Generic_ClampsNegativeToZero() =>
      Lugha.Ordinal.Format<EnglishOrdinal>(-1, Suffixes, Culture).Should().StartWith("0");

  [Fact]
  public void Ordinal_TryFormat_Generic_ClampsNegativeToZero()
  {
    char[] buffer = new char[64];
    Lugha.Ordinal.TryFormat<EnglishOrdinal>(-1, Suffixes, Culture, buffer, out int written)
        .Should().BeTrue();
    new string(buffer, 0, written).Should().StartWith("0");
  }

  // ---- Ordinal - locale path --------------------------------------

  [Fact]
  public void Ordinal_Select_Locale_ClampsNegativeToZero() =>
      Lugha.Ordinal.Select(-1, Suffixes, Locale).Should().Be("th");

  [Fact]
  public void Ordinal_Format_Locale_ClampsNegativeToZero() =>
      Lugha.Ordinal.Format(-1, Suffixes, Locale).Should().StartWith("0");

  [Fact]
  public void Ordinal_TryFormat_Locale_ClampsNegativeToZero()
  {
    char[] buffer = new char[64];
    Lugha.Ordinal.TryFormat(-1, Suffixes, Locale, buffer, out int written)
        .Should().BeTrue();
    new string(buffer, 0, written).Should().StartWith("0");
  }

  // ---- ILocale DIM boundary ---------------------------------------

  [Fact]
  public void ILocale_Cardinal_ClampsNegativeToZero() =>
      ((ILocale)Locale).Cardinal(-1).Should().Be(PluralCategory.Other);

  [Fact]
  public void ILocale_Ordinal_ClampsNegativeToZero() =>
      ((ILocale)Locale).Ordinal(-1).Should().Be(OrdinalCategory.Other);
}
