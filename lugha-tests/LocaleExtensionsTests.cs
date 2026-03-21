// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Tests;

/// <summary>
/// Tests <see cref="LocaleExtensions"/> convenience members on <see cref="ILocale"/>.
/// Plural/Ordinal extensions are covered by <see cref="PluralTests"/> and
/// <see cref="OrdinalTests"/>; this class covers the remaining members.
/// </summary>
public sealed class LocaleExtensionsTests
{
  [Fact]
  public void IsRightToLeft_false_for_ltr_locale() =>
      new TestEnGbLocale().IsRightToLeft.Should().BeFalse();

  [Fact]
  public void IsRightToLeft_true_for_rtl_locale()
  {
    // ar-SA is a right-to-left locale
    ILocale locale = new TestArSaLocale();
    locale.IsRightToLeft.Should().BeTrue();
  }
}
