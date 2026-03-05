// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Ordinals;

namespace Lugha.Tests.Ordinals;

/// <summary>
/// Tests <see cref="EnglishOrdinal"/> against CLDR @integer sample vectors.
/// CLDR source: ordinals.xml lines 106-111.
/// Rule: one = n%10=1 ∧ n%100≠11; two = n%10=2 ∧ n%100≠12;
///   few = n%10=3 ∧ n%100≠13; other = everything else.
/// </summary>
public sealed class EnglishOrdinalTests
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
  public void Ordinal_ReturnsOne(int count) =>
      EnglishOrdinal.Ordinal(count).Should().Be(OrdinalCategory.One);

  [Theory]
  [InlineData(2)]
  [InlineData(22)]
  [InlineData(32)]
  [InlineData(42)]
  [InlineData(52)]
  [InlineData(62)]
  [InlineData(72)]
  [InlineData(82)]
  [InlineData(102)]
  [InlineData(1002)]
  public void Ordinal_ReturnsTwo(int count) =>
      EnglishOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Two);

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
      EnglishOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Few);

  [Theory]
  [InlineData(0)]
  [InlineData(4)]
  [InlineData(5)]
  [InlineData(6)]
  [InlineData(7)]
  [InlineData(8)]
  [InlineData(9)]
  [InlineData(10)]
  [InlineData(14)]
  [InlineData(15)]
  [InlineData(100)]
  [InlineData(1000)]
  public void Ordinal_ReturnsOther(int count) =>
      EnglishOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Other);

  /// <summary>
  /// n%10=1 but n%100=11 → other (not one). Boundary for the "teens" exception.
  /// </summary>
  [Theory]
  [InlineData(11)]
  [InlineData(111)]
  [InlineData(211)]
  public void Ordinal_Mod100Is11_ReturnsOther(int count) =>
      EnglishOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Other);

  /// <summary>
  /// n%10=2 but n%100=12 → other (not two).
  /// </summary>
  [Theory]
  [InlineData(12)]
  [InlineData(112)]
  [InlineData(212)]
  public void Ordinal_Mod100Is12_ReturnsOther(int count) =>
      EnglishOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Other);

  /// <summary>
  /// n%10=3 but n%100=13 → other (not few).
  /// </summary>
  [Theory]
  [InlineData(13)]
  [InlineData(113)]
  [InlineData(213)]
  public void Ordinal_Mod100Is13_ReturnsOther(int count) =>
      EnglishOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Other);
}
