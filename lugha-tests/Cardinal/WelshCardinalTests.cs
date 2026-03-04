// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Cardinal;

namespace Lugha.Tests.Cardinal;

/// <summary>
/// Tests <see cref="WelshCardinal"/> against CLDR @integer sample vectors.
/// CLDR source: plurals.xml lines 249–256.
/// Welsh uses all six categories at exact values:
/// zero=0, one=1, two=2, few=3, many=6, other=everything else.
/// </summary>
public sealed class WelshCardinalTests
{
  [Theory]
  [InlineData(0)]
  public void Cardinal_ReturnsZero(int count) =>
      WelshCardinal.Cardinal(count).Should().Be(PluralCategory.Zero);

  [Theory]
  [InlineData(1)]
  public void Cardinal_ReturnsOne(int count) =>
      WelshCardinal.Cardinal(count).Should().Be(PluralCategory.One);

  [Theory]
  [InlineData(2)]
  public void Cardinal_ReturnsTwo(int count) =>
      WelshCardinal.Cardinal(count).Should().Be(PluralCategory.Two);

  [Theory]
  [InlineData(3)]
  public void Cardinal_ReturnsFew(int count) =>
      WelshCardinal.Cardinal(count).Should().Be(PluralCategory.Few);

  [Theory]
  [InlineData(6)]
  public void Cardinal_ReturnsMany(int count) =>
      WelshCardinal.Cardinal(count).Should().Be(PluralCategory.Many);

  [Theory]
  [InlineData(4)]
  [InlineData(5)]
  [InlineData(7)]
  [InlineData(8)]
  [InlineData(9)]
  [InlineData(10)]
  [InlineData(15)]
  [InlineData(20)]
  [InlineData(100)]
  [InlineData(1000)]
  public void Cardinal_ReturnsOther(int count) =>
      WelshCardinal.Cardinal(count).Should().Be(PluralCategory.Other);

  [Fact]
  public void Cardinal_AllSixCategories_Exercised()
  {
    WelshCardinal.Cardinal(0).Should().Be(PluralCategory.Zero);
    WelshCardinal.Cardinal(1).Should().Be(PluralCategory.One);
    WelshCardinal.Cardinal(2).Should().Be(PluralCategory.Two);
    WelshCardinal.Cardinal(3).Should().Be(PluralCategory.Few);
    WelshCardinal.Cardinal(6).Should().Be(PluralCategory.Many);
    WelshCardinal.Cardinal(10).Should().Be(PluralCategory.Other);
  }
}
