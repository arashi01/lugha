// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Lugha.Analysers;

/// <summary>
/// <b>LGH008</b> — Warning: text scope implementation body contains known
/// side-effecting calls. This is a heuristic analyser — it detects common
/// side-effect patterns but is not sound.
/// </summary>
/// <remarks>
/// Flagged patterns: <c>Console.Write*</c>, <c>File.*</c>, <c>HttpClient.*</c>,
/// <c>Debug.*</c>, <c>Trace.*</c>, <c>await</c> expressions, assignments to
/// fields/properties outside the method, and <c>throw</c> of non-argument exceptions.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SideEffectAnalyser : DiagnosticAnalyzer
{
  /// <summary>Diagnostic identifier.</summary>
  public const string DiagnosticId = "LGH008";

  private static readonly DiagnosticDescriptor Rule = new(
      DiagnosticId,
      title: "Side-effecting call in text scope implementation",
      messageFormat: "Text scope member '{0}' contains potentially side-effecting call to '{1}'",
      category: "Lugha.Design",
      defaultSeverity: DiagnosticSeverity.Warning,
      isEnabledByDefault: true,
      description: "Text scope implementation bodies should be pure (no side effects). This is a heuristic check — it detects common side-effect patterns but is not sound.");

  // Known side-effecting type prefixes for member access detection.
  private static readonly ImmutableArray<string> SideEffectingTypes =
      ["Console", "File", "HttpClient", "Debug", "Trace"];

  /// <inheritdoc />
  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
      [Rule];

  /// <inheritdoc />
  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();

    context.RegisterSyntaxNodeAction(AnalyseMethodDeclaration, SyntaxKind.MethodDeclaration);
    context.RegisterSyntaxNodeAction(AnalysePropertyDeclaration, SyntaxKind.PropertyDeclaration);
  }

  private static void AnalyseMethodDeclaration(SyntaxNodeAnalysisContext context)
  {
    var methodDecl = (MethodDeclarationSyntax)context.Node;
    if (context.SemanticModel.GetDeclaredSymbol(methodDecl, context.CancellationToken) is not IMethodSymbol method)
    {
      return;
    }

    if (method.ContainingType.TypeKind is TypeKind.Interface)
    {
      return;
    }

    if (!TextScopeHelper.ImplementsTextScopeMember(method, method.ContainingType))
    {
      return;
    }

    SyntaxNode? body = (SyntaxNode?)methodDecl.Body ?? methodDecl.ExpressionBody;
    if (body is null)
    {
      return;
    }

    AnalyseBody(context, body, method.Name);
  }

  private static void AnalysePropertyDeclaration(SyntaxNodeAnalysisContext context)
  {
    var propertyDecl = (PropertyDeclarationSyntax)context.Node;
    if (context.SemanticModel.GetDeclaredSymbol(propertyDecl, context.CancellationToken) is not IPropertySymbol property)
    {
      return;
    }

    if (property.ContainingType.TypeKind is TypeKind.Interface)
    {
      return;
    }

    if (!TextScopeHelper.ImplementsTextScopeMember(property, property.ContainingType))
    {
      return;
    }

    if (propertyDecl.ExpressionBody is not null)
    {
      AnalyseBody(context, propertyDecl.ExpressionBody, property.Name);
    }

    if (propertyDecl.AccessorList is not null)
    {
      foreach (AccessorDeclarationSyntax accessor in propertyDecl.AccessorList.Accessors)
      {
        SyntaxNode? body = (SyntaxNode?)accessor.Body ?? accessor.ExpressionBody;
        if (body is not null)
        {
          AnalyseBody(context, body, property.Name);
        }
      }
    }
  }

  private static void AnalyseBody(SyntaxNodeAnalysisContext context, SyntaxNode body, string memberName)
  {
    foreach (SyntaxNode node in body.DescendantNodesAndSelf())
    {
      // await expressions.
      if (node is AwaitExpressionSyntax awaitExpr)
      {
        context.ReportDiagnostic(
            Diagnostic.Create(Rule, awaitExpr.GetLocation(), memberName, "await"));
        continue;
      }

      // Invocations: Console.Write*, File.*, HttpClient.*, Debug.*, Trace.*
      if (node is InvocationExpressionSyntax invocation &&
          invocation.Expression is MemberAccessExpressionSyntax memberAccessInvocation)
      {
        string? typeName = GetTypeNameFromExpression(memberAccessInvocation.Expression);
        if (typeName is not null)
        {
          foreach (string sideEffecting in SideEffectingTypes)
          {
            if (typeName == sideEffecting)
            {
              string fullCall = $"{typeName}.{memberAccessInvocation.Name.Identifier.Text}";
              context.ReportDiagnostic(
                  Diagnostic.Create(Rule, invocation.GetLocation(), memberName, fullCall));
              break;
            }
          }
        }
      }

      // Assignments to fields/properties outside the method.
      if (node is AssignmentExpressionSyntax assignment)
      {
        if (assignment.Left is MemberAccessExpressionSyntax fieldAccess &&
            fieldAccess.Expression is ThisExpressionSyntax or BaseExpressionSyntax)
        {
          string target = fieldAccess.Name.Identifier.Text;
          context.ReportDiagnostic(
              Diagnostic.Create(Rule, assignment.GetLocation(), memberName, $"this.{target}"));
        }
        else if (assignment.Left is IdentifierNameSyntax identifierAssignment)
        {
          // Check if it is a field or property assignment (not a local variable).
          SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(identifierAssignment, context.CancellationToken);
          if (symbolInfo.Symbol is IFieldSymbol or IPropertySymbol)
          {
            context.ReportDiagnostic(
                Diagnostic.Create(Rule, assignment.GetLocation(), memberName, identifierAssignment.Identifier.Text));
          }
        }
      }

      // throw of non-argument exceptions.
      if (node is ThrowExpressionSyntax throwExpr && !IsArgumentException(throwExpr.Expression))
      {
        context.ReportDiagnostic(
            Diagnostic.Create(Rule, throwExpr.GetLocation(), memberName, "throw"));
      }

      if (node is ThrowStatementSyntax throwStatement &&
          throwStatement.Expression is not null &&
          !IsArgumentException(throwStatement.Expression))
      {
        context.ReportDiagnostic(
            Diagnostic.Create(Rule, throwStatement.GetLocation(), memberName, "throw"));
      }
    }
  }

  private static string? GetTypeNameFromExpression(ExpressionSyntax expression) => expression switch
  {
    IdentifierNameSyntax identifier => identifier.Identifier.Text,
    MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
    _ => null,
  };

  private static bool IsArgumentException(ExpressionSyntax? expression)
  {
    if (expression is not ObjectCreationExpressionSyntax creation)
    {
      return false;
    }

    string typeName = creation.Type.ToString();
    return typeName.Contains("ArgumentException") ||
           typeName.Contains("ArgumentNullException") ||
           typeName.Contains("ArgumentOutOfRangeException");
  }
}
