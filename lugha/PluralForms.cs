// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Diagnostics.CodeAnalysis;

namespace Lugha;

/// <summary>
/// CLDR plural forms for a countable noun. Value type. No virtual dispatch.
/// <see cref="Other"/> is the only required slot. Unset categories
/// resolve to <see cref="Other"/> — the public API is null-free.
/// Equality compares resolved values.
/// </summary>
public readonly record struct PluralForms
{
  /// <summary>Required. General/default form.</summary>
  public required string Other { get; init; }

  /// <summary>Zero-quantity form. Resolves to <see cref="Other"/> when unset.</summary>
  [field: MaybeNull]
  public string Zero { get => field ?? Other; init; }

  /// <summary>Singular form. Resolves to <see cref="Other"/> when unset.</summary>
  [field: MaybeNull]
  public string One { get => field ?? Other; init; }

  /// <summary>Dual form. Resolves to <see cref="Other"/> when unset.</summary>
  [field: MaybeNull]
  public string Two { get => field ?? Other; init; }

  /// <summary>Paucal form. Resolves to <see cref="Other"/> when unset.</summary>
  [field: MaybeNull]
  public string Few { get => field ?? Other; init; }

  /// <summary>Large-quantity form. Resolves to <see cref="Other"/> when unset.</summary>
  [field: MaybeNull]
  public string Many { get => field ?? Other; init; }

  /// <summary>Resolved-value equality.</summary>
  public bool Equals(PluralForms other) =>
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
