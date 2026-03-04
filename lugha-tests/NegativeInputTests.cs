// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System;
using System.Globalization;
using Lugha.Rules.Cardinal;
using Lugha.Rules.Ordinal;

namespace Lugha.Tests;

/// <summary>
/// Verifies that all public entry points accepting a <c>count</c> parameter
/// throw <see cref="ArgumentOutOfRangeException"/> for negative values.
/// </summary>
public sealed class NegativeInputTests
{
  private static readonly PluralForms ItemForms = new() { Other = "items", One = "item" };
  private static readonly OrdinalForms Suffixes = new() { Other = "th", One = "st" };
  private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
  private static readonly TestEnGbLocale Locale = new();

  // ---- Plural — generic path --------------------------------------

  [Fact]
  public void Plural_Select_Generic_ThrowsForNegative() =>
      Assert.Throws<ArgumentOutOfRangeException>(
          () => Plural.Select<OneOtherCardinal>(-1, ItemForms));

  [Fact]
  public void Plural_Format_Generic_ThrowsForNegative() =>
      Assert.Throws<ArgumentOutOfRangeException>(
          () => Plural.Format<OneOtherCardinal>(-1, ItemForms, Culture));

  [Fact]
  public void Plural_TryFormat_Generic_ThrowsForNegative()
  {
    char[] buffer = new char[64];
    Assert.Throws<ArgumentOutOfRangeException>(
        () => Plural.TryFormat<OneOtherCardinal>(-1, ItemForms, Culture, buffer, out _));
  }

  // ---- Plural — locale path ---------------------------------------

  [Fact]
  public void Plural_Select_Locale_ThrowsForNegative() =>
      Assert.Throws<ArgumentOutOfRangeException>(
          () => Plural.Select(-1, ItemForms, Locale));

  [Fact]
  public void Plural_Format_Locale_ThrowsForNegative() =>
      Assert.Throws<ArgumentOutOfRangeException>(
          () => Plural.Format(-1, ItemForms, Locale));

  [Fact]
  public void Plural_TryFormat_Locale_ThrowsForNegative()
  {
    char[] buffer = new char[64];
    Assert.Throws<ArgumentOutOfRangeException>(
        () => Plural.TryFormat(-1, ItemForms, Locale, buffer, out _));
  }

  // ---- Ordinal — generic path -------------------------------------

  [Fact]
  public void Ordinal_Select_Generic_ThrowsForNegative() =>
      Assert.Throws<ArgumentOutOfRangeException>(
          () => Lugha.Ordinal.Select<EnglishOrdinal>(-1, Suffixes));

  [Fact]
  public void Ordinal_Format_Generic_ThrowsForNegative() =>
      Assert.Throws<ArgumentOutOfRangeException>(
          () => Lugha.Ordinal.Format<EnglishOrdinal>(-1, Suffixes, Culture));

  [Fact]
  public void Ordinal_TryFormat_Generic_ThrowsForNegative()
  {
    char[] buffer = new char[64];
    Assert.Throws<ArgumentOutOfRangeException>(
        () => Lugha.Ordinal.TryFormat<EnglishOrdinal>(-1, Suffixes, Culture, buffer, out _));
  }

  // ---- Ordinal — locale path --------------------------------------

  [Fact]
  public void Ordinal_Select_Locale_ThrowsForNegative() =>
      Assert.Throws<ArgumentOutOfRangeException>(
          () => Lugha.Ordinal.Select(-1, Suffixes, Locale));

  [Fact]
  public void Ordinal_Format_Locale_ThrowsForNegative() =>
      Assert.Throws<ArgumentOutOfRangeException>(
          () => Lugha.Ordinal.Format(-1, Suffixes, Locale));

  [Fact]
  public void Ordinal_TryFormat_Locale_ThrowsForNegative()
  {
    char[] buffer = new char[64];
    Assert.Throws<ArgumentOutOfRangeException>(
        () => Lugha.Ordinal.TryFormat(-1, Suffixes, Locale, buffer, out _));
  }

  // ---- ILocale DIM boundary ---------------------------------------

  [Fact]
  public void ILocale_Cardinal_ThrowsForNegative() =>
      Assert.Throws<ArgumentOutOfRangeException>(
          () => ((ILocale)Locale).Cardinal(-1));

  [Fact]
  public void ILocale_Ordinal_ThrowsForNegative() =>
      Assert.Throws<ArgumentOutOfRangeException>(
          () => ((ILocale)Locale).Ordinal(-1));
}
