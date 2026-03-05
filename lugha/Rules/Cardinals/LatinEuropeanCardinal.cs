// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules.Cardinals;

/// <summary>
/// Cardinal: one → i = 1 ∧ v = 0;
///   many → e = 0 ∧ i ≠ 0 ∧ i % 1000000 = 0 ∧ v = 0;
///   other → everything else.
/// Applies to: es, it, pt-PT, ca, lld, scn, vec.
/// </summary>
/// <remarks>
/// <para>For integer inputs (v = 0, e = 0), the <c>many</c> category
/// applies to non-zero exact multiples of 1,000,000.</para>
/// <para>
/// See <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/plurals.xml">CLDR plurals.xml</see>.
/// </para>
/// </remarks>
public readonly struct LatinEuropeanCardinal : ICardinalRules<LatinEuropeanCardinal>
{
  public static PluralCategory Cardinal(int count) => count switch
  {
    1 => PluralCategory.One,
    _ when count != 0 && count % 1_000_000 == 0 => PluralCategory.Many,
    _ => PluralCategory.Other,
  };
}
