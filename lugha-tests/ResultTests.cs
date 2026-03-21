// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Tests;

/// <summary>
/// Tests <see cref="Result{TValue,TError}"/> ADT, implicit conversions,
/// and all <see cref="ResultExtensions"/> members.
/// </summary>
public sealed class ResultTests
{
  // ---- Construction / implicit conversions ----------------------------

  [Fact]
  public void Ok_carries_value()
  {
    var ok = new Result<int, string>.Ok(42);
    ok.Value.Should().Be(42);
  }

  [Fact]
  public void Err_carries_error()
  {
    var err = new Result<int, string>.Err("fail");
    err.Error.Should().Be("fail");
  }

  [Fact]
  public void Implicit_from_value_produces_Ok()
  {
    Result<int, string> result = 42;
    result.Should().BeOfType<Result<int, string>.Ok>()
        .Which.Value.Should().Be(42);
  }

  [Fact]
  public void Implicit_from_error_produces_Err()
  {
    Result<int, string> result = "fail";
    result.Should().BeOfType<Result<int, string>.Err>()
        .Which.Error.Should().Be("fail");
  }

  // ---- Record equality ------------------------------------------------

  [Fact]
  public void Ok_equality_by_value()
  {
    Result<int, string> a = 42;
    Result<int, string> b = 42;
    a.Should().Be(b);
  }

  [Fact]
  public void Err_equality_by_error()
  {
    Result<int, string> a = "fail";
    Result<int, string> b = "fail";
    a.Should().Be(b);
  }

  [Fact]
  public void Ok_and_Err_are_not_equal()
  {
    // When TValue == TError, implicit conversions are ambiguous — construct explicitly.
    Result<int, int> ok = new Result<int, int>.Ok(1);
    Result<int, int> err = new Result<int, int>.Err(1);
    ok.Should().NotBe(err);
  }

  // ---- IsOk / IsErr ---------------------------------------------------

  [Fact]
  public void IsOk_true_for_Ok()
  {
    Result<int, string> result = 1;
    result.IsOk.Should().BeTrue();
    result.IsErr.Should().BeFalse();
  }

  [Fact]
  public void IsErr_true_for_Err()
  {
    Result<int, string> result = "fail";
    result.IsErr.Should().BeTrue();
    result.IsOk.Should().BeFalse();
  }

  // ---- Match -----------------------------------------------------------

  [Fact]
  public void Match_Ok_invokes_ok_branch()
  {
    Result<int, string> result = 42;
    string output = result.Match(v => $"v={v}", e => $"e={e}");
    output.Should().Be("v=42");
  }

  [Fact]
  public void Match_Err_invokes_err_branch()
  {
    Result<int, string> result = "fail";
    string output = result.Match(v => $"v={v}", e => $"e={e}");
    output.Should().Be("e=fail");
  }

  // ---- Switch ----------------------------------------------------------

  [Fact]
  public void Switch_Ok_invokes_ok_action()
  {
    Result<int, string> result = 42;
    int? captured = null;
    result.Switch(v => captured = v, _ => { });
    captured.Should().Be(42);
  }

  [Fact]
  public void Switch_Err_invokes_err_action()
  {
    Result<int, string> result = "fail";
    string? captured = null;
    result.Switch(_ => { }, e => captured = e);
    captured.Should().Be("fail");
  }

  // ---- Or (value fallback) --------------------------------------------

  [Fact]
  public void Or_Ok_returns_value()
  {
    Result<int, string> result = 42;
    result.Or(0).Should().Be(42);
  }

  [Fact]
  public void Or_Err_returns_fallback()
  {
    Result<int, string> result = "fail";
    result.Or(0).Should().Be(0);
  }

  // ---- Or (computed fallback) -----------------------------------------

  [Fact]
  public void Or_computed_Ok_returns_value_without_invoking_fallback()
  {
    Result<int, string> result = 42;
    bool invoked = false;
    int value = result.Or(_ => { invoked = true; return 0; });
    value.Should().Be(42);
    invoked.Should().BeFalse();
  }

  [Fact]
  public void Or_computed_Err_invokes_fallback_with_error()
  {
    Result<int, string> result = "fail";
    int value = result.Or(e => e.Length);
    value.Should().Be(4);
  }

  // ---- Map -------------------------------------------------------------

  [Fact]
  public void Map_Ok_transforms_value()
  {
    Result<int, string> result = 42;
    Result<string, string> mapped = result.Map(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture));
    mapped.Should().BeOfType<Result<string, string>.Ok>()
        .Which.Value.Should().Be("42");
  }

  [Fact]
  public void Map_Err_preserves_error()
  {
    Result<int, string> result = "fail";
    Result<string, string> mapped = result.Map(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture));
    mapped.Should().BeOfType<Result<string, string>.Err>()
        .Which.Error.Should().Be("fail");
  }

  // ---- MapError --------------------------------------------------------

  [Fact]
  public void MapError_Ok_preserves_value()
  {
    Result<int, string> result = 42;
    Result<int, int> mapped = result.MapError(e => e.Length);
    mapped.Should().BeOfType<Result<int, int>.Ok>()
        .Which.Value.Should().Be(42);
  }

  [Fact]
  public void MapError_Err_transforms_error()
  {
    Result<int, string> result = "fail";
    Result<int, int> mapped = result.MapError(e => e.Length);
    mapped.Should().BeOfType<Result<int, int>.Err>()
        .Which.Error.Should().Be(4);
  }

  // ---- Bind ------------------------------------------------------------

  [Fact]
  public void Bind_Ok_chains_to_second_Ok()
  {
    Result<int, string> result = 42;
    Result<double, string> bound = result.Bind(v => (Result<double, string>)(v * 1.5));
    bound.Should().BeOfType<Result<double, string>.Ok>()
        .Which.Value.Should().Be(63.0);
  }

  [Fact]
  public void Bind_Ok_chains_to_second_Err()
  {
    Result<int, string> result = 42;
    Result<int, string> bound = result.Bind(_ => (Result<int, string>)"second fail");
    bound.Should().BeOfType<Result<int, string>.Err>()
        .Which.Error.Should().Be("second fail");
  }

  [Fact]
  public void Bind_Err_short_circuits()
  {
    Result<int, string> result = "first fail";
    bool invoked = false;
    Result<int, string> bound = result.Bind(v => { invoked = true; return (Result<int, string>)v; });
    bound.Should().BeOfType<Result<int, string>.Err>()
        .Which.Error.Should().Be("first fail");
    invoked.Should().BeFalse();
  }

  // ---- Pattern matching -----------------------------------------------

#pragma warning disable CS8509 // Closed hierarchy — Ok and Err are exhaustive
  [Fact]
  public void Pattern_match_Ok_extracts_value()
  {
    Result<int, string> result = 42;
    int value = result switch
    {
      Result<int, string>.Ok(var v) => v,
      Result<int, string>.Err(var e) => e.Length,
    };
    value.Should().Be(42);
  }

  [Fact]
  public void Pattern_match_Err_extracts_error()
  {
    Result<int, string> result = "fail";
    int value = result switch
    {
      Result<int, string>.Ok(var v) => v,
      Result<int, string>.Err(var e) => e.Length,
    };
    value.Should().Be(4);
  }
#pragma warning restore CS8509
}
