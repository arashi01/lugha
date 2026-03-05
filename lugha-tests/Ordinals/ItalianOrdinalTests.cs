// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Ordinals;

namespace Lugha.Tests.Ordinals;

/// <summary>
/// Tests <see cref="ItalianOrdinal"/> against CLDR @integer sample vectors.
/// CLDR source: ordinals.xml lines 68–71.
/// Rule: many = n ∈ {8, 11, 80, 800}; other = everything else.
/// </summary>
public sealed class ItalianOrdinalTests
{
  [Theory]
  [InlineData(8)]
  [InlineData(11)]
  [InlineData(80)]
  [InlineData(800)]
  public void Ordinal_ReturnsMany(int count) =>
      ItalianOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Many);

  [Theory]
  [InlineData(0)]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(4)]
  [InlineData(5)]
  [InlineData(6)]
  [InlineData(7)]
  [InlineData(9)]
  [InlineData(10)]
  [InlineData(12)]
  [InlineData(13)]
  [InlineData(20)]
  [InlineData(50)]
  [InlineData(79)]
  [InlineData(81)]
  [InlineData(100)]
  [InlineData(799)]
  [InlineData(801)]
  [InlineData(1000)]
  public void Ordinal_ReturnsOther(int count) =>
      ItalianOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Other);

  /// <summary>
  /// Exhaustive check: exactly four values produce Many.
  /// </summary>
  [Fact]
  public void Ordinal_ManyValues_AreExactlyFour()
  {
    int[] manyValues = [8, 11, 80, 800];

    foreach (int value in manyValues)
    {
      ItalianOrdinal.Ordinal(value).Should().Be(OrdinalCategory.Many,
          because: $"{value} should be Many");
    }
  }
}
