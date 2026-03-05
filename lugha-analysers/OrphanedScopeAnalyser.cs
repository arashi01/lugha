// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Lugha.Analysers;

/// <summary>
/// <b>LGH005</b> - Info (opt-in): a text scope interface has no
/// implementations in the assembly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OrphanedScopeAnalyser : DiagnosticAnalyzer
{
  /// <summary>Diagnostic identifier.</summary>
  public const string DiagnosticId = "LGH005";

  private static readonly DiagnosticDescriptor Rule = new(
      DiagnosticId,
      title: "Orphaned text scope interface",
      messageFormat: "Text scope '{0}' has no implementations in this assembly",
      category: "Lugha.Design",
      defaultSeverity: DiagnosticSeverity.Info,
      isEnabledByDefault: false,
      description: "A text scope interface has no concrete implementations in the assembly.",
      customTags: [WellKnownDiagnosticTags.CompilationEnd]);

  /// <inheritdoc />
  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
      [Rule];

  /// <inheritdoc />
  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();

    context.RegisterCompilationAction(AnalyseCompilation);
  }

  private static void AnalyseCompilation(CompilationAnalysisContext context)
  {
    var textScopeInterfaces = new List<INamedTypeSymbol>();
    var implementedScopes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

    CollectSymbols(context.Compilation.GlobalNamespace, textScopeInterfaces, implementedScopes);

    foreach (INamedTypeSymbol scope in textScopeInterfaces)
    {
      if (implementedScopes.Contains(scope))
      {
        continue;
      }

      foreach (SyntaxReference syntaxRef in scope.DeclaringSyntaxReferences)
      {
        context.ReportDiagnostic(
            Diagnostic.Create(Rule, syntaxRef.GetSyntax(context.CancellationToken).GetLocation(), scope.Name));
      }
    }
  }

  private static void CollectSymbols(
      INamespaceSymbol ns,
      List<INamedTypeSymbol> textScopeInterfaces,
      HashSet<INamedTypeSymbol> implementedScopes)
  {
    foreach (INamedTypeSymbol type in ns.GetTypeMembers())
    {
      ProcessType(type, textScopeInterfaces, implementedScopes);
    }

    foreach (INamespaceSymbol child in ns.GetNamespaceMembers())
    {
      CollectSymbols(child, textScopeInterfaces, implementedScopes);
    }
  }

  private static void ProcessType(
      INamedTypeSymbol type,
      List<INamedTypeSymbol> textScopeInterfaces,
      HashSet<INamedTypeSymbol> implementedScopes)
  {
    if (TextScopeHelper.IsTextScopeInterface(type))
    {
      textScopeInterfaces.Add(type);
    }

    if (type.TypeKind is TypeKind.Class or TypeKind.Struct)
    {
      foreach (INamedTypeSymbol iface in type.AllInterfaces)
      {
        if (TextScopeHelper.IsTextScopeInterface(iface))
        {
          implementedScopes.Add(iface);
        }
      }
    }

    // Process nested types.
    foreach (INamedTypeSymbol nested in type.GetTypeMembers())
    {
      ProcessType(nested, textScopeInterfaces, implementedScopes);
    }
  }
}
