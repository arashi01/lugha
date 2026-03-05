// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha;

/// <summary>CLDR ordinal plural category.</summary>
public enum OrdinalCategory
{
  /// <summary>General form (e.g. English "th").</summary>
  Other,

  /// <summary>Zero-quantity ordinal. Used by Welsh and others.</summary>
  Zero,

  /// <summary>First-like ordinal (e.g. English "st" for 1st, 21st).</summary>
  One,

  /// <summary>Second-like ordinal (e.g. English "nd" for 2nd, 22nd).</summary>
  Two,

  /// <summary>Third-like ordinal (e.g. English "rd" for 3rd, 23rd).</summary>
  Few,

  /// <summary>Large-quantity ordinal. Used by Italian, Welsh, and others.</summary>
  Many,
}
