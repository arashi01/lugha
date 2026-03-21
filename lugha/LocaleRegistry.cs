// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Collections.Frozen;

namespace Lugha;

/// <summary>
/// Maps BCP 47 language tags to pre-constructed locale instances.
/// Immutable after construction. Thread-safe.
/// </summary>
/// <typeparam name="TLocale">The composite locale interface.</typeparam>
public sealed class LocaleRegistry<TLocale>
    where TLocale : class, ILocale
{
  private readonly FrozenDictionary<string, TLocale> _locales;

  /// <summary>The default locale, guaranteed registered.</summary>
  public TLocale Default { get; }

  /// <summary>Number of registered locales.</summary>
  public int Count => _locales.Count;

  /// <summary>All registered language tags.</summary>
  public IEnumerable<string> Languages => _locales.Keys;

  /// <summary>All registered locale instances.</summary>
  public IEnumerable<TLocale> Locales => _locales.Values;

  private LocaleRegistry(TLocale defaultLocale, FrozenDictionary<string, TLocale> locales)
  {
    Default = defaultLocale;
    _locales = locales;
  }

  /// <summary>
  /// Creates a registry with <paramref name="defaultLocale"/> and
  /// any <paramref name="additionalLocales"/>.
  /// </summary>
  /// <returns>
  /// <see cref="Result{TValue,TError}.Ok"/> with the registry, or
  /// <see cref="Result{TValue,TError}.Err"/> with the first duplicate tag found.
  /// </returns>
#pragma warning disable CA1000 // Static factory on generic type is the intended creation pattern
  public static Result<LocaleRegistry<TLocale>, DuplicateLanguageTag> Create(
      TLocale defaultLocale,
      params ReadOnlySpan<TLocale> additionalLocales)
  {
    List<TLocale> all = new(additionalLocales.Length + 1) { defaultLocale };
    HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase) { defaultLocale.Culture.Name };

    foreach (TLocale locale in additionalLocales)
    {
      if (!seen.Add(locale.Culture.Name))
      {
        return new DuplicateLanguageTag(locale.Culture.Name);
      }

      all.Add(locale);
    }

    var frozen = all.ToFrozenDictionary(l => l.Culture.Name, StringComparer.OrdinalIgnoreCase);

    return new LocaleRegistry<TLocale>(defaultLocale, frozen);
  }
#pragma warning restore CA1000

  /// <summary>
  /// Resolves by BCP 47 tag with subtag fallback.
  /// Total function - returns <see cref="Default"/> when no match is found.
  /// </summary>
  public TLocale Resolve(string language) => TryResolve(language) ?? Default;

  /// <summary>
  /// Resolves by BCP 47 tag with subtag fallback.
  /// Returns <see langword="null"/> if unregistered.
  /// </summary>
  public TLocale? TryResolve(string language)
  {
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

  /// <summary>Whether a locale matching <paramref name="language"/> is registered.</summary>
  public bool Contains(string language) => TryResolve(language) is not null;
}
