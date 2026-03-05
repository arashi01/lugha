// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Cardinals;

namespace Lugha.Tests.Cardinals;

/// <summary>
/// Tests <see cref="LatinEuropeanCardinal"/> against CLDR @integer sample vectors.
/// CLDR source: plurals.xml - one/many/other for es, it, pt-PT, ca, lld, scn, vec.
/// The <c>many</c> category applies to non-zero exact multiples of 1,000,000
/// for integer inputs (e = 0, v = 0).
/// </summary>
public sealed class LatinEuropeanCardinalTests
{
  [Theory]
  [InlineData(1)]
  public void Cardinal_ReturnsOne(int count) =>
      LatinEuropeanCardinal.Cardinal(count).Should().Be(PluralCategory.One);

  // CLDR @integer many: 1000000, …
  [Theory]
  [InlineData(1_000_000)]
  [InlineData(2_000_000)]
  [InlineData(3_000_000)]
  [InlineData(100_000_000)]
  public void Cardinal_ReturnsMany(int count) =>
      LatinEuropeanCardinal.Cardinal(count).Should().Be(PluralCategory.Many);

  // CLDR @integer other: 0, 2~16, 100, 1000, 10000, 100000, …
  [Theory]
  [InlineData(0)]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(5)]
  [InlineData(10)]
  [InlineData(15)]
  [InlineData(16)]
  [InlineData(100)]
  [InlineData(1000)]
  [InlineData(10_000)]
  [InlineData(100_000)]
  [InlineData(1_500_000)]
  public void Cardinal_ReturnsOther(int count) =>
      LatinEuropeanCardinal.Cardinal(count).Should().Be(PluralCategory.Other);
}
