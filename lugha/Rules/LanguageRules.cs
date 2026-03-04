// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Rules;

/// <summary>
/// Reference mapping of language tags to their correct CLDR cardinal/ordinal
/// rule pair.
/// </summary>
/// <remarks>
/// <para>Use this mapping when declaring a locale class:</para>
/// <code>
/// public sealed class EnGbLocale
///     : IAppLocale, ILocale&lt;OneOtherCardinal, EnglishOrdinal&gt; { ... }
/// </code>
/// <para>
/// Verified against CLDR
/// <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/plurals.xml">plurals.xml</see> and
/// <see href="https://github.com/unicode-org/cldr/blob/f7e8edbd1838e518932531fd24d0cd7025d77bf4/common/supplemental/ordinals.xml">ordinals.xml</see>.
/// </para>
/// </remarks>
public static class LanguageRules
{
  // en:    OneOtherCardinal    + EnglishOrdinal
  // de:    OneOtherCardinal    + OtherOnlyOrdinal
  // nl:    OneOtherCardinal    + OtherOnlyOrdinal
  // nb:    OneOtherCardinal    + OtherOnlyOrdinal
  // da:    OneOtherCardinal    + OtherOnlyOrdinal
  // sv:    OneOtherCardinal    + SwedishOrdinal
  // fr:    FrenchCardinal      + OneOnlyOrdinal
  // es:    OneOtherCardinal    + SpanishOrdinal
  // it:    OneOtherCardinal    + ItalianOrdinal
  // pt:    FrenchCardinal      + OtherOnlyOrdinal   (Brazilian)
  // pt-PT: OneOtherCardinal    + OtherOnlyOrdinal
  // ro:    RomanianCardinal    + OneOnlyOrdinal
  // ru:    EastSlavicCardinal  + OtherOnlyOrdinal
  // uk:    EastSlavicCardinal  + UkrainianOrdinal
  // pl:    PolishCardinal      + OtherOnlyOrdinal
  // cs:    CzechSlovakCardinal + OtherOnlyOrdinal
  // sk:    CzechSlovakCardinal + OtherOnlyOrdinal
  // ar:    ArabicCardinal      + OtherOnlyOrdinal
  // he:    HebrewCardinal      + OtherOnlyOrdinal
  // cy:    WelshCardinal       + WelshOrdinal
  // zh:    OtherOnlyCardinal   + OtherOnlyOrdinal
  // ja:    OtherOnlyCardinal   + OtherOnlyOrdinal
  // ko:    OtherOnlyCardinal   + OtherOnlyOrdinal
}
