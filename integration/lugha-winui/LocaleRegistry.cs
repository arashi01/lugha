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
  /// Resolves a locale by BCP 47 language tag with subtag fallback.
  /// Strips subtags right-to-left until a match is found.
  /// Returns <see langword="null"/> if no ancestor tag is registered.
  /// </summary>
  /// <param name="language">The BCP 47 language tag to look up.</param>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="language"/> is <see langword="null"/>.
  /// </exception>
  public TLocale? Resolve(string language)
  {
    ArgumentNullException.ThrowIfNull(language);
    if (_locales.TryGetValue(language, out TLocale? locale))
    {
      return locale;
    }

    ReadOnlySpan<char> tag = language.AsSpan();
    while (true)
    {
      int lastHyphen = tag.LastIndexOf('-');
      if (lastHyphen <= 0)
      {
        return null;
      }

      tag = tag[..lastHyphen];
      if (_locales.TryGetValue(tag.ToString(), out locale))
      {
        return locale;
      }
    }
  }

  /// <summary>
  /// Resolves a locale by BCP 47 language tag with subtag fallback.
  /// Returns <paramref name="fallback"/> if no match is found.
  /// </summary>
  /// <param name="language">The BCP 47 language tag to look up.</param>
  /// <param name="fallback">The locale to return when no match is found.</param>
  /// <returns>The matched locale, or <paramref name="fallback"/> if not registered.</returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="language"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="fallback"/> is <see langword="null"/>.
  /// </exception>
  public TLocale Resolve(string language, TLocale fallback)
  {
    ArgumentNullException.ThrowIfNull(fallback);
    return Resolve(language) ?? fallback;
  }
}
