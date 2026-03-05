// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Cardinals;

namespace Lugha.Tests.Cardinals;

/// <summary>
/// Tests <see cref="OtherOnlyCardinal"/> against CLDR @integer sample vectors.
/// CLDR source: plurals.xml lines 16-18.
/// Applies to: zh, ja, ko - no plural distinctions whatsoever.
/// </summary>
public sealed class OtherOnlyCardinalTests
{
  [Theory]
  [InlineData(0)]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(5)]
  [InlineData(10)]
  [InlineData(15)]
  [InlineData(100)]
  [InlineData(1000)]
  [InlineData(10000)]
  public void Cardinal_AlwaysReturnsOther(int count) =>
      OtherOnlyCardinal.Cardinal(count).Should().Be(PluralCategory.Other);
}
