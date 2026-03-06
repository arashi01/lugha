// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.WinUI.Tests;

public sealed class LocaleRegistryTests
{
  [Fact]
  public void Constructor_rejects_null()
  {
    Action act = () => _ = new LocaleRegistry<ILocale>(null!);

    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("locales");
  }

  [Fact]
  public void Languages_returns_registered_tags()
  {
    LocaleRegistry<ILocale> registry = new([
        new TestEnGbLocale(),
            new TestArSaLocale(),
            new TestEsEsLocale(),
        ]);

    registry.Languages.Should().BeEquivalentTo(["en-GB", "ar-SA", "es-ES"]);
  }

  [Fact]
  public void Resolve_returns_matching_locale()
  {
    TestEnGbLocale enGb = new();
    TestArSaLocale arSa = new();
    LocaleRegistry<ILocale> registry = new([enGb, arSa]);

    registry.Resolve("en-GB").Should().BeSameAs(enGb);
    registry.Resolve("ar-SA").Should().BeSameAs(arSa);
  }

  [Fact]
  public void Resolve_is_case_insensitive()
  {
    TestEnGbLocale enGb = new();
    LocaleRegistry<ILocale> registry = new([enGb]);

    registry.Resolve("EN-GB").Should().BeSameAs(enGb);
    registry.Resolve("en-gb").Should().BeSameAs(enGb);
  }

  [Fact]
  public void Resolve_returns_null_for_unknown_tag()
  {
    LocaleRegistry<ILocale> registry = new([new TestEnGbLocale()]);

    registry.Resolve("fr-FR").Should().BeNull();
  }

  [Fact]
  public void Empty_registry_has_no_languages()
  {
    LocaleRegistry<ILocale> registry = new([]);

    registry.Languages.Should().BeEmpty();
  }

  [Fact]
  public void Empty_registry_resolve_returns_null()
  {
    LocaleRegistry<ILocale> registry = new([]);

    registry.Resolve("en-GB").Should().BeNull();
  }

  [Fact]
  public void Resolve_falls_back_to_parent_tag_when_exact_tag_unregistered()
  {
    TestEsLocale es = new();
    LocaleRegistry<ILocale> registry = new([es]);

    registry.Resolve("es-419").Should().BeSameAs(es);
  }

  [Fact]
  public void Resolve_falls_back_through_multiple_subtags()
  {
    TestArSaLocale arSa = new();
    LocaleRegistry<ILocale> registry = new([arSa]);

    // ar-SA-u-ca-islamic -> ar-SA-u-ca -> ar-SA-u -> ar-SA -> match
    registry.Resolve("ar-SA-u-ca-islamic").Should().BeSameAs(arSa);
  }

  [Fact]
  public void Resolve_prefers_exact_match_over_parent_fallback()
  {
    TestEsLocale es = new();
    TestEsEsLocale esEs = new();
    LocaleRegistry<ILocale> registry = new([es, esEs]);

    registry.Resolve("es-ES").Should().BeSameAs(esEs);
    registry.Resolve("es-419").Should().BeSameAs(es);
  }

  [Fact]
  public void Resolve_returns_null_when_no_ancestor_registered()
  {
    LocaleRegistry<ILocale> registry = new([new TestEnGbLocale()]);

    registry.Resolve("fr-FR").Should().BeNull();
  }

  [Fact]
  public void Resolve_with_fallback_returns_matched_locale()
  {
    TestEnGbLocale enGb = new();
    TestArSaLocale arSa = new();
    LocaleRegistry<ILocale> registry = new([enGb, arSa]);

    registry.Resolve("en-GB", arSa).Should().BeSameAs(enGb);
  }

  [Fact]
  public void Resolve_with_fallback_returns_fallback_on_miss()
  {
    TestEnGbLocale enGb = new();
    TestArSaLocale arSa = new();
    LocaleRegistry<ILocale> registry = new([enGb]);

    registry.Resolve("fr-FR", arSa).Should().BeSameAs(arSa);
  }

  [Fact]
  public void Resolve_with_fallback_rejects_null_fallback()
  {
    LocaleRegistry<ILocale> registry = new([new TestEnGbLocale()]);
    Action act = () => _ = registry.Resolve("en-GB", null!);

    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("fallback");
  }
}
