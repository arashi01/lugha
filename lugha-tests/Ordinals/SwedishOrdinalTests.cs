// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Ordinals;

namespace Lugha.Tests.Ordinals;

/// <summary>
/// Tests <see cref="SwedishOrdinal"/> against CLDR @integer sample vectors.
/// CLDR source: ordinals.xml lines 22-25.
/// Rule: one = n%10 ∈ {1,2} ∧ n%100 ∉ {11,12}; other = everything else.
/// </summary>
public sealed class SwedishOrdinalTests
{
  [Theory]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(21)]
  [InlineData(22)]
  [InlineData(31)]
  [InlineData(32)]
  [InlineData(41)]
  [InlineData(42)]
  [InlineData(51)]
  [InlineData(52)]
  [InlineData(101)]
  [InlineData(102)]
  public void Ordinal_ReturnsOne(int count) =>
      SwedishOrdinal.Ordinal(count).Should().Be(OrdinalCategory.One);

  [Theory]
  [InlineData(0)]
  [InlineData(3)]
  [InlineData(4)]
  [InlineData(5)]
  [InlineData(10)]
  [InlineData(13)]
  [InlineData(14)]
  [InlineData(20)]
  [InlineData(100)]
  [InlineData(1000)]
  public void Ordinal_ReturnsOther(int count) =>
      SwedishOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Other);

  /// <summary>
  /// n%10=1 but n%100=11 → other (not one). Teens exception.
  /// </summary>
  [Theory]
  [InlineData(11)]
  [InlineData(111)]
  public void Ordinal_Mod100Is11_ReturnsOther(int count) =>
      SwedishOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Other);

  /// <summary>
  /// n%10=2 but n%100=12 → other (not one). Teens exception.
  /// </summary>
  [Theory]
  [InlineData(12)]
  [InlineData(112)]
  public void Ordinal_Mod100Is12_ReturnsOther(int count) =>
      SwedishOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Other);
}
