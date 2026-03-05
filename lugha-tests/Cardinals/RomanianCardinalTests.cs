// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Cardinals;

namespace Lugha.Tests.Cardinals;

/// <summary>
/// Tests <see cref="RomanianCardinal"/> against CLDR @integer sample vectors.
/// CLDR source: plurals.xml lines 105–109.
/// For int inputs (v=0): one → 1; few → 0 or n%100 in 1..19 (n≠1); other → 20+.
/// </summary>
public sealed class RomanianCardinalTests
{
  [Theory]
  [InlineData(1)]
  public void Cardinal_ReturnsOne(int count) =>
      RomanianCardinal.Cardinal(count).Should().Be(PluralCategory.One);

  [Theory]
  [InlineData(0)]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(10)]
  [InlineData(15)]
  [InlineData(19)]
  [InlineData(101)]
  [InlineData(102)]
  [InlineData(119)]
  public void Cardinal_ReturnsFew(int count) =>
      RomanianCardinal.Cardinal(count).Should().Be(PluralCategory.Few);

  [Theory]
  [InlineData(20)]
  [InlineData(21)]
  [InlineData(50)]
  [InlineData(100)]
  [InlineData(120)]
  [InlineData(200)]
  [InlineData(1000)]
  public void Cardinal_ReturnsOther(int count) =>
      RomanianCardinal.Cardinal(count).Should().Be(PluralCategory.Other);

  [Fact]
  public void Cardinal_BoundaryBetweenFewAndOther()
  {
    // 19 → few (n%100 = 19, in 1..19)
    RomanianCardinal.Cardinal(19).Should().Be(PluralCategory.Few);
    // 20 → other (n%100 = 20, not in 1..19)
    RomanianCardinal.Cardinal(20).Should().Be(PluralCategory.Other);
  }
}
