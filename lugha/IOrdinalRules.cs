// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha;

/// <summary>
/// Type class for CLDR ordinal plural rule resolution.
/// </summary>
public interface IOrdinalRules<TSelf> where TSelf : IOrdinalRules<TSelf>
{
  /// <summary>
  /// Resolves the CLDR ordinal plural category.
  /// Pure function.
  /// </summary>
  /// <param name="count">Non-negative integer count.</param>
  public static abstract OrdinalCategory Ordinal(int count);
}
