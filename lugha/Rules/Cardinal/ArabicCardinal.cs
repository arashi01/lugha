// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules.Cardinal;

/// <summary>
/// Cardinal: zero → 0; one → 1; two → 2;
///   few → n % 100 in 3..10; many → n % 100 in 11..99;
///   other → everything else.
/// Applies to: ar.
/// </summary>
/// <remarks>
/// All six CLDR categories are reachable for integer counts.
/// See <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/plurals.xml">CLDR plurals.xml</see>.
/// </remarks>
public readonly struct ArabicCardinal : ICardinalRules<ArabicCardinal>
{
  public static PluralCategory Cardinal(int count)
  {
    int mod100 = count % 100;
    return count switch
    {
      0 => PluralCategory.Zero,
      1 => PluralCategory.One,
      2 => PluralCategory.Two,
      _ when mod100 is >= 3 and <= 10 => PluralCategory.Few,
      _ when mod100 is >= 11 and <= 99 => PluralCategory.Many,
      _ => PluralCategory.Other,
    };
  }
}
