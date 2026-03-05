// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules.Ordinals;

/// <summary>
/// Ordinal: one → n % 10 = 1,3 ∧ n % 100 ≠ 11;
///   other → everything else.
/// Applies to: es.
/// </summary>
/// <remarks>
/// See <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/ordinals.xml">CLDR ordinals.xml</see>.
/// </remarks>
public readonly struct SpanishOrdinal : IOrdinalRules<SpanishOrdinal>
{
  public static OrdinalCategory Ordinal(int count)
  {
    int mod10 = count % 10;
    int mod100 = count % 100;
    return mod10 is 1 or 3 && mod100 is not 11
        ? OrdinalCategory.One
        : OrdinalCategory.Other;
  }
}
