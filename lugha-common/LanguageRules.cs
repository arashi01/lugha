// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Common;

/// <summary>
/// Maps language tags to their correct CLDR cardinal/ordinal rule pair.
/// </summary>
/// <remarks>
/// <para>Use <see cref="Resolve"/> to look up the correct rule types for a
/// language tag, or consult the comments below when declaring a locale class:</para>
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
  // en:    OneOtherCardinal       + EnglishOrdinal
  // de:    OneOtherCardinal       + OtherOnlyOrdinal
  // nl:    OneOtherCardinal       + OtherOnlyOrdinal
  // nb:    OneOtherCardinal       + OtherOnlyOrdinal
  // da:    OneOtherCardinal       + OtherOnlyOrdinal
  // sv:    OneOtherCardinal       + SwedishOrdinal
  // fr:    FrenchCardinal         + OneOnlyOrdinal
  // es:    LatinEuropeanCardinal  + SpanishOrdinal
  // it:    LatinEuropeanCardinal  + ItalianOrdinal
  // pt:    FrenchCardinal         + OtherOnlyOrdinal   (Brazilian)
  // pt-PT: LatinEuropeanCardinal  + OtherOnlyOrdinal
  // ca:    LatinEuropeanCardinal  + OtherOnlyOrdinal
  // ro:    RomanianCardinal       + OneOnlyOrdinal
  // ru:    EastSlavicCardinal     + OtherOnlyOrdinal
  // uk:    EastSlavicCardinal     + UkrainianOrdinal
  // pl:    PolishCardinal         + OtherOnlyOrdinal
  // cs:    CzechSlovakCardinal    + OtherOnlyOrdinal
  // sk:    CzechSlovakCardinal    + OtherOnlyOrdinal
  // ar:    ArabicCardinal         + OtherOnlyOrdinal
  // he:    HebrewCardinal         + OtherOnlyOrdinal
  // cy:    WelshCardinal          + WelshOrdinal
  // zh:    OtherOnlyCardinal      + OtherOnlyOrdinal
  // ja:    OtherOnlyCardinal      + OtherOnlyOrdinal
  // ko:    OtherOnlyCardinal      + OtherOnlyOrdinal

  /// <summary>
  /// Resolves the CLDR cardinal and ordinal rule type names for a language tag.
  /// </summary>
  /// <param name="languageTag">
  /// IETF BCP 47 language tag (e.g. <c>"en"</c>, <c>"pt-PT"</c>).
  /// Region subtags are significant only where the base language is ambiguous
  /// (e.g. <c>"pt"</c> vs <c>"pt-PT"</c>).
  /// </param>
  /// <returns>
  /// The <see cref="RulePair"/> for the language, or <see langword="null"/>
  /// if the language tag is not recognised.
  /// </returns>
  public static RulePair? Resolve(string languageTag) => languageTag switch
  {
    "en" => new("OneOtherCardinal", "EnglishOrdinal"),
    "de" => new("OneOtherCardinal", "OtherOnlyOrdinal"),
    "nl" => new("OneOtherCardinal", "OtherOnlyOrdinal"),
    "nb" => new("OneOtherCardinal", "OtherOnlyOrdinal"),
    "da" => new("OneOtherCardinal", "OtherOnlyOrdinal"),
    "sv" => new("OneOtherCardinal", "SwedishOrdinal"),
    "fr" => new("FrenchCardinal", "OneOnlyOrdinal"),
    "es" => new("LatinEuropeanCardinal", "SpanishOrdinal"),
    "it" => new("LatinEuropeanCardinal", "ItalianOrdinal"),
    "pt" => new("FrenchCardinal", "OtherOnlyOrdinal"),
    "pt-PT" => new("LatinEuropeanCardinal", "OtherOnlyOrdinal"),
    "ca" => new("LatinEuropeanCardinal", "OtherOnlyOrdinal"),
    "ro" => new("RomanianCardinal", "OneOnlyOrdinal"),
    "ru" => new("EastSlavicCardinal", "OtherOnlyOrdinal"),
    "uk" => new("EastSlavicCardinal", "UkrainianOrdinal"),
    "pl" => new("PolishCardinal", "OtherOnlyOrdinal"),
    "cs" => new("CzechSlovakCardinal", "OtherOnlyOrdinal"),
    "sk" => new("CzechSlovakCardinal", "OtherOnlyOrdinal"),
    "ar" => new("ArabicCardinal", "OtherOnlyOrdinal"),
    "he" => new("HebrewCardinal", "OtherOnlyOrdinal"),
    "cy" => new("WelshCardinal", "WelshOrdinal"),
    "zh" => new("OtherOnlyCardinal", "OtherOnlyOrdinal"),
    "ja" => new("OtherOnlyCardinal", "OtherOnlyOrdinal"),
    "ko" => new("OtherOnlyCardinal", "OtherOnlyOrdinal"),
    _ => null,
  };
}

/// <summary>
/// Cardinal and ordinal rule type names for a language.
/// </summary>
/// <param name="Cardinal">
/// Unqualified type name of the cardinal rules implementation
/// (e.g. <c>"OneOtherCardinal"</c>).
/// </param>
/// <param name="Ordinal">
/// Unqualified type name of the ordinal rules implementation
/// (e.g. <c>"EnglishOrdinal"</c>).
/// </param>
public readonly record struct RulePair(string Cardinal, string Ordinal);
