// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Tests;

/// <summary>
/// Tests <see cref="OrdinalForms"/> resolved-value equality, fallback
/// behaviour, and <c>ToString</c> output.
/// </summary>
public sealed class OrdinalFormsTests
{
  private static readonly OrdinalForms EnglishSuffixes = new()
  {
    Other = "th",
    One = "st",
    Two = "nd",
    Few = "rd",
  };

  // ---- Fallback behaviour -----------------------------------------

  [Fact]
  public void UnsetSlots_FallBackToOther()
  {
    OrdinalForms forms = new() { Other = "th" };

    forms.Zero.Should().Be("th");
    forms.One.Should().Be("th");
    forms.Two.Should().Be("th");
    forms.Few.Should().Be("th");
    forms.Many.Should().Be("th");
  }

  [Fact]
  public void ExplicitlySetSlots_ReturnTheirOwnValue()
  {
    OrdinalForms forms = new()
    {
      Other = "th",
      Zero = "zero",
      One = "st",
      Two = "nd",
      Few = "rd",
      Many = "many",
    };

    forms.Other.Should().Be("th");
    forms.Zero.Should().Be("zero");
    forms.One.Should().Be("st");
    forms.Two.Should().Be("nd");
    forms.Few.Should().Be("rd");
    forms.Many.Should().Be("many");
  }

  // ---- Resolved-value equality ------------------------------------

  [Fact]
  public void Equality_ComparesResolvedValues_NotBackingFields()
  {
    OrdinalForms implicitFallback = new() { Other = "th" };

    OrdinalForms explicitAll = new()
    {
      Other = "th",
      Zero = "th",
      One = "th",
      Two = "th",
      Few = "th",
      Many = "th",
    };

    implicitFallback.Should().Be(explicitAll);
    implicitFallback.GetHashCode().Should().Be(explicitAll.GetHashCode());
  }

  [Fact]
  public void Equality_DifferentResolvedValues_AreNotEqual()
  {
    OrdinalForms a = new() { Other = "th", One = "st" };
    OrdinalForms b = new() { Other = "th", One = "er" };

    a.Should().NotBe(b);
  }

  // ---- ToString ---------------------------------------------------

  [Fact]
  public void ToString_PrintsResolvedValues()
  {
    string text = EnglishSuffixes.ToString();

    text.Should().Contain("st");
    text.Should().Contain("nd");
    text.Should().Contain("rd");
    text.Should().Contain("th");
  }

  // ---- Select extension -------------------------------------------

  [Fact]
  public void Select_ReturnsCorrectForm_ForEachCategory()
  {
    OrdinalForms forms = new()
    {
      Other = "th",
      Zero = "zero",
      One = "st",
      Two = "nd",
      Few = "rd",
      Many = "many",
    };

    OrdinalCategory.Other.Select(forms).Should().Be("th");
    OrdinalCategory.Zero.Select(forms).Should().Be("zero");
    OrdinalCategory.One.Select(forms).Should().Be("st");
    OrdinalCategory.Two.Select(forms).Should().Be("nd");
    OrdinalCategory.Few.Select(forms).Should().Be("rd");
    OrdinalCategory.Many.Select(forms).Should().Be("many");
  }

  [Fact]
  public void Select_UnsetCategory_ReturnsFallback()
  {
    OrdinalCategory.Many.Select(EnglishSuffixes).Should().Be("th");
  }
}
