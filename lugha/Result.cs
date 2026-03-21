// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha;

/// <summary>
/// Discriminated union for fallible operations. Closed hierarchy —
/// exhaustive pattern matching is compiler-verified.
/// </summary>
/// <remarks>
/// Implicit conversions from both <typeparamref name="TValue"/> and
/// <typeparamref name="TError"/> are provided. When the two type parameters
/// are the same type, the implicit conversions are ambiguous — construct
/// explicitly via <c>new Result&lt;T, T&gt;.Ok(value)</c> or
/// <c>new Result&lt;T, T&gt;.Err(error)</c>.
/// </remarks>
/// <typeparam name="TValue">Success value type.</typeparam>
/// <typeparam name="TError">Error value type.</typeparam>
#pragma warning disable CA1034 // Sealed hierarchy requires nested subtypes
public abstract record Result<TValue, TError>
{
  private protected Result() { }

  /// <summary>Successful outcome carrying <paramref name="Value"/>.</summary>
  public sealed record Ok(TValue Value) : Result<TValue, TError>;

  /// <summary>Failed outcome carrying <paramref name="Error"/>.</summary>
  public sealed record Err(TError Error) : Result<TValue, TError>;

  /// <summary>Implicit conversion from a success value.</summary>
  public static implicit operator Result<TValue, TError>(TValue value) => new Ok(value);

  /// <summary>Implicit conversion from an error value.</summary>
  public static implicit operator Result<TValue, TError>(TError error) => new Err(error);
}
#pragma warning restore CA1034
