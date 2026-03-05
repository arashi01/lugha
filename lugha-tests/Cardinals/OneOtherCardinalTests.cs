// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Cardinals;

namespace Lugha.Tests.Cardinals;

/// <summary>
/// Tests <see cref="OneOtherCardinal"/> against CLDR @integer sample vectors.
/// CLDR source: plurals.xml — <c>i = 1 and v = 0</c>.
/// Applies to: en, de, nl, sv, nb, da.
/// </summary>
public sealed class OneOtherCardinalTests
{
  [Theory]
  [InlineData(1)]
  public void Cardinal_ReturnsOne(int count) =>
      OneOtherCardinal.Cardinal(count).Should().Be(PluralCategory.One);

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
  public void Cardinal_ReturnsOther(int count) =>
      OneOtherCardinal.Cardinal(count).Should().Be(PluralCategory.Other);
}
