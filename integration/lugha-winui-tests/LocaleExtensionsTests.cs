// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Microsoft.UI.Xaml;

namespace Lugha.WinUI.Tests;

public sealed class LocaleExtensionsTests
{
  [Fact]
  public void FlowDirection_returns_LeftToRight_for_ltr_locale()
  {
    new TestEnGbLocale().FlowDirection().Should().Be(FlowDirection.LeftToRight);
  }

  [Fact]
  public void FlowDirection_returns_RightToLeft_for_rtl_locale()
  {
    new TestArSaLocale().FlowDirection().Should().Be(FlowDirection.RightToLeft);
  }

  [Fact]
  public void FlowDirection_rejects_null_locale()
  {
    Action act = () => ((ILocale)null!).FlowDirection();

    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("locale");
  }
}
