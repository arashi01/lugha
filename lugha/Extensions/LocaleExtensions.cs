// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

#pragma warning disable IDE0130 // Namespace intentionally Lugha for discoverability
namespace Lugha;
#pragma warning restore IDE0130

/// <summary>Convenience extensions on <see cref="ILocale"/>.</summary>
#pragma warning disable CA1034 // C# 14 extension block compiles as nested type
public static class LocaleExtensions
{
  extension(ILocale locale)
  {
    /// <summary>
    /// Resolves the cardinal plural form for <paramref name="count"/>
    /// using this locale's cardinal rules.
    /// Equivalent to <c>Plural.Select(count, forms, locale)</c>.
    /// </summary>
    /// <param name="count">Non-negative item count.</param>
    /// <param name="forms">Plural form strings keyed by CLDR category.</param>
    /// <returns>The resolved plural form string (e.g. <c>"item"</c> or <c>"items"</c>).</returns>
    public string PluralSelect(int count, PluralForms forms) =>
        Plural.Select(count, forms, locale);

    /// <summary>
    /// Resolves the ordinal suffix for <paramref name="count"/>
    /// using this locale's ordinal rules.
    /// Equivalent to <c>Ordinal.Select(count, forms, locale)</c>.
    /// </summary>
    /// <param name="count">Non-negative ordinal position.</param>
    /// <param name="forms">Ordinal suffix strings keyed by CLDR category.</param>
    /// <returns>The resolved ordinal suffix string (e.g. <c>"st"</c> or <c>"th"</c>).</returns>
    public string OrdinalSelect(int count, OrdinalForms forms) =>
        Ordinal.Select(count, forms, locale);

    /// <summary>
    /// Formats count + noun form as <c>"{count:N0} {form}"</c> using this
    /// locale's cardinal rules and culture. This is a convenience for
    /// the common <c>"{count} {form}"</c> pattern. For languages that
    /// require different word order or no space, use
    /// <see cref="PluralSelect"/> with custom interpolation instead.
    /// Equivalent to <c>Plural.Format(count, forms, locale)</c>.
    /// </summary>
    /// <param name="count">Non-negative item count.</param>
    /// <param name="forms">Plural form strings keyed by CLDR category.</param>
    /// <returns>Formatted string such as <c>"1 item"</c> or <c>"5 items"</c>.</returns>
    public string PluralFormat(int count, PluralForms forms) =>
        Plural.Format(count, forms, locale);

    /// <summary>
    /// Formats ordinal + suffix as <c>"{count:N0}{suffix}"</c> (no space) using
    /// this locale's ordinal rules and culture. For languages that require
    /// different composition, use <see cref="OrdinalSelect"/> with custom
    /// interpolation instead.
    /// Equivalent to <c>Ordinal.Format(count, forms, locale)</c>.
    /// </summary>
    /// <param name="count">Non-negative ordinal position.</param>
    /// <param name="forms">Ordinal suffix strings keyed by CLDR category.</param>
    /// <returns>Formatted string such as <c>"1st"</c> or <c>"3rd"</c>.</returns>
    public string OrdinalFormat(int count, OrdinalForms forms) =>
        Ordinal.Format(count, forms, locale);
  }
}
