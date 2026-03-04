// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System;
using System.Globalization;

namespace Lugha;

/// <summary>
/// Pure functions for ordinal category resolution and formatting.
/// </summary>
/// <remarks>
/// <para>Ordinal formatting concatenates count and suffix <b>without</b> a
/// separating space (e.g. <c>"1st"</c>, <c>"2nd"</c>), unlike cardinal
/// formatting which inserts a space (<c>"1 item"</c>).</para>
/// <para>
/// All methods guard against negative <c>count</c> via
/// <see cref="ArgumentOutOfRangeException.ThrowIfNegative"/>.
/// </para>
/// </remarks>
#pragma warning disable CA1724 // Type name Ordinal matches namespace Lugha.Rules.Ordinal — both are spec-mandated; no consumer ambiguity arises because the namespace is never imported directly
public static class Ordinal
#pragma warning restore CA1724
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
  /// <param name="count">Non-negative ordinal position.</param>
  /// <param name="forms">Ordinal suffix strings keyed by CLDR category.</param>
  /// <returns>The resolved ordinal suffix string.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="count"/> is negative.
  /// </exception>
  public static string Select<TRules>(int count, OrdinalForms forms)
      where TRules : IOrdinalRules<TRules>
  {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    return TRules.Ordinal(count).Select(forms);
  }

  /// <summary>
  /// Formats <paramref name="count"/> and the resolved ordinal suffix as
  /// <c>"{count}{suffix}"</c> (no space) using <paramref name="culture"/>
  /// for number formatting.
  /// </summary>
  /// <typeparam name="TRules">A CLDR ordinal rule implementation.</typeparam>
  /// <param name="count">Non-negative ordinal position.</param>
  /// <param name="forms">Ordinal suffix strings keyed by CLDR category.</param>
  /// <param name="culture">
  /// Culture for number formatting (e.g. thousands separator).
  /// </param>
  /// <returns>Formatted string such as <c>"1st"</c> or <c>"3rd"</c>.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="count"/> is negative.
  /// </exception>
  public static string Format<TRules>(
      int count, OrdinalForms forms, CultureInfo culture)
      where TRules : IOrdinalRules<TRules>
  {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    return $"{count.ToString(culture)}{TRules.Ordinal(count).Select(forms)}";
  }

  /// <summary>
  /// Attempts to write <c>"{count}{suffix}"</c> (no space) into
  /// <paramref name="destination"/>.
  /// </summary>
  /// <typeparam name="TRules">A CLDR ordinal rule implementation.</typeparam>
  /// <param name="count">Non-negative ordinal position.</param>
  /// <param name="forms">Ordinal suffix strings keyed by CLDR category.</param>
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
      OrdinalForms forms,
      CultureInfo culture,
      Span<char> destination,
      out int written)
      where TRules : IOrdinalRules<TRules>
  {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    return destination.TryWrite(
        culture,
        $"{count}{TRules.Ordinal(count).Select(forms)}",
        out written);
  }

  // ---- Locale path ------------------------------------------------

  /// <summary>
  /// Resolves the ordinal suffix for <paramref name="count"/>
  /// using the rules bound to <paramref name="locale"/>.
  /// </summary>
  /// <param name="count">Non-negative ordinal position.</param>
  /// <param name="forms">Ordinal suffix strings keyed by CLDR category.</param>
  /// <param name="locale">Locale providing ordinal resolution and culture.</param>
  /// <returns>The resolved ordinal suffix string.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="count"/> is negative.
  /// </exception>
  public static string Select(int count, OrdinalForms forms, ILocale locale)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    return locale.Ordinal(count).Select(forms);
  }

  /// <summary>
  /// Formats <paramref name="count"/> and the resolved ordinal suffix as
  /// <c>"{count}{suffix}"</c> (no space) using the locale's culture
  /// for number formatting.
  /// </summary>
  /// <param name="count">Non-negative ordinal position.</param>
  /// <param name="forms">Ordinal suffix strings keyed by CLDR category.</param>
  /// <param name="locale">Locale providing ordinal resolution and culture.</param>
  /// <returns>Formatted string such as <c>"1st"</c> or <c>"3rd"</c>.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="count"/> is negative.
  /// </exception>
  public static string Format(int count, OrdinalForms forms, ILocale locale)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    return $"{count.ToString(locale.Culture)}{locale.Ordinal(count).Select(forms)}";
  }

  /// <summary>
  /// Attempts to write <c>"{count}{suffix}"</c> (no space) into
  /// <paramref name="destination"/> using the locale's culture
  /// for number formatting.
  /// </summary>
  /// <param name="count">Non-negative ordinal position.</param>
  /// <param name="forms">Ordinal suffix strings keyed by CLDR category.</param>
  /// <param name="locale">Locale providing ordinal resolution and culture.</param>
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
      OrdinalForms forms,
      ILocale locale,
      Span<char> destination,
      out int written)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    return destination.TryWrite(
        locale.Culture,
        $"{count}{locale.Ordinal(count).Select(forms)}",
        out written);
  }
}
