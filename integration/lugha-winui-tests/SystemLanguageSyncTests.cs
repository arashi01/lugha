// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.WinUI.Tests;

public sealed class SystemLanguageSyncTests
{
  [Fact(Skip = "ApplicationLanguages.PrimaryLanguageOverride requires a packaged application. " +
               "Verified visually via the Lugha.Samples.WinUI packaged sample.")]
  public void TryApply_sets_PrimaryLanguageOverride()
  {
    // This test cannot run in the unpackaged test host.
    // The behaviour is verified by the packaged sample project.
  }
}
