// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Lugha.Analysers;

/// <summary>
/// <b>LGH004</b> - Warning: a parameterised text scope method implementation
/// must use all its parameters in the return expression.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParameterUsageAnalyser : DiagnosticAnalyzer
{
  /// <summary>Diagnostic identifier.</summary>
  public const string DiagnosticId = "LGH004";

  private static readonly DiagnosticDescriptor Rule = new(
      DiagnosticId,
      title: "Unused parameter in text scope method",
      messageFormat: "Parameter '{0}' is not used in the return expression of text scope member '{1}'",
      category: "Lugha.Design",
      defaultSeverity: DiagnosticSeverity.Warning,
      isEnabledByDefault: true,
      description: "All parameters of a text scope method implementation should be used in the return expression.");

  /// <inheritdoc />
  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
      [Rule];

  /// <inheritdoc />
  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();

    context.RegisterSymbolAction(AnalyseMethod, SymbolKind.Method);
  }

  private static void AnalyseMethod(SymbolAnalysisContext context)
  {
    var method = (IMethodSymbol)context.Symbol;

    if (method.Parameters.Length == 0)
    {
      return;
    }

    if (method.MethodKind is not MethodKind.Ordinary)
    {
      return;
    }

    INamedTypeSymbol? containingType = method.ContainingType;
    if (containingType is null || containingType.TypeKind is TypeKind.Interface)
    {
      return;
    }

    if (!TextScopeHelper.ImplementsTextScopeMember(method, containingType))
    {
      return;
    }

    foreach (SyntaxReference syntaxRef in method.DeclaringSyntaxReferences)
    {
      SyntaxNode declaration = syntaxRef.GetSyntax(context.CancellationToken);

      if (declaration is not MethodDeclarationSyntax methodDecl)
      {
        return;
      }

      if (methodDecl.ExpressionBody is not null)
      {
        CheckParameterUsage(context, method, methodDecl.ExpressionBody.Expression);
      }
      else if (methodDecl.Body is not null)
      {
        // Collect identifiers from all return expressions.
        var allIdentifiers = new HashSet<string>();
        foreach (ReturnStatementSyntax ret in methodDecl.Body.DescendantNodes().OfType<ReturnStatementSyntax>())
        {
          if (ret.Expression is not null)
          {
            foreach (IdentifierNameSyntax id in ret.Expression.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
            {
              allIdentifiers.Add(id.Identifier.Text);
            }
          }
        }

        foreach (IParameterSymbol parameter in method.Parameters)
        {
          if (!allIdentifiers.Contains(parameter.Name))
          {
            context.ReportDiagnostic(
                Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name, method.Name));
          }
        }
      }
    }
  }

  private static void CheckParameterUsage(
      SymbolAnalysisContext context,
      IMethodSymbol method,
      ExpressionSyntax returnExpression)
  {
    var identifierNames = new HashSet<string>(
        returnExpression
            .DescendantNodesAndSelf()
            .OfType<IdentifierNameSyntax>()
            .Select(id => id.Identifier.Text));

    foreach (IParameterSymbol parameter in method.Parameters)
    {
      if (!identifierNames.Contains(parameter.Name))
      {
        context.ReportDiagnostic(
            Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name, method.Name));
      }
    }
  }
}
