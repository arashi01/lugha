// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Cardinals;

namespace Lugha.Tests.Cardinals;

/// <summary>
/// Tests <see cref="CzechSlovakCardinal"/> against CLDR @integer sample vectors.
/// CLDR source: plurals.xml lines 162–167.
/// Categories: one/few/other (many unreachable for int).
/// </summary>
public sealed class CzechSlovakCardinalTests
{
  [Theory]
  [InlineData(1)]
  public void Cardinal_ReturnsOne(int count) =>
      CzechSlovakCardinal.Cardinal(count).Should().Be(PluralCategory.One);

  [Theory]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(4)]
  public void Cardinal_ReturnsFew(int count) =>
      CzechSlovakCardinal.Cardinal(count).Should().Be(PluralCategory.Few);

  [Theory]
  [InlineData(0)]
  [InlineData(5)]
  [InlineData(6)]
  [InlineData(7)]
  [InlineData(10)]
  [InlineData(11)]
  [InlineData(20)]
  [InlineData(100)]
  [InlineData(1000)]
  public void Cardinal_ReturnsOther(int count) =>
      CzechSlovakCardinal.Cardinal(count).Should().Be(PluralCategory.Other);

  [Fact]
  public void Cardinal_FewIsExact2To4_NotModBased()
  {
    // The Czech/Slovak rule is i in 2..4 (exact values), not a mod-based rule.
    CzechSlovakCardinal.Cardinal(12).Should().Be(PluralCategory.Other);
    CzechSlovakCardinal.Cardinal(22).Should().Be(PluralCategory.Other);
  }
}
