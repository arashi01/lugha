// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha;

/// <summary>
/// Type class for CLDR cardinal plural rule resolution.
/// Implementations are zero-size structs. The JIT monomorphises generic
/// call sites - no virtual dispatch, no allocation.
/// </summary>
/// <remarks>
/// Cardinal rules for integer inputs. The full CLDR rule system uses
/// operands (n, i, v, w, f, t) for decimal numbers; this interface
/// covers the integer subset (v=0, f=0, t=0, n=i=count). Decimal
/// support is a post-v1.0 candidate.
/// </remarks>
public interface ICardinalRules<TSelf> where TSelf : ICardinalRules<TSelf>
{
  /// <summary>
  /// Resolves the CLDR cardinal plural category.
  /// Pure function.
  /// </summary>
  /// <param name="count">Non-negative integer count.</param>
  public static abstract PluralCategory Cardinal(int count);
}
