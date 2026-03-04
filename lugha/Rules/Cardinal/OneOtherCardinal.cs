// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules.Cardinal;

/// <summary>
/// Cardinal: one → i = 1 ∧ v = 0; other → everything else.
/// Applies to: en, de, nl, sv, it, pt-PT, es (for integer inputs).
/// </summary>
/// <remarks>
/// See <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/plurals.xml">CLDR plurals.xml</see>.
/// </remarks>
public readonly struct OneOtherCardinal : ICardinalRules<OneOtherCardinal>
{
  public static PluralCategory Cardinal(int count) =>
      count == 1 ? PluralCategory.One : PluralCategory.Other;
}
