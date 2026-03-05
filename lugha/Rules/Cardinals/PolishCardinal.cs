// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules.Cardinals;

/// <summary>
/// Cardinal: one → i = 1 ∧ v = 0;
///   few → v = 0 ∧ i % 10 in 2..4 ∧ i % 100 not in 12..14;
///   many → v = 0 ∧ (i ≠ 1) ∧ (i % 10 in 0..1 ∨ i % 10 in 5..9 ∨ i % 100 in 12..14);
///   other → everything else.
/// Applies to: pl.
/// </summary>
/// <remarks>
/// For integer counts, only one/few/many are reachable; the CLDR <c>other</c>
/// category applies to non-integer operands only.
/// See <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/plurals.xml">CLDR plurals.xml</see>.
/// </remarks>
public readonly struct PolishCardinal : ICardinalRules<PolishCardinal>
{
  public static PluralCategory Cardinal(int count)
  {
    if (count == 1)
    {
      return PluralCategory.One;
    }

    int mod10 = count % 10;
    int mod100 = count % 100;

    if (mod10 is >= 2 and <= 4 && mod100 is not (>= 12 and <= 14))
    {
      return PluralCategory.Few;
    }

    return PluralCategory.Many;
  }
}
