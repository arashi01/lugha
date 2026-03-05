// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules.Cardinals;

/// <summary>
/// Cardinal: one → i = 1 ∧ v = 0;
///   few → v ≠ 0 ∨ n = 0 ∨ (n ≠ 1 ∧ n % 100 in 1..19);
///   other → everything else.
/// For integer inputs (v = 0): one → 1; few → 0 or count % 100 in 1..19 (count ≠ 1);
///   other → everything else.
/// Applies to: ro.
/// </summary>
/// <remarks>
/// See <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/plurals.xml">CLDR plurals.xml</see>.
/// </remarks>
public readonly struct RomanianCardinal : ICardinalRules<RomanianCardinal>
{
  public static PluralCategory Cardinal(int count)
  {
    if (count == 1)
    {
      return PluralCategory.One;
    }

    int mod100 = count % 100;
    if (count == 0 || mod100 is >= 1 and <= 19)
    {
      return PluralCategory.Few;
    }

    return PluralCategory.Other;
  }
}
