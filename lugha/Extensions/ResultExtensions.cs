// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

#pragma warning disable IDE0130 // Namespace intentionally Lugha for discoverability
namespace Lugha;
#pragma warning restore IDE0130

/// <summary>Extensions on <see cref="Result{TValue,TError}"/>.</summary>
#pragma warning disable CA1034 // C# 14 extension block compiles as nested type
#pragma warning disable CS8509 // Closed hierarchy (private protected ctor) — Ok and Err are exhaustive
public static class ResultExtensions
{
  extension<TValue, TError>(Result<TValue, TError> result)
  {
    /// <summary>Whether this is a successful outcome.</summary>
    public bool IsOk => result is Result<TValue, TError>.Ok;

    /// <summary>Whether this is a failed outcome.</summary>
    public bool IsErr => result is Result<TValue, TError>.Err;

    /// <summary>Exhaustively fold both cases into a single value.</summary>
    public TOut Match<TOut>(Func<TValue, TOut> ok, Func<TError, TOut> err) =>
        result switch
        {
          Result<TValue, TError>.Ok(var v) => ok(v),
          Result<TValue, TError>.Err(var e) => err(e),
        };

    /// <summary>Exhaustively handle both cases via side-effecting actions.</summary>
    public void Switch(Action<TValue> ok, Action<TError> err)
    {
      switch (result)
      {
        case Result<TValue, TError>.Ok(var v):
          ok(v);
          break;
        case Result<TValue, TError>.Err(var e):
          err(e);
          break;
      }
    }

    /// <summary>Returns the value if Ok, otherwise <paramref name="fallback"/>.</summary>
    public TValue Or(TValue fallback) =>
        result switch
        {
          Result<TValue, TError>.Ok(var v) => v,
          Result<TValue, TError>.Err => fallback,
        };

    /// <summary>Returns the value if Ok, otherwise computes a fallback from the error.</summary>
    public TValue Or(Func<TError, TValue> fallback) =>
        result switch
        {
          Result<TValue, TError>.Ok(var v) => v,
          Result<TValue, TError>.Err(var e) => fallback(e),
        };

    /// <summary>Transforms the success value, preserving errors unchanged.</summary>
    public Result<TResult, TError> Map<TResult>(Func<TValue, TResult> f) =>
        result switch
        {
          Result<TValue, TError>.Ok(var v) => f(v),
          Result<TValue, TError>.Err(var e) => e,
        };

    /// <summary>Transforms the error value, preserving successes unchanged.</summary>
    public Result<TValue, TResult> MapError<TResult>(Func<TError, TResult> f) =>
        result switch
        {
          Result<TValue, TError>.Ok(var v) => v,
          Result<TValue, TError>.Err(var e) => f(e),
        };

    /// <summary>Chains a fallible operation on the success value.</summary>
    public Result<TResult, TError> Bind<TResult>(
        Func<TValue, Result<TResult, TError>> f) =>
        result switch
        {
          Result<TValue, TError>.Ok(var v) => f(v),
          Result<TValue, TError>.Err(var e) => e,
        };
  }
}
#pragma warning restore CS8509
