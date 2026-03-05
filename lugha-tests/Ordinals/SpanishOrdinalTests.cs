// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Ordinals;

namespace Lugha.Tests.Ordinals;

/// <summary>
/// Tests <see cref="SpanishOrdinal"/> against CLDR @integer sample vectors.
/// CLDR source: ordinals.xml lines 26-29.
/// Rule: one = n%10 ∈ {1,3} ∧ n%100 ≠ 11; other = everything else.
/// </summary>
public sealed class SpanishOrdinalTests
{
  [Theory]
  [InlineData(1)]
  [InlineData(3)]
  [InlineData(21)]
  [InlineData(23)]
  [InlineData(31)]
  [InlineData(33)]
  [InlineData(41)]
  [InlineData(43)]
  [InlineData(51)]
  [InlineData(53)]
  [InlineData(101)]
  [InlineData(103)]
  public void Ordinal_ReturnsOne(int count) =>
      SpanishOrdinal.Ordinal(count).Should().Be(OrdinalCategory.One);

  [Theory]
  [InlineData(0)]
  [InlineData(2)]
  [InlineData(4)]
  [InlineData(5)]
  [InlineData(6)]
  [InlineData(10)]
  [InlineData(14)]
  [InlineData(20)]
  [InlineData(100)]
  [InlineData(1000)]
  public void Ordinal_ReturnsOther(int count) =>
      SpanishOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Other);

  /// <summary>
  /// n%10=1 but n%100=11 → other (not one). Teens exception.
  /// </summary>
  [Theory]
  [InlineData(11)]
  [InlineData(111)]
  public void Ordinal_Mod100Is11_ReturnsOther(int count) =>
      SpanishOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Other);
}
