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
    /// <summary>Whether this locale's writing system is right-to-left.</summary>
    public bool IsRightToLeft => locale.Culture.TextInfo.IsRightToLeft;

    /// <summary>
    /// Resolves the cardinal plural form for <paramref name="count"/>
    /// using this locale's cardinal rules.
    /// </summary>
    /// <param name="count">Integer count. Negative values are clamped to zero.</param>
    /// <param name="forms">Plural form strings keyed by CLDR category.</param>
    public string PluralSelect(int count, PluralForms forms) =>
        Plural.Select(count, forms, locale);

    /// <summary>
    /// Resolves the ordinal suffix for <paramref name="count"/>
    /// using this locale's ordinal rules.
    /// </summary>
    /// <param name="count">Integer count. Negative values are clamped to zero.</param>
    /// <param name="forms">Ordinal suffix strings keyed by CLDR category.</param>
    public string OrdinalSelect(int count, OrdinalForms forms) =>
        Ordinal.Select(count, forms, locale);

    /// <summary>
    /// Formats count + noun form as <c>"{count:N0} {form}"</c> using this
    /// locale's cardinal rules and culture. For languages that require
    /// different word order or no space, use
    /// <see cref="PluralSelect"/> with custom interpolation instead.
    /// </summary>
    /// <param name="count">Integer count. Negative values are clamped to zero.</param>
    /// <param name="forms">Plural form strings keyed by CLDR category.</param>
    public string PluralFormat(int count, PluralForms forms) =>
        Plural.Format(count, forms, locale);

    /// <summary>
    /// Formats ordinal + suffix as <c>"{count:N0}{suffix}"</c> (no space) using
    /// this locale's ordinal rules and culture. For languages that require
    /// different composition, use <see cref="OrdinalSelect"/> with custom
    /// interpolation instead.
    /// </summary>
    /// <param name="count">Integer count. Negative values are clamped to zero.</param>
    /// <param name="forms">Ordinal suffix strings keyed by CLDR category.</param>
    public string OrdinalFormat(int count, OrdinalForms forms) =>
        Ordinal.Format(count, forms, locale);
  }
}
