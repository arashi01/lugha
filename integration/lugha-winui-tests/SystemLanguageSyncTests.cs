// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.WinUI.Tests;

public sealed class SystemLanguageSyncTests
{
  [Fact]
  public void Apply_rejects_null_locale()
  {
    Action act = () => SystemLanguageSync.Apply(null!);

    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("locale");
  }

  [Fact]
  public void Apply_with_root_rejects_null_locale()
  {
    // Cannot create a FrameworkElement here (requires dispatcher thread),
    // so pass null for both and assert the locale null check fires first.
    Action act = () => SystemLanguageSync.Apply(null!, null!);

    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("locale");
  }

  [Fact]
  public void Apply_with_root_rejects_null_element()
  {
    Action act = () => SystemLanguageSync.Apply(new TestEnGbLocale(), null!);

    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("rootElement");
  }
}
