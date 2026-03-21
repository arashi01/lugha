// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Globalization;

namespace Lugha;

/// <summary>
/// Pure functions for ordinal category resolution and formatting.
/// </summary>
/// <remarks>
/// <para><b>Choosing an API:</b></para>
/// <list type="bullet">
///   <item><b>Select</b> - returns only the resolved suffix string. Use this
///     as the general-purpose API for all languages. Compose the count and
///     suffix in your own interpolated string to control composition.</item>
///   <item><b>Format</b> - convenience that produces <c>"{count:N0}{suffix}"</c>
///     (no space). Suitable for languages where the count directly precedes
///     the suffix (e.g. English <c>"1st"</c>). Not appropriate for all languages.</item>
/// </list>
/// <para>
/// All methods are total functions. Negative <c>count</c> values are
/// clamped to zero.
/// </para>
/// </remarks>
public static class Ordinal
{
  // ---- Generic path -----------------------------------------------

  /// <summary>
  /// Resolves the ordinal suffix for <paramref name="count"/>
  /// using the rule type <typeparamref name="TRules"/>.
  /// </summary>
  /// <typeparam name="TRules">
  /// A CLDR ordinal rule implementation (e.g.
  /// <c>EnglishOrdinal</c>, <c>WelshOrdinal</c>).
  /// </typeparam>
  /// <param name="count">Integer count. Negative values are clamped to zero.</param>
  /// <param name="forms">Ordinal suffix strings keyed by CLDR category.</param>
  public static string Select<TRules>(int count, OrdinalForms forms)
      where TRules : IOrdinalRules<TRules>
  {
    int c = Math.Max(0, count);
    return TRules.Ordinal(c).Select(forms);
  }

  /// <summary>
  /// Formats <paramref name="count"/> and the resolved ordinal suffix as
  /// <c>"{count}{suffix}"</c> (no space) using <paramref name="culture"/>
  /// for number formatting.
  /// </summary>
  /// <typeparam name="TRules">A CLDR ordinal rule implementation.</typeparam>
  /// <param name="count">Integer count. Negative values are clamped to zero.</param>
  /// <param name="forms">Ordinal suffix strings keyed by CLDR category.</param>
  /// <param name="culture">Culture for number formatting.</param>
  public static string Format<TRules>(
      int count, OrdinalForms forms, CultureInfo culture)
      where TRules : IOrdinalRules<TRules>
  {
    int c = Math.Max(0, count);
    return $"{c.ToString("N0", culture)}{TRules.Ordinal(c).Select(forms)}";
  }

  /// <summary>
  /// Attempts to write <c>"{count}{suffix}"</c> (no space) into
  /// <paramref name="destination"/>.
  /// </summary>
  /// <typeparam name="TRules">A CLDR ordinal rule implementation.</typeparam>
  /// <param name="count">Integer count. Negative values are clamped to zero.</param>
  /// <param name="forms">Ordinal suffix strings keyed by CLDR category.</param>
  /// <param name="culture">Culture for number formatting.</param>
  /// <param name="destination">Target buffer.</param>
  /// <param name="written">Number of characters written on success.</param>
  /// <returns>
  /// <see langword="true"/> if the formatted text fits;
  /// <see langword="false"/> if <paramref name="destination"/> is too small.
  /// </returns>
  public static bool TryFormat<TRules>(
      int count,
      OrdinalForms forms,
      CultureInfo culture,
      Span<char> destination,
      out int written)
      where TRules : IOrdinalRules<TRules>
  {
    int c = Math.Max(0, count);
    return destination.TryWrite(
        culture,
        $"{c:N0}{TRules.Ordinal(c).Select(forms)}",
        out written);
  }

  // ---- Locale path ------------------------------------------------

  /// <summary>
  /// Resolves the ordinal suffix for <paramref name="count"/>
  /// using the rules bound to <paramref name="locale"/>.
  /// </summary>
  /// <param name="count">Integer count. Negative values are clamped to zero.</param>
  /// <param name="forms">Ordinal suffix strings keyed by CLDR category.</param>
  /// <param name="locale">Locale providing ordinal resolution and culture.</param>
  public static string Select(int count, OrdinalForms forms, ILocale locale)
  {
    int c = Math.Max(0, count);
    return locale.Ordinal(c).Select(forms);
  }

  /// <summary>
  /// Formats <paramref name="count"/> and the resolved ordinal suffix as
  /// <c>"{count}{suffix}"</c> (no space) using the locale's culture
  /// for number formatting.
  /// </summary>
  /// <param name="count">Integer count. Negative values are clamped to zero.</param>
  /// <param name="forms">Ordinal suffix strings keyed by CLDR category.</param>
  /// <param name="locale">Locale providing ordinal resolution and culture.</param>
  public static string Format(int count, OrdinalForms forms, ILocale locale)
  {
    int c = Math.Max(0, count);
    return $"{c.ToString("N0", locale.Culture)}{locale.Ordinal(c).Select(forms)}";
  }

  /// <summary>
  /// Attempts to write <c>"{count}{suffix}"</c> (no space) into
  /// <paramref name="destination"/> using the locale's culture
  /// for number formatting.
  /// </summary>
  /// <param name="count">Integer count. Negative values are clamped to zero.</param>
  /// <param name="forms">Ordinal suffix strings keyed by CLDR category.</param>
  /// <param name="locale">Locale providing ordinal resolution and culture.</param>
  /// <param name="destination">Target buffer.</param>
  /// <param name="written">Number of characters written on success.</param>
  /// <returns>
  /// <see langword="true"/> if the formatted text fits;
  /// <see langword="false"/> if <paramref name="destination"/> is too small.
  /// </returns>
  public static bool TryFormat(
      int count,
      OrdinalForms forms,
      ILocale locale,
      Span<char> destination,
      out int written)
  {
    int c = Math.Max(0, count);
    return destination.TryWrite(
        locale.Culture,
        $"{c:N0}{locale.Ordinal(c).Select(forms)}",
        out written);
  }
}
