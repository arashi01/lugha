// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules.Cardinal;

/// <summary>
/// Cardinal: zero → 0; one → 1; two → 2; few → 3; many → 6;
///   other → everything else.
/// Applies to: cy.
/// </summary>
/// <remarks>
/// All six CLDR categories are reachable for integer counts.
/// See <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/plurals.xml">CLDR plurals.xml</see>.
/// </remarks>
public readonly struct WelshCardinal : ICardinalRules<WelshCardinal>
{
  public static PluralCategory Cardinal(int count) => count switch
  {
    0 => PluralCategory.Zero,
    1 => PluralCategory.One,
    2 => PluralCategory.Two,
    3 => PluralCategory.Few,
    6 => PluralCategory.Many,
    _ => PluralCategory.Other,
  };
}
