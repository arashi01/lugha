// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules.Cardinals;

/// <summary>
/// Cardinal: one → i = 1 ∧ v = 0; few → i in 2..4 ∧ v = 0;
///   many → v ≠ 0; other → everything else.
/// Applies to: cs, sk.
/// </summary>
/// <remarks>
/// For integer counts, only one/few/other are reachable; the CLDR <c>many</c>
/// category requires v ≠ 0 (non-integer operands).
/// See <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/plurals.xml">CLDR plurals.xml</see>.
/// </remarks>
public readonly struct CzechSlovakCardinal : ICardinalRules<CzechSlovakCardinal>
{
  public static PluralCategory Cardinal(int count) => count switch
  {
    1 => PluralCategory.One,
    >= 2 and <= 4 => PluralCategory.Few,
    _ => PluralCategory.Other,
  };
}
