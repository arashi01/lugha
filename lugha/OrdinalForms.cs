// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System;

namespace Lugha;

/// <summary>
/// CLDR ordinal forms. Same structure as <see cref="PluralForms"/>,
/// separate type for compile-time discrimination.
/// </summary>
/// <remarks>
/// See <see cref="PluralForms"/> remarks for equality semantics.
/// </remarks>
public readonly record struct OrdinalForms
{
  /// <summary>Required. General/default form (e.g. "th").</summary>
  public required string Other { get; init; }

  /// <summary>Falls back to <see cref="Other"/>.</summary>
  public string Zero { get => field ?? Other; init; }

  /// <summary>E.g. "st" (1st, 21st). Falls back to <see cref="Other"/>.</summary>
  public string One { get => field ?? Other; init; }

  /// <summary>E.g. "nd" (2nd, 22nd). Falls back to <see cref="Other"/>.</summary>
  public string Two { get => field ?? Other; init; }

  /// <summary>E.g. "rd" (3rd, 23rd). Falls back to <see cref="Other"/>.</summary>
  public string Few { get => field ?? Other; init; }

  /// <summary>Falls back to <see cref="Other"/>.</summary>
  public string Many { get => field ?? Other; init; }

  // Resolved-value equality — see PluralForms.
  public bool Equals(OrdinalForms other) =>
      Other == other.Other &&
      Zero == other.Zero &&
      One == other.One &&
      Two == other.Two &&
      Few == other.Few &&
      Many == other.Many;

  public override int GetHashCode() =>
      HashCode.Combine(Other, Zero, One, Two, Few, Many);
}
