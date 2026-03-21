// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Tests;

/// <summary>
/// Tests <see cref="LocaleHost{TLocale}"/> with synchronous dispatch
/// (no WinUI dependency).
/// </summary>
public sealed class LocaleHostTests
{
  private static LocaleHost<ILocale> CreateHost(ILocale initial) =>
      new(initial, action => action());

  [Fact]
  public void Constructor_sets_initial_Current()
  {
    var locale = new TestEnGbLocale();
    LocaleHost<ILocale> host = CreateHost(locale);

    host.Current.Should().BeSameAs(locale);
  }

  [Fact]
  public void SetLocale_updates_Current()
  {
    var initial = new TestEnGbLocale();
    var replacement = new TestDeLocale();
    LocaleHost<ILocale> host = CreateHost(initial);

    host.SetLocale(replacement);

    host.Current.Should().BeSameAs(replacement);
  }

  [Fact]
  public void SetLocale_fires_PropertyChanged_for_Current()
  {
    LocaleHost<ILocale> host = CreateHost(new TestEnGbLocale());
    List<string?> firedProperties = [];

    host.PropertyChanged += (_, e) => firedProperties.Add(e.PropertyName);
    host.SetLocale(new TestDeLocale());

    firedProperties.Should().Contain(nameof(LocaleHost<ILocale>.Current));
  }

  [Fact]
  public void SetLocale_same_instance_does_not_fire_PropertyChanged()
  {
    var locale = new TestEnGbLocale();
    LocaleHost<ILocale> host = CreateHost(locale);
    bool fired = false;

    host.PropertyChanged += (_, _) => fired = true;
    host.SetLocale(locale);

    fired.Should().BeFalse();
  }

  [Fact]
  public void SetLocale_different_instance_same_type_fires_PropertyChanged()
  {
    var a = new TestEnGbLocale();
    var b = new TestEnGbLocale();
    LocaleHost<ILocale> host = CreateHost(a);
    bool fired = false;

    host.PropertyChanged += (_, _) => fired = true;
    host.SetLocale(b);

    fired.Should().BeTrue();
    host.Current.Should().BeSameAs(b);
  }
}
