// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Tests;

/// <summary>
/// Tests <see cref="PluralForms"/> resolved-value equality, fallback
/// behaviour, and <c>ToString</c> output.
/// </summary>
public sealed class PluralFormsTests
{
  private static readonly PluralForms ItemForms = new()
  {
    Other = "items",
    One = "item",
  };

  // ---- Fallback behaviour -----------------------------------------

  [Fact]
  public void UnsetSlots_FallBackToOther()
  {
    PluralForms forms = new() { Other = "x" };

    forms.One.Should().Be("x");
    forms.Two.Should().Be("x");
    forms.Zero.Should().Be("x");
    forms.Few.Should().Be("x");
    forms.Many.Should().Be("x");
  }

  [Fact]
  public void ExplicitlySetSlots_ReturnTheirOwnValue()
  {
    PluralForms forms = new()
    {
      Other = "other",
      Zero = "zero",
      One = "one",
      Two = "two",
      Few = "few",
      Many = "many",
    };

    forms.Other.Should().Be("other");
    forms.Zero.Should().Be("zero");
    forms.One.Should().Be("one");
    forms.Two.Should().Be("two");
    forms.Few.Should().Be("few");
    forms.Many.Should().Be("many");
  }

  // ---- Resolved-value equality ------------------------------------

  [Fact]
  public void Equality_ComparesResolvedValues_NotBackingFields()
  {
    // Only Other set explicitly — all slots resolve to "x"
    PluralForms implicitFallback = new() { Other = "x" };

    // All slots explicitly set to the same value
    PluralForms explicitAll = new()
    {
      Other = "x",
      Zero = "x",
      One = "x",
      Two = "x",
      Few = "x",
      Many = "x",
    };

    implicitFallback.Should().Be(explicitAll);
    implicitFallback.GetHashCode().Should().Be(explicitAll.GetHashCode());
  }

  [Fact]
  public void Equality_DifferentResolvedValues_AreNotEqual()
  {
    PluralForms a = new() { Other = "items", One = "item" };
    PluralForms b = new() { Other = "things", One = "thing" };

    a.Should().NotBe(b);
  }

  [Fact]
  public void Equality_SameOther_DifferentExplicitSlot_AreNotEqual()
  {
    PluralForms withOne = new() { Other = "x", One = "y" };
    PluralForms withoutOne = new() { Other = "x" };

    withOne.Should().NotBe(withoutOne);
  }

  // ---- ToString ---------------------------------------------------

  [Fact]
  public void ToString_PrintsResolvedValues()
  {
    PluralForms forms = new() { Other = "items", One = "item" };

    string text = forms.ToString();

    // Resolved: One = "item", all others = "items" (the fallback value)
    text.Should().Contain("item");
    text.Should().Contain("items");
  }

  // ---- Select extension -------------------------------------------

  [Fact]
  public void Select_ReturnsCorrectForm_ForEachCategory()
  {
    PluralForms forms = new()
    {
      Other = "other",
      Zero = "zero",
      One = "one",
      Two = "two",
      Few = "few",
      Many = "many",
    };

    PluralCategory.Other.Select(forms).Should().Be("other");
    PluralCategory.Zero.Select(forms).Should().Be("zero");
    PluralCategory.One.Select(forms).Should().Be("one");
    PluralCategory.Two.Select(forms).Should().Be("two");
    PluralCategory.Few.Select(forms).Should().Be("few");
    PluralCategory.Many.Select(forms).Should().Be("many");
  }

  [Fact]
  public void Select_UnsetCategory_ReturnsFallback()
  {
    PluralCategory.Few.Select(ItemForms).Should().Be("items");
  }
}
