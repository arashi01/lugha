// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.


namespace Lugha;

/// <summary>
/// CLDR plural forms for a countable noun. Value type. No virtual dispatch.
/// <see cref="Other"/> is the only required slot. All remaining categories
/// fall back to <see cref="Other"/> when unset.
/// </summary>
/// <remarks>
/// <para>Uses C# 14 <c>field</c> keyword. Value equality compares resolved values
/// (post-fallback), not nullable backing fields - two instances with identical
/// resolved text are equal regardless of which slots were explicitly set.
/// The auto-generated <c>ToString</c> calls property getters and therefore
/// also prints resolved values.</para>
/// <para>The custom <c>Equals</c>/<c>GetHashCode</c> overrides ensure
/// resolved-value equality. Record structs generate equality over backing
/// fields by default; overriding is necessary to compare the resolved
/// (post-fallback) values rather than the nullable backing stores.</para>
/// </remarks>
public readonly record struct PluralForms
{
  /// <summary>Required. General/default form.</summary>
  public required string Other { get; init; }

  /// <summary>Zero-quantity. Falls back to <see cref="Other"/>.</summary>
  public string Zero { get => field ?? Other; init; }

  /// <summary>Singular. Falls back to <see cref="Other"/>.</summary>
  public string One { get => field ?? Other; init; }

  /// <summary>Dual. Falls back to <see cref="Other"/>.</summary>
  public string Two { get => field ?? Other; init; }

  /// <summary>Paucal. Falls back to <see cref="Other"/>.</summary>
  public string Few { get => field ?? Other; init; }

  /// <summary>Large-quantity. Falls back to <see cref="Other"/>.</summary>
  public string Many { get => field ?? Other; init; }

  // Resolved-value equality - overrides record-generated field equality.
  // Backing fields are nullable but resolved values are not.
  public bool Equals(PluralForms other) =>
      Other == other.Other &&
      Zero == other.Zero &&
      One == other.One &&
      Two == other.Two &&
      Few == other.Few &&
      Many == other.Many;

  public override int GetHashCode() =>
      HashCode.Combine(Other, Zero, One, Two, Few, Many);
}
