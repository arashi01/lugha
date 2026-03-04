// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System;
using System.Globalization;

namespace Lugha;

/// <summary>
/// Locale contract. Composes text scope implementations for a specific
/// language/region. Each property returns a concrete <see cref="ITextScope"/>
/// implementation. Locale instances are constructed once and reused.
/// Text scope implementations must be stateless and pure.
/// </summary>
/// <remarks>
/// All Lugha types are thread-safe. Locale instances may be shared freely
/// across threads. <see cref="Culture"/> must be a read-only instance
/// obtained via <see cref="CultureInfo.GetCultureInfo(string)"/>.
/// </remarks>
public interface ILocale
{
  /// <summary>
  /// Culture associated with this locale.
  /// Explicit parameter — never reads ambient state.
  /// Must be a read-only <see cref="CultureInfo"/> instance.
  /// </summary>
  public CultureInfo Culture { get; }

  /// <summary>
  /// Resolves the CLDR cardinal plural category for a given count.
  /// Pure function.
  /// </summary>
  /// <param name="count">Non-negative integer count.</param>
  public PluralCategory Cardinal(int count);

  /// <summary>
  /// Resolves the CLDR ordinal plural category for a given count.
  /// Pure function.
  /// </summary>
  /// <param name="count">Non-negative integer count.</param>
  public OrdinalCategory Ordinal(int count);
}

/// <summary>
/// Locale bound to independent cardinal and ordinal rule sets.
/// Resolution is provided automatically via default interface methods.
/// Non-negative count is enforced at this boundary.
/// </summary>
public interface ILocale<TCardinal, TOrdinal> : ILocale
    where TCardinal : ICardinalRules<TCardinal>
    where TOrdinal : IOrdinalRules<TOrdinal>
{
#pragma warning disable CA1033 // Default interface method — explicit implementation is the intended pattern
  PluralCategory ILocale.Cardinal(int count)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    return TCardinal.Cardinal(count);
  }

  OrdinalCategory ILocale.Ordinal(int count)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    return TOrdinal.Ordinal(count);
  }
#pragma warning restore CA1033
}
