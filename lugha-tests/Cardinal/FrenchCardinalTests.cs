// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Cardinal;

namespace Lugha.Tests.Cardinal;

/// <summary>
/// Tests <see cref="FrenchCardinal"/> against CLDR @integer sample vectors.
/// CLDR source: plurals.xml lines 118–127 — <c>i = 0,1</c>.
/// Applies to: fr, pt (Brazilian).
/// </summary>
public sealed class FrenchCardinalTests
{
  [Theory]
  [InlineData(0)]
  [InlineData(1)]
  public void Cardinal_ReturnsOne(int count) =>
      FrenchCardinal.Cardinal(count).Should().Be(PluralCategory.One);

  [Theory]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(10)]
  [InlineData(17)]
  [InlineData(100)]
  [InlineData(1000)]
  [InlineData(10000)]
  public void Cardinal_ReturnsOther(int count) =>
      FrenchCardinal.Cardinal(count).Should().Be(PluralCategory.Other);
}
