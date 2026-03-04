// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules.Ordinal;

/// <summary>
/// Ordinal: other → everything. No distinctions.
/// Applies to: ar, cs, da, de, fa, fi, he, hr, id, ja, km, kn, ko,
///   ky, lt, lv, ml, mn, my, nb, nl, no, pa, pl, ps, pt, root, ru, sd,
///   sh, si, sk, sl, sr, sw, ta, te, th, tr, ur, uz, zh, and many more.
/// </summary>
/// <remarks>
/// See <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/ordinals.xml">CLDR ordinals.xml</see>.
/// </remarks>
public readonly struct OtherOnlyOrdinal : IOrdinalRules<OtherOnlyOrdinal>
{
  public static OrdinalCategory Ordinal(int count) =>
      OrdinalCategory.Other;
}
