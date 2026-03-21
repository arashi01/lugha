// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

#pragma warning disable IDE0130 // Namespace intentionally Lugha for discoverability
namespace Lugha;
#pragma warning restore IDE0130

/// <summary>Extensions on <see cref="Result{TValue,TError}"/>.</summary>
#pragma warning disable CA1034 // C# 14 extension block compiles as nested type
public static class ResultExtensions
{
  extension<TValue, TError>(Result<TValue, TError> result)
  {
    /// <summary>Exhaustively fold both cases into a single value.</summary>
    public TOut Match<TOut>(Func<TValue, TOut> ok, Func<TError, TOut> err)
    {
      if (result is Result<TValue, TError>.Ok(var v))
      {
        return ok(v);
      }

      return err(((Result<TValue, TError>.Err)result).Error);
    }

    /// <summary>Exhaustively handle both cases via side-effecting actions.</summary>
    public void Switch(Action<TValue> ok, Action<TError> err)
    {
      if (result is Result<TValue, TError>.Ok(var v))
      {
        ok(v);
      }
      else
      {
        err(((Result<TValue, TError>.Err)result).Error);
      }
    }

    /// <summary>Returns the value if Ok, otherwise <paramref name="fallback"/>.</summary>
    public TValue Or(TValue fallback) =>
        result is Result<TValue, TError>.Ok(var v) ? v : fallback;

    /// <summary>Returns the value if Ok, otherwise computes a fallback from the error.</summary>
    public TValue Or(Func<TError, TValue> fallback)
    {
      if (result is Result<TValue, TError>.Ok(var v))
      {
        return v;
      }

      return fallback(((Result<TValue, TError>.Err)result).Error);
    }

    /// <summary>Transforms the success value, preserving errors unchanged.</summary>
    public Result<TResult, TError> Map<TResult>(Func<TValue, TResult> f)
    {
      if (result is Result<TValue, TError>.Ok(var v))
      {
        return f(v);
      }

      return ((Result<TValue, TError>.Err)result).Error;
    }
  }
}
