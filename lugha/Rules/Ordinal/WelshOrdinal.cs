// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules.Ordinal;

/// <summary>
/// Ordinal: zero → 0, 7, 8, 9; one → 1; two → 2;
///   few → 3, 4; many → 5, 6; other → everything else.
/// Applies to: cy.
/// </summary>
/// <remarks>
/// All six CLDR ordinal categories are reachable.
/// See <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/ordinals.xml">CLDR ordinals.xml</see>.
/// </remarks>
public readonly struct WelshOrdinal : IOrdinalRules<WelshOrdinal>
{
  public static OrdinalCategory Ordinal(int count) => count switch
  {
    0 or 7 or 8 or 9 => OrdinalCategory.Zero,
    1 => OrdinalCategory.One,
    2 => OrdinalCategory.Two,
    3 or 4 => OrdinalCategory.Few,
    5 or 6 => OrdinalCategory.Many,
    _ => OrdinalCategory.Other,
  };
}
