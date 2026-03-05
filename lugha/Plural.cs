// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System;
using System.Globalization;

namespace Lugha;

/// <summary>
/// Pure functions for cardinal plural category resolution and formatting.
/// </summary>
/// <remarks>
/// <para><b>Choosing an API:</b></para>
/// <list type="bullet">
///   <item><b>Select</b> — returns only the resolved form string. Use this
///     as the general-purpose API for all languages. Compose the count and
///     form in your own interpolated string to control word order, spacing,
///     and grammatical structure.</item>
///   <item><b>Format</b> — convenience that produces <c>"{count:N0} {form}"</c>.
///     Suitable for languages where the count precedes the noun with a space
///     (e.g. English <c>"5 items"</c>). Not appropriate for languages with
///     different word order, no space, or case inflection around the number.</item>
/// </list>
/// <para><b>Choosing a path:</b></para>
/// <list type="bullet">
///   <item><b>Generic path</b> (<c>Select&lt;TRules&gt;</c>, <c>Format&lt;TRules&gt;</c>):
///     Use in locale-specific scope implementations where the rule type is
///     statically known. Zero virtual dispatch.</item>
///   <item><b>Locale path</b> (<c>Select(..., locale)</c>, <c>Format(..., locale)</c>):
///     Use in locale-agnostic components receiving <see cref="ILocale"/>.</item>
/// </list>
/// <para>
/// All methods guard against negative <c>count</c> via
/// <see cref="ArgumentOutOfRangeException.ThrowIfNegative"/>.
/// </para>
/// </remarks>
public static class Plural
{
  // ---- Generic path -----------------------------------------------

  /// <summary>
  /// Resolves the cardinal plural form for <paramref name="count"/>
  /// using the rule type <typeparamref name="TRules"/>.
  /// </summary>
  /// <typeparam name="TRules">
  /// A CLDR cardinal rule implementation (e.g.
  /// <c>OneOtherCardinal</c>, <c>EastSlavicCardinal</c>).
  /// </typeparam>
  /// <param name="count">Non-negative item count.</param>
  /// <param name="forms">Plural form strings keyed by CLDR category.</param>
  /// <returns>The resolved plural form string.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="count"/> is negative.
  /// </exception>
  public static string Select<TRules>(int count, PluralForms forms)
      where TRules : ICardinalRules<TRules>
  {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    return TRules.Cardinal(count).Select(forms);
  }

  /// <summary>
  /// Formats <paramref name="count"/> and the resolved plural form as
  /// <c>"{count} {form}"</c> using <paramref name="culture"/> for number formatting.
  /// </summary>
  /// <typeparam name="TRules">A CLDR cardinal rule implementation.</typeparam>
  /// <param name="count">Non-negative item count.</param>
  /// <param name="forms">Plural form strings keyed by CLDR category.</param>
  /// <param name="culture">
  /// Culture for number formatting (e.g. thousands separator).
  /// </param>
  /// <returns>Formatted string such as <c>"1 item"</c> or <c>"5 items"</c>.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="count"/> is negative.
  /// </exception>
  public static string Format<TRules>(
      int count, PluralForms forms, CultureInfo culture)
      where TRules : ICardinalRules<TRules>
  {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    return $"{count.ToString("N0", culture)} {TRules.Cardinal(count).Select(forms)}";
  }

  /// <summary>
  /// Attempts to write <c>"{count} {form}"</c> into <paramref name="destination"/>.
  /// </summary>
  /// <typeparam name="TRules">A CLDR cardinal rule implementation.</typeparam>
  /// <param name="count">Non-negative item count.</param>
  /// <param name="forms">Plural form strings keyed by CLDR category.</param>
  /// <param name="culture">Culture for number formatting.</param>
  /// <param name="destination">Target buffer.</param>
  /// <param name="written">Number of characters written on success.</param>
  /// <returns>
  /// <see langword="true"/> if the formatted text fits;
  /// <see langword="false"/> if <paramref name="destination"/> is too small.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="count"/> is negative.
  /// </exception>
  public static bool TryFormat<TRules>(
      int count,
      PluralForms forms,
      CultureInfo culture,
      Span<char> destination,
      out int written)
      where TRules : ICardinalRules<TRules>
  {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    return destination.TryWrite(
        culture,
        $"{count:N0} {TRules.Cardinal(count).Select(forms)}",
        out written);
  }

  // ---- Locale path ------------------------------------------------

  /// <summary>
  /// Resolves the cardinal plural form for <paramref name="count"/>
  /// using the rules bound to <paramref name="locale"/>.
  /// </summary>
  /// <param name="count">Non-negative item count.</param>
  /// <param name="forms">Plural form strings keyed by CLDR category.</param>
  /// <param name="locale">Locale providing cardinal resolution and culture.</param>
  /// <returns>The resolved plural form string.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="count"/> is negative.
  /// </exception>
  public static string Select(int count, PluralForms forms, ILocale locale)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    return locale.Cardinal(count).Select(forms);
  }

  /// <summary>
  /// Formats <paramref name="count"/> and the resolved plural form as
  /// <c>"{count} {form}"</c> using the locale's culture for number formatting.
  /// </summary>
  /// <param name="count">Non-negative item count.</param>
  /// <param name="forms">Plural form strings keyed by CLDR category.</param>
  /// <param name="locale">Locale providing cardinal resolution and culture.</param>
  /// <returns>Formatted string such as <c>"1 item"</c> or <c>"5 items"</c>.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="count"/> is negative.
  /// </exception>
  public static string Format(int count, PluralForms forms, ILocale locale)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    return $"{count.ToString("N0", locale.Culture)} {locale.Cardinal(count).Select(forms)}";
  }

  /// <summary>
  /// Attempts to write <c>"{count} {form}"</c> into <paramref name="destination"/>
  /// using the locale's culture for number formatting.
  /// </summary>
  /// <param name="count">Non-negative item count.</param>
  /// <param name="forms">Plural form strings keyed by CLDR category.</param>
  /// <param name="locale">Locale providing cardinal resolution and culture.</param>
  /// <param name="destination">Target buffer.</param>
  /// <param name="written">Number of characters written on success.</param>
  /// <returns>
  /// <see langword="true"/> if the formatted text fits;
  /// <see langword="false"/> if <paramref name="destination"/> is too small.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="count"/> is negative.
  /// </exception>
  public static bool TryFormat(
      int count,
      PluralForms forms,
      ILocale locale,
      Span<char> destination,
      out int written)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    return destination.TryWrite(
        locale.Culture,
        $"{count:N0} {locale.Cardinal(count).Select(forms)}",
        out written);
  }
}
