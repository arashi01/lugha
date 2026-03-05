// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Ordinals;

namespace Lugha.Tests.Ordinals;

/// <summary>
/// Tests <see cref="WelshOrdinal"/> against CLDR @integer sample vectors.
/// CLDR source: ordinals.xml lines 183-190.
/// Welsh ordinals use all six categories at exact values:
/// zero=0,7,8,9; one=1; two=2; few=3,4; many=5,6; other=10+.
/// </summary>
public sealed class WelshOrdinalTests
{
  [Theory]
  [InlineData(0)]
  [InlineData(7)]
  [InlineData(8)]
  [InlineData(9)]
  public void Ordinal_ReturnsZero(int count) =>
      WelshOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Zero);

  [Theory]
  [InlineData(1)]
  public void Ordinal_ReturnsOne(int count) =>
      WelshOrdinal.Ordinal(count).Should().Be(OrdinalCategory.One);

  [Theory]
  [InlineData(2)]
  public void Ordinal_ReturnsTwo(int count) =>
      WelshOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Two);

  [Theory]
  [InlineData(3)]
  [InlineData(4)]
  public void Ordinal_ReturnsFew(int count) =>
      WelshOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Few);

  [Theory]
  [InlineData(5)]
  [InlineData(6)]
  public void Ordinal_ReturnsMany(int count) =>
      WelshOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Many);

  [Theory]
  [InlineData(10)]
  [InlineData(11)]
  [InlineData(15)]
  [InlineData(20)]
  [InlineData(50)]
  [InlineData(100)]
  [InlineData(1000)]
  public void Ordinal_ReturnsOther(int count) =>
      WelshOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Other);

  [Fact]
  public void Ordinal_AllSixCategories_Exercised()
  {
    WelshOrdinal.Ordinal(0).Should().Be(OrdinalCategory.Zero);
    WelshOrdinal.Ordinal(1).Should().Be(OrdinalCategory.One);
    WelshOrdinal.Ordinal(2).Should().Be(OrdinalCategory.Two);
    WelshOrdinal.Ordinal(3).Should().Be(OrdinalCategory.Few);
    WelshOrdinal.Ordinal(5).Should().Be(OrdinalCategory.Many);
    WelshOrdinal.Ordinal(10).Should().Be(OrdinalCategory.Other);
  }
}
