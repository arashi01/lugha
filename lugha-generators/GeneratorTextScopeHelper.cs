// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Microsoft.CodeAnalysis;

namespace Lugha.Generators;

/// <summary>
/// Shared helper for detecting <c>ITextScope</c>-derived interfaces and
/// composite locale interfaces across Lugha source generators.
/// </summary>
internal static class GeneratorTextScopeHelper
{
  /// <summary>
  /// Fully-qualified metadata name of the <c>Lugha.ITextScope</c> marker interface.
  /// </summary>
  internal const string TextScopeFullName = "Lugha.ITextScope";

  /// <summary>
  /// Fully-qualified metadata name of the <c>Lugha.ILocale</c> interface.
  /// </summary>
  internal const string LocaleFullName = "Lugha.ILocale";

  /// <summary>
  /// Determines whether <paramref name="symbol"/> is an interface that directly
  /// or transitively extends <c>Lugha.ITextScope</c> (but is not <c>ITextScope</c> itself).
  /// </summary>
  internal static bool IsTextScopeInterface(INamedTypeSymbol symbol)
  {
    if (symbol.TypeKind is not TypeKind.Interface)
    {
      return false;
    }

    if (IsTextScopeMarker(symbol))
    {
      return false;
    }

    foreach (INamedTypeSymbol iface in symbol.AllInterfaces)
    {
      if (IsTextScopeMarker(iface))
      {
        return true;
      }
    }

    return false;
  }

  /// <summary>
  /// Determines whether <paramref name="symbol"/> is the <c>Lugha.ITextScope</c>
  /// marker interface itself.
  /// </summary>
  internal static bool IsTextScopeMarker(INamedTypeSymbol symbol) =>
      symbol.ToDisplayString() == TextScopeFullName;

  /// <summary>
  /// Determines whether <paramref name="symbol"/> implements <c>Lugha.ILocale</c>.
  /// </summary>
  internal static bool ImplementsLocale(INamedTypeSymbol symbol)
  {
    foreach (INamedTypeSymbol iface in symbol.AllInterfaces)
    {
      string display = iface.OriginalDefinition.ToDisplayString();
      if (display == LocaleFullName || display.StartsWith(LocaleFullName + "<", System.StringComparison.Ordinal))
      {
        return true;
      }
    }

    return false;
  }
}
