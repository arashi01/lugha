// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha;

/// <summary>CLDR cardinal plural category.</summary>
public enum PluralCategory
{
  /// <summary>General form. The only category every language uses.</summary>
  Other,

  /// <summary>Zero-quantity form. Used by Arabic, Welsh, and others.</summary>
  Zero,

  /// <summary>Singular form. Used by most languages with singular/plural distinction.</summary>
  One,

  /// <summary>Dual form. Used by Arabic, Welsh, and others for exactly two.</summary>
  Two,

  /// <summary>Paucal form. Used by Arabic, Polish, Czech, and others for small quantities.</summary>
  Few,

  /// <summary>Large-quantity form. Used by Arabic, Polish, and others.</summary>
  Many,
}
