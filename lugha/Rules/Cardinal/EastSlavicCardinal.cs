// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules.Cardinal;

/// <summary>
/// Cardinal: one → v = 0 ∧ i % 10 = 1 ∧ i % 100 ≠ 11;
///   few → v = 0 ∧ i % 10 in 2..4 ∧ i % 100 not in 12..14;
///   many → v = 0 ∧ (i % 10 = 0 ∨ i % 10 in 5..9 ∨ i % 100 in 11..14);
///   other → everything else.
/// Applies to: ru, uk.
/// </summary>
/// <remarks>
/// For integer counts, only one/few/many are reachable; the CLDR <c>other</c>
/// category applies to non-integer operands only.
/// See <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/plurals.xml">CLDR plurals.xml</see>.
/// </remarks>
public readonly struct EastSlavicCardinal : ICardinalRules<EastSlavicCardinal>
{
  public static PluralCategory Cardinal(int count)
  {
    int mod10 = count % 10;
    int mod100 = count % 100;

    if (mod10 == 1 && mod100 != 11)
    {
      return PluralCategory.One;
    }

    if (mod10 is >= 2 and <= 4 && mod100 is not (>= 12 and <= 14))
    {
      return PluralCategory.Few;
    }

    if (mod10 == 0 || mod10 is >= 5 and <= 9 || mod100 is >= 11 and <= 14)
    {
      return PluralCategory.Many;
    }

    return PluralCategory.Other;
  }
}
