// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Cardinals;

namespace Lugha.Tests.Cardinals;

/// <summary>
/// Tests <see cref="HebrewCardinal"/> against CLDR @integer sample vectors.
/// CLDR source: plurals.xml lines 87–91.
/// Hebrew uses one/two/other (many was removed in CLDR 42).
/// </summary>
public sealed class HebrewCardinalTests
{
  [Theory]
  [InlineData(1)]
  public void Cardinal_ReturnsOne(int count) =>
      HebrewCardinal.Cardinal(count).Should().Be(PluralCategory.One);

  [Theory]
  [InlineData(2)]
  public void Cardinal_ReturnsTwo(int count) =>
      HebrewCardinal.Cardinal(count).Should().Be(PluralCategory.Two);

  [Theory]
  [InlineData(0)]
  [InlineData(3)]
  [InlineData(4)]
  [InlineData(5)]
  [InlineData(10)]
  [InlineData(11)]
  [InlineData(17)]
  [InlineData(20)]
  [InlineData(100)]
  [InlineData(1000)]
  public void Cardinal_ReturnsOther(int count) =>
      HebrewCardinal.Cardinal(count).Should().Be(PluralCategory.Other);
}
