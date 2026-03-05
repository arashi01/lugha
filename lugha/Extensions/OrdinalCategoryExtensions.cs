// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

#pragma warning disable IDE0130 // Namespace intentionally Lugha for discoverability
namespace Lugha;
#pragma warning restore IDE0130

/// <summary>Extensions on <see cref="OrdinalCategory"/>.</summary>
#pragma warning disable CA1034 // C# 14 extension block compiles as nested type
public static class OrdinalCategoryExtensions
{
  extension(OrdinalCategory category)
  {
    /// <summary>
    /// Selects the form matching this category from <paramref name="forms"/>.
    /// Unset categories resolve to <see cref="OrdinalForms.Other"/>.
    /// </summary>
    public string Select(OrdinalForms forms) => category switch
    {
      OrdinalCategory.Zero => forms.Zero,
      OrdinalCategory.One => forms.One,
      OrdinalCategory.Two => forms.Two,
      OrdinalCategory.Few => forms.Few,
      OrdinalCategory.Many => forms.Many,
      _ => forms.Other,
    };
  }
}
