// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Cardinals;

namespace Lugha.Tests.Cardinals;

/// <summary>
/// Tests <see cref="EastSlavicCardinal"/> against CLDR @integer sample vectors.
/// CLDR source: plurals.xml lines 186-191.
/// Applies to: ru, uk.
/// Categories: one/few/many/other (other unreachable for int).
/// </summary>
public sealed class EastSlavicCardinalTests
{
  [Theory]
  [InlineData(1)]
  [InlineData(21)]
  [InlineData(31)]
  [InlineData(41)]
  [InlineData(51)]
  [InlineData(61)]
  [InlineData(71)]
  [InlineData(81)]
  [InlineData(101)]
  [InlineData(1001)]
  public void Cardinal_ReturnsOne(int count) =>
      EastSlavicCardinal.Cardinal(count).Should().Be(PluralCategory.One);

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
      EastSlavicCardinal.Cardinal(count).Should().Be(PluralCategory.Few);

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
  [InlineData(16)]
  [InlineData(17)]
  [InlineData(18)]
  [InlineData(19)]
  [InlineData(20)]
  [InlineData(25)]
  [InlineData(100)]
  [InlineData(111)]
  [InlineData(112)]
  public void Cardinal_ReturnsMany(int count) =>
      EastSlavicCardinal.Cardinal(count).Should().Be(PluralCategory.Many);

  [Fact]
  public void Cardinal_Mod100In11To14_ReturnsMany_NotOneOrFew()
  {
    // 11 → many, not one (despite mod10 = 1)
    EastSlavicCardinal.Cardinal(11).Should().Be(PluralCategory.Many);
    // 12 → many, not few (despite mod10 = 2)
    EastSlavicCardinal.Cardinal(12).Should().Be(PluralCategory.Many);
    // 13 → many (despite mod10 = 3)
    EastSlavicCardinal.Cardinal(13).Should().Be(PluralCategory.Many);
    // 14 → many (despite mod10 = 4)
    EastSlavicCardinal.Cardinal(14).Should().Be(PluralCategory.Many);
  }
}
