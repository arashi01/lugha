// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules.Ordinals;

/// <summary>
/// Ordinal: one → n % 10 = 1 ∧ n % 100 ≠ 11;
///   two → n % 10 = 2 ∧ n % 100 ≠ 12;
///   few → n % 10 = 3 ∧ n % 100 ≠ 13;
///   other → everything else.
/// Applies to: en.
/// </summary>
/// <remarks>
/// See <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/ordinals.xml">CLDR ordinals.xml</see>.
/// </remarks>
public readonly struct EnglishOrdinal : IOrdinalRules<EnglishOrdinal>
{
  public static OrdinalCategory Ordinal(int count)
  {
    int mod10 = count % 10;
    int mod100 = count % 100;
    return mod10 switch
    {
      1 when mod100 is not 11 => OrdinalCategory.One,
      2 when mod100 is not 12 => OrdinalCategory.Two,
      3 when mod100 is not 13 => OrdinalCategory.Few,
      _ => OrdinalCategory.Other,
    };
  }
}
