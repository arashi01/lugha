// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Cardinals;

namespace Lugha.Tests.Cardinals;

/// <summary>
/// Tests <see cref="ArabicCardinal"/> against CLDR @integer sample vectors.
/// CLDR source: plurals.xml lines 241-248.
/// Uses all six categories: zero/one/two/few/many/other.
/// </summary>
public sealed class ArabicCardinalTests
{
  [Theory]
  [InlineData(0)]
  public void Cardinal_ReturnsZero(int count) =>
      ArabicCardinal.Cardinal(count).Should().Be(PluralCategory.Zero);

  [Theory]
  [InlineData(1)]
  public void Cardinal_ReturnsOne(int count) =>
      ArabicCardinal.Cardinal(count).Should().Be(PluralCategory.One);

  [Theory]
  [InlineData(2)]
  public void Cardinal_ReturnsTwo(int count) =>
      ArabicCardinal.Cardinal(count).Should().Be(PluralCategory.Two);

  [Theory]
  [InlineData(3)]
  [InlineData(4)]
  [InlineData(5)]
  [InlineData(6)]
  [InlineData(7)]
  [InlineData(8)]
  [InlineData(9)]
  [InlineData(10)]
  [InlineData(103)]
  [InlineData(110)]
  [InlineData(1003)]
  public void Cardinal_ReturnsFew(int count) =>
      ArabicCardinal.Cardinal(count).Should().Be(PluralCategory.Few);

  [Theory]
  [InlineData(11)]
  [InlineData(12)]
  [InlineData(25)]
  [InlineData(50)]
  [InlineData(99)]
  [InlineData(111)]
  [InlineData(199)]
  public void Cardinal_ReturnsMany(int count) =>
      ArabicCardinal.Cardinal(count).Should().Be(PluralCategory.Many);

  [Theory]
  [InlineData(100)]
  [InlineData(200)]
  [InlineData(300)]
  [InlineData(400)]
  [InlineData(500)]
  [InlineData(600)]
  [InlineData(700)]
  [InlineData(800)]
  [InlineData(900)]
  [InlineData(1000)]
  public void Cardinal_ReturnsOther(int count) =>
      ArabicCardinal.Cardinal(count).Should().Be(PluralCategory.Other);

  [Fact]
  public void Cardinal_AllSixCategories_Exercised()
  {
    // Verifies that the Arabic rule truly exercises all six CLDR categories.
    ArabicCardinal.Cardinal(0).Should().Be(PluralCategory.Zero);
    ArabicCardinal.Cardinal(1).Should().Be(PluralCategory.One);
    ArabicCardinal.Cardinal(2).Should().Be(PluralCategory.Two);
    ArabicCardinal.Cardinal(3).Should().Be(PluralCategory.Few);
    ArabicCardinal.Cardinal(11).Should().Be(PluralCategory.Many);
    ArabicCardinal.Cardinal(100).Should().Be(PluralCategory.Other);
  }
}
