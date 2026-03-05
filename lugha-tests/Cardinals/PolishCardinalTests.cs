// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Cardinals;

namespace Lugha.Tests.Cardinals;

/// <summary>
/// Tests <see cref="PolishCardinal"/> against CLDR @integer sample vectors.
/// CLDR source: plurals.xml lines 168-173.
/// Categories: one/few/many (other unreachable for int).
/// </summary>
public sealed class PolishCardinalTests
{
  [Theory]
  [InlineData(1)]
  public void Cardinal_ReturnsOne(int count) =>
      PolishCardinal.Cardinal(count).Should().Be(PluralCategory.One);

  [Theory]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(4)]
  [InlineData(22)]
  [InlineData(23)]
  [InlineData(24)]
  [InlineData(32)]
  [InlineData(33)]
  [InlineData(34)]
  [InlineData(102)]
  [InlineData(1002)]
  public void Cardinal_ReturnsFew(int count) =>
      PolishCardinal.Cardinal(count).Should().Be(PluralCategory.Few);

  [Theory]
  [InlineData(0)]
  [InlineData(5)]
  [InlineData(6)]
  [InlineData(7)]
  [InlineData(8)]
  [InlineData(9)]
  [InlineData(10)]
  [InlineData(11)]
  [InlineData(12)]
  [InlineData(13)]
  [InlineData(14)]
  [InlineData(15)]
  [InlineData(19)]
  [InlineData(20)]
  [InlineData(21)]
  [InlineData(25)]
  [InlineData(100)]
  [InlineData(112)]
  [InlineData(1000)]
  public void Cardinal_ReturnsMany(int count) =>
      PolishCardinal.Cardinal(count).Should().Be(PluralCategory.Many);

  [Fact]
  public void Cardinal_Mod100In12To14_ReturnsMany_NotFew()
  {
    // 12 → many, not few (despite mod10 = 2)
    PolishCardinal.Cardinal(12).Should().Be(PluralCategory.Many);
    // 13 → many, not few (despite mod10 = 3)
    PolishCardinal.Cardinal(13).Should().Be(PluralCategory.Many);
    // 14 → many, not few (despite mod10 = 4)
    PolishCardinal.Cardinal(14).Should().Be(PluralCategory.Many);
  }
}
