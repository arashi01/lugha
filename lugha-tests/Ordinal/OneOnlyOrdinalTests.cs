// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Ordinal;

namespace Lugha.Tests.Ordinal;

/// <summary>
/// Tests <see cref="OneOnlyOrdinal"/> against CLDR @integer sample vectors.
/// CLDR source: ordinals.xml lines 30–33.
/// Rule: one = n = 1; other = everything else.
/// Applies to: fr, ro, ms, vi.
/// </summary>
public sealed class OneOnlyOrdinalTests
{
  [Theory]
  [InlineData(1)]
  public void Ordinal_ReturnsOne(int count) =>
      OneOnlyOrdinal.Ordinal(count).Should().Be(OrdinalCategory.One);

  [Theory]
  [InlineData(0)]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(5)]
  [InlineData(10)]
  [InlineData(15)]
  [InlineData(21)]
  [InlineData(100)]
  [InlineData(1000)]
  public void Ordinal_ReturnsOther(int count) =>
      OneOnlyOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Other);
}
