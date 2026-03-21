// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Diagnostics.CodeAnalysis;

namespace Lugha;

/// <summary>
/// CLDR ordinal forms. Same structure as <see cref="PluralForms"/>,
/// separate type for compile-time discrimination.
/// <see cref="Other"/> is the only required slot. Unset categories
/// resolve to <see cref="Other"/> — the public API is null-free.
/// Equality compares resolved values.
/// </summary>
public readonly record struct OrdinalForms
{
  /// <summary>Required. General/default form (e.g. "th").</summary>
  public required string Other { get; init; }

  /// <summary>Zero-quantity ordinal form. Resolves to <see cref="Other"/> when unset.</summary>
  [field: MaybeNull]
  public string Zero { get => field ?? Other; init; }

  /// <summary>E.g. "st" (1st, 21st). Resolves to <see cref="Other"/> when unset.</summary>
  [field: MaybeNull]
  public string One { get => field ?? Other; init; }

  /// <summary>E.g. "nd" (2nd, 22nd). Resolves to <see cref="Other"/> when unset.</summary>
  [field: MaybeNull]
  public string Two { get => field ?? Other; init; }

  /// <summary>E.g. "rd" (3rd, 23rd). Resolves to <see cref="Other"/> when unset.</summary>
  [field: MaybeNull]
  public string Few { get => field ?? Other; init; }

  /// <summary>Large-quantity ordinal form. Resolves to <see cref="Other"/> when unset.</summary>
  [field: MaybeNull]
  public string Many { get => field ?? Other; init; }

  /// <summary>Resolved-value equality.</summary>
  public bool Equals(OrdinalForms other) =>
      Other == other.Other &&
      Zero == other.Zero &&
      One == other.One &&
      Two == other.Two &&
      Few == other.Few &&
      Many == other.Many;

  /// <summary>Resolved-value hash code.</summary>
  public override int GetHashCode() =>
      HashCode.Combine(Other, Zero, One, Two, Few, Many);
}
