// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules.Cardinals;

/// <summary>
/// Cardinal: one → i = 0 or 1; other → everything else.
/// Applies to: fr, pt (Brazilian).
/// </summary>
/// <remarks>
/// See <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/plurals.xml">CLDR plurals.xml</see>.
/// </remarks>
public readonly struct FrenchCardinal : ICardinalRules<FrenchCardinal>
{
  public static PluralCategory Cardinal(int count) =>
      count is 0 or 1 ? PluralCategory.One : PluralCategory.Other;
}
