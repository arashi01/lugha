// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules.Ordinal;

/// <summary>
/// Ordinal: many → n = 8, 11, 80, 800; other → everything else.
/// Applies to: it.
/// </summary>
/// <remarks>
/// See <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/ordinals.xml">CLDR ordinals.xml</see>.
/// </remarks>
public readonly struct ItalianOrdinal : IOrdinalRules<ItalianOrdinal>
{
  public static OrdinalCategory Ordinal(int count) =>
      count is 8 or 11 or 80 or 800
          ? OrdinalCategory.Many
          : OrdinalCategory.Other;
}
