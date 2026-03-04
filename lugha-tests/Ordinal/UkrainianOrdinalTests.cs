// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Ordinal;

namespace Lugha.Tests.Ordinal;

/// <summary>
/// Tests <see cref="UkrainianOrdinal"/> against CLDR @integer sample vectors.
/// CLDR source: ordinals.xml lines 53–56.
/// Rule: few = n%10 = 3 ∧ n%100 ≠ 13; other = everything else.
/// </summary>
public sealed class UkrainianOrdinalTests
{
  [Theory]
  [InlineData(3)]
  [InlineData(23)]
  [InlineData(33)]
  [InlineData(43)]
  [InlineData(53)]
  [InlineData(63)]
  [InlineData(73)]
  [InlineData(83)]
  [InlineData(103)]
  [InlineData(1003)]
  public void Ordinal_ReturnsFew(int count) =>
      UkrainianOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Few);

  [Theory]
  [InlineData(0)]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(4)]
  [InlineData(5)]
  [InlineData(10)]
  [InlineData(14)]
  [InlineData(15)]
  [InlineData(20)]
  [InlineData(100)]
  [InlineData(1000)]
  public void Ordinal_ReturnsOther(int count) =>
      UkrainianOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Other);

  /// <summary>
  /// n%10=3 but n%100=13 → other (not few). Teens exception.
  /// </summary>
  [Theory]
  [InlineData(13)]
  [InlineData(113)]
  [InlineData(213)]
  public void Ordinal_Mod100Is13_ReturnsOther(int count) =>
      UkrainianOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Other);
}
