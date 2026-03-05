// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Collections.Frozen;

namespace Lugha.WinUI;

/// <summary>
/// Maps language tags to pre-constructed locale instances.
/// Immutable after construction. Thread-safe.
/// </summary>
/// <remarks>
/// Uses <see cref="FrozenDictionary{TKey,TValue}"/> for optimal
/// read performance. Language tag comparison is case-insensitive
/// per BCP 47.
/// </remarks>
/// <typeparam name="TLocale">
/// The composite locale interface (e.g. <c>IAppLocale</c>).
/// </typeparam>
public sealed class LocaleRegistry<TLocale>
    where TLocale : class, ILocale
{
  private readonly FrozenDictionary<string, TLocale> _locales;

  /// <summary>
  /// Initialises a new <see cref="LocaleRegistry{TLocale}"/> from the
  /// specified locale instances.
  /// </summary>
  /// <param name="locales">
  /// The locales to register. Each locale's
  /// <see cref="ILocale.Culture"/> name is used as the lookup key.
  /// </param>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="locales"/> is <see langword="null"/>.
  /// </exception>
  public LocaleRegistry(IEnumerable<TLocale> locales)
  {
    ArgumentNullException.ThrowIfNull(locales);
    _locales = locales.ToFrozenDictionary(
        l => l.Culture.Name,
        StringComparer.OrdinalIgnoreCase);
  }

  /// <summary>
  /// Available language tags.
  /// </summary>
  public IEnumerable<string> Languages => _locales.Keys;

  /// <summary>
  /// Resolves a locale by language tag.
  /// Returns <see langword="null"/> if not found - caller decides fallback policy.
  /// </summary>
  /// <param name="language">The BCP 47 language tag to look up.</param>
  /// <returns>
  /// The matching locale, or <see langword="null"/> if no locale is
  /// registered for the specified tag.
  /// </returns>
  public TLocale? Resolve(string language) =>
      _locales.GetValueOrDefault(language);
}
