// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Microsoft.CodeAnalysis;

namespace Lugha.Analysers;

/// <summary>
/// Shared helper for detecting <c>ITextScope</c>-derived interfaces
/// across Lugha Roslyn analysers.
/// </summary>
internal static class TextScopeHelper
{
  /// <summary>
  /// Fully-qualified metadata name of the <c>Lugha.ITextScope</c> marker interface.
  /// </summary>
  internal const string TextScopeFullName = "Lugha.ITextScope";

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
  /// Resolves the <c>Lugha.ITextScope</c> type symbol from the compilation, or
  /// <see langword="null"/> if the type is not referenced.
  /// </summary>
  internal static INamedTypeSymbol? GetTextScopeSymbol(Compilation compilation) =>
      compilation.GetTypeByMetadataName(TextScopeFullName);

  /// <summary>
  /// Determines whether <paramref name="type"/> implements any
  /// <c>ITextScope</c>-derived interface.
  /// </summary>
  internal static bool ImplementsTextScope(INamedTypeSymbol type)
  {
    foreach (INamedTypeSymbol iface in type.AllInterfaces)
    {
      if (IsTextScopeInterface(iface))
      {
        return true;
      }
    }

    return false;
  }

  /// <summary>
  /// Checks whether <paramref name="member"/> implements a member
  /// of an <c>ITextScope</c>-derived interface.
  /// </summary>
  internal static bool ImplementsTextScopeMember(ISymbol member, INamedTypeSymbol containingType)
  {
    foreach (INamedTypeSymbol iface in containingType.AllInterfaces)
    {
      if (!IsTextScopeInterface(iface))
      {
        continue;
      }

      foreach (ISymbol ifaceMember in iface.GetMembers())
      {
        ISymbol? impl = containingType.FindImplementationForInterfaceMember(ifaceMember);
        if (impl is not null && SymbolEqualityComparer.Default.Equals(impl, member))
        {
          return true;
        }
      }
    }

    return false;
  }
}
