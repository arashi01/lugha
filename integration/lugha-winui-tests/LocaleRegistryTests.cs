// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.WinUI.Tests;

public sealed class LocaleRegistryTests
{
  [Fact]
  public void Create_succeeds_with_single_locale()
  {
    Result<LocaleRegistry<ILocale>, DuplicateLanguageTag> result =
        LocaleRegistry<ILocale>.Create(new TestEnGbLocale());

    Result<LocaleRegistry<ILocale>, DuplicateLanguageTag>.Ok ok =
        result.Should().BeOfType<Result<LocaleRegistry<ILocale>, DuplicateLanguageTag>.Ok>()
            .Which;
    ok.Value.Count.Should().Be(1);
    ok.Value.Default.Culture.Name.Should().Be("en-GB");
  }

  [Fact]
  public void Create_succeeds_with_multiple_locales()
  {
    Result<LocaleRegistry<ILocale>, DuplicateLanguageTag> result =
        LocaleRegistry<ILocale>.Create(
            new TestEnGbLocale(), new TestArSaLocale(), new TestEsEsLocale());

    Result<LocaleRegistry<ILocale>, DuplicateLanguageTag>.Ok ok =
        result.Should().BeOfType<Result<LocaleRegistry<ILocale>, DuplicateLanguageTag>.Ok>()
            .Which;
    ok.Value.Count.Should().Be(3);
  }

  [Fact]
  public void Create_returns_err_for_duplicate_tag()
  {
    Result<LocaleRegistry<ILocale>, DuplicateLanguageTag> result =
        LocaleRegistry<ILocale>.Create(new TestEnGbLocale(), new TestEnGbLocale());

    result.Should().BeOfType<Result<LocaleRegistry<ILocale>, DuplicateLanguageTag>.Err>()
        .Which.Error.Tag.Should().Be("en-GB");
  }

  [Fact]
  public void Languages_returns_registered_tags()
  {
    LocaleRegistry<ILocale> registry =
        CreateRegistry(new TestEnGbLocale(), new TestArSaLocale(), new TestEsEsLocale());

    registry.Languages.Should().BeEquivalentTo(["en-GB", "ar-SA", "es-ES"]);
  }

  [Fact]
  public void Locales_returns_all_instances()
  {
    var enGb = new TestEnGbLocale();
    var arSa = new TestArSaLocale();
    LocaleRegistry<ILocale> registry = CreateRegistry(enGb, arSa);

    registry.Locales.Should().BeEquivalentTo(new ILocale[] { enGb, arSa });
  }

  [Fact]
  public void Resolve_returns_matching_locale()
  {
    var enGb = new TestEnGbLocale();
    var arSa = new TestArSaLocale();
    LocaleRegistry<ILocale> registry = CreateRegistry(enGb, arSa);

    registry.Resolve("en-GB").Should().BeSameAs(enGb);
    registry.Resolve("ar-SA").Should().BeSameAs(arSa);
  }

  [Fact]
  public void Resolve_is_case_insensitive()
  {
    var enGb = new TestEnGbLocale();
    LocaleRegistry<ILocale> registry = CreateRegistry(enGb);

    registry.Resolve("EN-GB").Should().BeSameAs(enGb);
    registry.Resolve("en-gb").Should().BeSameAs(enGb);
  }

  [Fact]
  public void Resolve_returns_default_for_unknown_tag()
  {
    var enGb = new TestEnGbLocale();
    LocaleRegistry<ILocale> registry = CreateRegistry(enGb);

    registry.Resolve("fr-FR").Should().BeSameAs(enGb);
  }

  [Fact]
  public void TryResolve_returns_null_for_unknown_tag()
  {
    LocaleRegistry<ILocale> registry = CreateRegistry(new TestEnGbLocale());

    registry.TryResolve("fr-FR").Should().BeNull();
  }

  [Fact]
  public void Resolve_falls_back_to_parent_tag()
  {
    var es = new TestEsLocale();
    LocaleRegistry<ILocale> registry = CreateRegistry(es);

    registry.Resolve("es-419").Should().BeSameAs(es);
  }

  [Fact]
  public void Resolve_falls_back_through_multiple_subtags()
  {
    var arSa = new TestArSaLocale();
    LocaleRegistry<ILocale> registry = CreateRegistry(arSa);

    registry.Resolve("ar-SA-u-ca-islamic").Should().BeSameAs(arSa);
  }

  [Fact]
  public void Resolve_prefers_exact_match_over_parent_fallback()
  {
    var es = new TestEsLocale();
    var esEs = new TestEsEsLocale();
    LocaleRegistry<ILocale> registry = CreateRegistry(es, esEs);

    registry.Resolve("es-ES").Should().BeSameAs(esEs);
    registry.Resolve("es-419").Should().BeSameAs(es);
  }

  [Fact]
  public void Contains_returns_true_for_registered_tag()
  {
    LocaleRegistry<ILocale> registry = CreateRegistry(new TestEnGbLocale());

    registry.Contains("en-GB").Should().BeTrue();
  }

  [Fact]
  public void Contains_returns_false_for_unregistered_tag()
  {
    LocaleRegistry<ILocale> registry = CreateRegistry(new TestEnGbLocale());

    registry.Contains("fr-FR").Should().BeFalse();
  }

  [Fact]
  public void Contains_is_exact_match_not_subtag_fallback()
  {
    LocaleRegistry<ILocale> registry = CreateRegistry(new TestEsLocale());

    registry.Contains("es").Should().BeTrue();
    registry.Contains("es-419").Should().BeFalse();
  }

  [Fact]
  public void Default_is_first_locale()
  {
    var enGb = new TestEnGbLocale();
    LocaleRegistry<ILocale> registry = CreateRegistry(enGb, new TestArSaLocale());

    registry.Default.Should().BeSameAs(enGb);
  }

  private static LocaleRegistry<ILocale> CreateRegistry(
      ILocale defaultLocale, params ILocale[] additional)
  {
    Result<LocaleRegistry<ILocale>, DuplicateLanguageTag> result =
        LocaleRegistry<ILocale>.Create(defaultLocale, additional);
    Result<LocaleRegistry<ILocale>, DuplicateLanguageTag>.Ok ok =
        result.Should().BeOfType<Result<LocaleRegistry<ILocale>, DuplicateLanguageTag>.Ok>()
            .Which;
    return ok.Value;
  }
}
