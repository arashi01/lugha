// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

#pragma warning disable IDE0130 // Namespace intentionally Lugha for discoverability
namespace Lugha;
#pragma warning restore IDE0130

/// <summary>Extensions on <see cref="PluralCategory"/>.</summary>
#pragma warning disable CA1034 // C# 14 extension block compiles as nested type
public static class PluralCategoryExtensions
{
  extension(PluralCategory category)
  {
    /// <summary>
    /// Selects the form matching this category from <paramref name="forms"/>.
    /// Unset categories resolve to <see cref="PluralForms.Other"/>.
    /// </summary>
    public string Select(PluralForms forms) => category switch
    {
      PluralCategory.Zero => forms.Zero,
      PluralCategory.One => forms.One,
      PluralCategory.Two => forms.Two,
      PluralCategory.Few => forms.Few,
      PluralCategory.Many => forms.Many,
      _ => forms.Other,
    };
  }
}
