// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules.Ordinal;

/// <summary>
/// Ordinal: one → n = 1; other → everything else.
/// Applies to: fr, ro, ms, vi.
/// </summary>
/// <remarks>
/// See <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/ordinals.xml">CLDR ordinals.xml</see>.
/// </remarks>
public readonly struct OneOnlyOrdinal : IOrdinalRules<OneOnlyOrdinal>
{
  public static OrdinalCategory Ordinal(int count) =>
      count == 1 ? OrdinalCategory.One : OrdinalCategory.Other;
}
