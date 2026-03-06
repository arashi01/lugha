// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Samples.WinUI.Tests;

/// <summary>
/// Verifies all generated translation scopes produce non-empty, well-formed text.
/// Each locale is tested for invariant strings, parameterised strings, and plural forms.
/// </summary>
public sealed class TranslationTests
{
  public static TheoryData<IAppLocale> AllLocales =>
      [
        new EnGbLocale(),
        new ArSaLocale(),
        new EsEsLocale(),
        new ZhHansLocale(),
      ];

  // --------------------------------------------------
  // Navigation scope - invariant strings
  // --------------------------------------------------

  [Theory]
  [MemberData(nameof(AllLocales))]
  public void Navigation_Dashboard_is_non_empty(IAppLocale locale) =>
      locale.Navigation.Dashboard.Should().NotBeNullOrWhiteSpace();

  [Theory]
  [MemberData(nameof(AllLocales))]
  public void Navigation_Settings_is_non_empty(IAppLocale locale) =>
      locale.Navigation.Settings.Should().NotBeNullOrWhiteSpace();

  // --------------------------------------------------
  // Connection scope - invariant + parameterised strings
  // --------------------------------------------------

  [Theory]
  [MemberData(nameof(AllLocales))]
  public void Connection_Discovering_is_non_empty(IAppLocale locale) =>
      locale.Connection.Discovering.Should().NotBeNullOrWhiteSpace();

  [Theory]
  [MemberData(nameof(AllLocales))]
  public void Connection_Connecting_contains_host(IAppLocale locale) =>
      locale.Connection.Connecting("myhost").Should().Contain("myhost");

  [Theory]
  [MemberData(nameof(AllLocales))]
  public void Connection_Connected_contains_host(IAppLocale locale) =>
      locale.Connection.Connected("server-1").Should().Contain("server-1");

  // --------------------------------------------------
  // Status scope - plural forms
  // --------------------------------------------------

  [Theory]
  [MemberData(nameof(AllLocales))]
  public void Status_OnlineUsers_zero_is_non_empty(IAppLocale locale) =>
      locale.Status.OnlineUsers(0).Should().NotBeNullOrWhiteSpace();

  [Theory]
  [MemberData(nameof(AllLocales))]
  public void Status_OnlineUsers_one_is_non_empty(IAppLocale locale) =>
      locale.Status.OnlineUsers(1).Should().NotBeNullOrWhiteSpace();

  [Theory]
  [MemberData(nameof(AllLocales))]
  public void Status_OnlineUsers_many_is_non_empty(IAppLocale locale) =>
      locale.Status.OnlineUsers(42).Should().NotBeNullOrWhiteSpace();

  // --------------------------------------------------
  // English plural correctness
  // --------------------------------------------------

  [Fact]
  public void EnGb_OnlineUsers_singular()
  {
    EnGbLocale locale = new();
    locale.Status.OnlineUsers(1).Should().Be("1 user online");
  }

  [Fact]
  public void EnGb_OnlineUsers_plural()
  {
    EnGbLocale locale = new();
    locale.Status.OnlineUsers(5).Should().Be("5 users online");
  }

  // --------------------------------------------------
  // Locale identity
  // --------------------------------------------------

  [Theory]
  [MemberData(nameof(AllLocales))]
  public void Locale_has_valid_culture(IAppLocale locale) =>
      locale.Culture.Should().NotBeNull();

  [Fact]
  public void EnGb_culture_name() =>
      new EnGbLocale().Culture.Name.Should().Be("en-GB");

  [Fact]
  public void ArSa_culture_name() =>
      new ArSaLocale().Culture.Name.Should().Be("ar-SA");

  [Fact]
  public void EsEs_culture_name() =>
      new EsEsLocale().Culture.Name.Should().Be("es-ES");

  [Fact]
  public void ZhHans_culture_name() =>
      new ZhHansLocale().Culture.Name.Should().Be("zh-Hans");
}
