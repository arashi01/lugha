// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Rules.Ordinals;

namespace Lugha.Tests.Ordinals;

/// <summary>
/// Tests <see cref="OtherOnlyOrdinal"/> against CLDR @integer sample vectors.
/// CLDR source: ordinals.xml lines 16-18.
/// Applies to: ar, cs, da, de, fa, fi, he, hr, id, ja, km, ko, nl, pl,
///   pt, ru, sk, sl, sr, sw, th, tr, ur, zh, and many more.
/// </summary>
public sealed class OtherOnlyOrdinalTests
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
  public void Ordinal_AlwaysReturnsOther(int count) =>
      OtherOnlyOrdinal.Ordinal(count).Should().Be(OrdinalCategory.Other);
}
