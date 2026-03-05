// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Lugha.Analysers;

/// <summary>
/// <b>LGH003</b> - Error: a text scope implementation must never return
/// <see langword="null"/>, <c>null!</c>, <see langword="default"/>,
/// <c>default!</c>, <see cref="string.Empty"/>, or <c>""</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NullOrEmptyReturnAnalyser : DiagnosticAnalyzer
{
  /// <summary>Diagnostic identifier.</summary>
  public const string DiagnosticId = "LGH003";

  private static readonly DiagnosticDescriptor Rule = new(
      DiagnosticId,
      title: "Text scope member must not return null or empty",
      messageFormat: "Text scope member '{0}' must not return null or empty",
      category: "Lugha.Design",
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "Text scope implementations must not return null, null!, default, default!, string.Empty, or \"\".");

  /// <inheritdoc />
  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
      [Rule];

  /// <inheritdoc />
  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();

    context.RegisterSyntaxNodeAction(AnalyseReturnStatement, SyntaxKind.ReturnStatement);
    context.RegisterSyntaxNodeAction(AnalyseArrowExpressionClause, SyntaxKind.ArrowExpressionClause);
  }

  private static void AnalyseReturnStatement(SyntaxNodeAnalysisContext context)
  {
    var returnStatement = (ReturnStatementSyntax)context.Node;
    if (returnStatement.Expression is null)
    {
      return;
    }

    CheckExpression(context, returnStatement.Expression);
  }

  private static void AnalyseArrowExpressionClause(SyntaxNodeAnalysisContext context)
  {
    var arrowClause = (ArrowExpressionClauseSyntax)context.Node;
    CheckExpression(context, arrowClause.Expression);
  }

  private static void CheckExpression(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
  {
    string? memberName = GetContainingTextScopeMemberName(context);
    if (memberName is null)
    {
      return;
    }

    if (IsProhibitedExpression(expression))
    {
      context.ReportDiagnostic(
          Diagnostic.Create(Rule, expression.GetLocation(), memberName));
    }
  }

  private static bool IsProhibitedExpression(ExpressionSyntax expression)
  {
    // Unwrap null-forgiving operator (null!, default!).
    ExpressionSyntax expr = expression;
    while (expr is PostfixUnaryExpressionSyntax postfix &&
           postfix.IsKind(SyntaxKind.SuppressNullableWarningExpression))
    {
      expr = postfix.Operand;
    }

    // null literal.
    if (expr.IsKind(SyntaxKind.NullLiteralExpression))
    {
      return true;
    }

    // default literal or default(T).
    if (expr.IsKind(SyntaxKind.DefaultLiteralExpression) || expr.IsKind(SyntaxKind.DefaultExpression))
    {
      return true;
    }

    // "" (empty string literal).
    if (expr is LiteralExpressionSyntax literal &&
        literal.IsKind(SyntaxKind.StringLiteralExpression) &&
        literal.Token.ValueText.Length == 0)
    {
      return true;
    }

    // string.Empty.
    if (expr is MemberAccessExpressionSyntax memberAccess &&
        memberAccess.Name.Identifier.Text == "Empty")
    {
      ExpressionSyntax target = memberAccess.Expression;

      // string.Empty
      if (target is PredefinedTypeSyntax predefined &&
          predefined.Keyword.IsKind(SyntaxKind.StringKeyword))
      {
        return true;
      }

      // String.Empty
      if (target is IdentifierNameSyntax identifier &&
          identifier.Identifier.Text == "String")
      {
        return true;
      }

      // System.String.Empty
      if (target is MemberAccessExpressionSyntax qualifiedAccess &&
          qualifiedAccess.Name.Identifier.Text == "String")
      {
        return true;
      }
    }

    return false;
  }

  private static string? GetContainingTextScopeMemberName(SyntaxNodeAnalysisContext context)
  {
    ISymbol? containingSymbol = context.ContainingSymbol;

    // For property getters, the containing symbol is the accessor - walk up to the property.
    if (containingSymbol is IMethodSymbol { AssociatedSymbol: IPropertySymbol property })
    {
      containingSymbol = property;
    }

    if (containingSymbol is null)
    {
      return null;
    }

    INamedTypeSymbol? containingType = containingSymbol.ContainingType;
    if (containingType is null || containingType.TypeKind is TypeKind.Interface)
    {
      return null;
    }

    if (!TextScopeHelper.ImplementsTextScopeMember(containingSymbol, containingType))
    {
      return null;
    }

    return containingSymbol.Name;
  }
}
