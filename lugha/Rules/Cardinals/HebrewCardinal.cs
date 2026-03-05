// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules.Cardinals;

/// <summary>
/// Cardinal: one → i = 1 ∧ v = 0; two → i = 2 ∧ v = 0;
///   other → everything else.
/// Applies to: he.
/// </summary>
/// <remarks>
/// See <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/plurals.xml">CLDR plurals.xml</see>.
/// </remarks>
public readonly struct HebrewCardinal : ICardinalRules<HebrewCardinal>
{
  public static PluralCategory Cardinal(int count) => count switch
  {
    1 => PluralCategory.One,
    2 => PluralCategory.Two,
    _ => PluralCategory.Other,
  };
}
