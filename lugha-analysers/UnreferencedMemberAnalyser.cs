// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Lugha.Analysers;

/// <summary>
/// <b>LGH007</b> - Info (opt-in): a text scope member defined but never
/// referenced (via any implementation) in the assembly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnreferencedMemberAnalyser : DiagnosticAnalyzer
{
  /// <summary>Diagnostic identifier.</summary>
  public const string DiagnosticId = "LGH007";

  private static readonly DiagnosticDescriptor Rule = new(
      DiagnosticId,
      title: "Unreferenced text scope member",
      messageFormat: "Text scope member '{0}.{1}' is defined but unreferenced in this assembly",
      category: "Lugha.Design",
      defaultSeverity: DiagnosticSeverity.Info,
      isEnabledByDefault: false,
      description: "A text scope member is declared but never accessed in the assembly.",
      customTags: [WellKnownDiagnosticTags.CompilationEnd]);

  /// <inheritdoc />
  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
      [Rule];

  /// <inheritdoc />
  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();

    context.RegisterCompilationStartAction(startContext =>
    {
      var referencedMembers = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

      startContext.RegisterSyntaxNodeAction(
              ctx => CollectReferencedSymbol(ctx, referencedMembers),
              Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression,
              Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression,
              Microsoft.CodeAnalysis.CSharp.SyntaxKind.IdentifierName);

      startContext.RegisterCompilationEndAction(endContext =>
              AnalyseCompilation(endContext, referencedMembers));
    });
  }

  private static void CollectReferencedSymbol(
      SyntaxNodeAnalysisContext context,
      HashSet<ISymbol> referencedMembers)
  {
    SymbolInfo info = context.SemanticModel.GetSymbolInfo(context.Node, context.CancellationToken);
    ISymbol? referencedSymbol = info.Symbol;

    if (referencedSymbol is null)
    {
      return;
    }

    lock (referencedMembers)
    {
      referencedMembers.Add(referencedSymbol);

      // Also resolve interface members that the referenced symbol may implement.
      if (referencedSymbol.ContainingType is not null)
      {
        foreach (INamedTypeSymbol iface in referencedSymbol.ContainingType.AllInterfaces)
        {
          foreach (ISymbol ifaceMember in iface.GetMembers())
          {
            ISymbol? impl = referencedSymbol.ContainingType.FindImplementationForInterfaceMember(ifaceMember);
            if (impl is not null && SymbolEqualityComparer.Default.Equals(impl, referencedSymbol))
            {
              referencedMembers.Add(ifaceMember);
            }
          }
        }
      }
    }
  }

  private static void AnalyseCompilation(
      CompilationAnalysisContext context,
      HashSet<ISymbol> referencedMembers)
  {
    // Collect all text scope interface members.
    var scopeMembers = new List<(INamedTypeSymbol Scope, ISymbol Member)>();
    CollectScopeMembers(context.Compilation.GlobalNamespace, scopeMembers);

    if (scopeMembers.Count == 0)
    {
      return;
    }

    // Also check if any implementation of a member is referenced.
    var allTypes = new List<INamedTypeSymbol>();
    CollectAllTypes(context.Compilation.GlobalNamespace, allTypes);

    foreach ((INamedTypeSymbol scope, ISymbol member) in scopeMembers)
    {
      if (referencedMembers.Contains(member))
      {
        continue;
      }

      // Check if any implementation of this member is referenced.
      bool isImplementationReferenced = false;
      foreach (INamedTypeSymbol type in allTypes)
      {
        if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct))
        {
          continue;
        }

        ISymbol? impl = type.FindImplementationForInterfaceMember(member);
        if (impl is not null && referencedMembers.Contains(impl))
        {
          isImplementationReferenced = true;
          break;
        }
      }

      if (isImplementationReferenced)
      {
        continue;
      }

      context.ReportDiagnostic(
          Diagnostic.Create(Rule, member.Locations[0], scope.Name, member.Name));
    }
  }

  private static void CollectScopeMembers(
      INamespaceSymbol ns,
      List<(INamedTypeSymbol Scope, ISymbol Member)> scopeMembers)
  {
    foreach (INamedTypeSymbol type in ns.GetTypeMembers())
    {
      CollectScopeMembersFromType(type, scopeMembers);
    }

    foreach (INamespaceSymbol child in ns.GetNamespaceMembers())
    {
      CollectScopeMembers(child, scopeMembers);
    }
  }

  private static void CollectScopeMembersFromType(
      INamedTypeSymbol type,
      List<(INamedTypeSymbol Scope, ISymbol Member)> scopeMembers)
  {
    if (TextScopeHelper.IsTextScopeInterface(type))
    {
      foreach (ISymbol member in type.GetMembers())
      {
        if (member.IsImplicitlyDeclared)
        {
          continue;
        }

        // Skip property/event accessor methods - track properties instead.
        if (member is IMethodSymbol { MethodKind: not MethodKind.Ordinary })
        {
          continue;
        }

        scopeMembers.Add((type, member));
      }
    }

    foreach (INamedTypeSymbol nested in type.GetTypeMembers())
    {
      CollectScopeMembersFromType(nested, scopeMembers);
    }
  }

  private static void CollectAllTypes(INamespaceSymbol ns, List<INamedTypeSymbol> result)
  {
    foreach (INamedTypeSymbol type in ns.GetTypeMembers())
    {
      result.Add(type);
      CollectAllNestedTypes(type, result);
    }

    foreach (INamespaceSymbol child in ns.GetNamespaceMembers())
    {
      CollectAllTypes(child, result);
    }
  }

  private static void CollectAllNestedTypes(INamedTypeSymbol type, List<INamedTypeSymbol> result)
  {
    foreach (INamedTypeSymbol nested in type.GetTypeMembers())
    {
      result.Add(nested);
      CollectAllNestedTypes(nested, result);
    }
  }
}
