// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Lugha.Analysers;

/// <summary>
/// <b>LGH006</b> - Info (opt-in): a <c>PluralForms</c> initialiser
/// for a locale requiring more than two CLDR categories only sets <c>Other</c> and <c>One</c>.
/// </summary>
/// <remarks>
/// Detection is heuristic. The analyser looks for <c>PluralForms</c> object-creation
/// expressions, then attempts to determine the language from the containing class's
/// <c>Culture</c> property (specifically looking for a
/// <c>CultureInfo.GetCultureInfo("...")</c> argument). The expected cardinal rule
/// category count is hardcoded in this analyser because analysers cannot reference
/// the runtime assembly.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PluralCategoryCompletenessAnalyser : DiagnosticAnalyzer
{
  /// <summary>Diagnostic identifier.</summary>
  public const string DiagnosticId = "LGH006";

  private static readonly DiagnosticDescriptor Rule = new(
      DiagnosticId,
      title: "PluralForms may need additional categories",
      messageFormat: "PluralForms for language '{0}' may need categories beyond Other and One",
      category: "Lugha.Design",
      defaultSeverity: DiagnosticSeverity.Info,
      isEnabledByDefault: false,
      description: "A PluralForms initialiser for a locale requiring more than two CLDR categories only sets Other and One.");

  /// <inheritdoc />
  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
      [Rule];

  /// <inheritdoc />
  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();

    context.RegisterSyntaxNodeAction(AnalyseObjectCreation, SyntaxKind.ObjectCreationExpression);
    context.RegisterSyntaxNodeAction(AnalyseImplicitObjectCreation, SyntaxKind.ImplicitObjectCreationExpression);
  }

  private static void AnalyseObjectCreation(SyntaxNodeAnalysisContext context)
  {
    var creation = (ObjectCreationExpressionSyntax)context.Node;
    TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(creation, context.CancellationToken);
    if (typeInfo.Type is not INamedTypeSymbol createdType)
    {
      return;
    }

    if (createdType.Name != "PluralForms" || createdType.ContainingNamespace?.ToDisplayString() != "Lugha")
    {
      return;
    }

    AnalyseInitialiser(context, creation.Initializer, creation.GetLocation());
  }

  private static void AnalyseImplicitObjectCreation(SyntaxNodeAnalysisContext context)
  {
    var creation = (ImplicitObjectCreationExpressionSyntax)context.Node;
    TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(creation, context.CancellationToken);
    if (typeInfo.ConvertedType is not INamedTypeSymbol createdType)
    {
      return;
    }

    if (createdType.Name != "PluralForms" || createdType.ContainingNamespace?.ToDisplayString() != "Lugha")
    {
      return;
    }

    AnalyseInitialiser(context, creation.Initializer, creation.GetLocation());
  }

  private static void AnalyseInitialiser(
      SyntaxNodeAnalysisContext context,
      InitializerExpressionSyntax? initialiser,
      Location location)
  {
    if (initialiser is null)
    {
      return;
    }

    // Collect which properties are set in the initialiser.
    var setProperties = new HashSet<string>();
    foreach (ExpressionSyntax expr in initialiser.Expressions)
    {
      if (expr is AssignmentExpressionSyntax assignment &&
          assignment.Left is IdentifierNameSyntax identifier)
      {
        setProperties.Add(identifier.Identifier.Text);
      }
    }

    // Only trigger if exactly Other and One (or just Other) are set.
    bool hasTwo = setProperties.Contains("Two");
    bool hasFew = setProperties.Contains("Few");
    bool hasMany = setProperties.Contains("Many");
    bool hasZero = setProperties.Contains("Zero");

    if (hasTwo || hasFew || hasMany || hasZero)
    {
      return; // Developer has set additional categories - no diagnostic.
    }

    if (!setProperties.Contains("Other"))
    {
      return; // Incomplete for different reasons - not our concern.
    }

    // Try to detect the language from the containing class's Culture property.
    string? languageTag = TryDetectLanguageTag(context);
    if (languageTag is null)
    {
      return;
    }

    int expectedCategories = GetExpectedCategoryCount(languageTag);
    if (expectedCategories <= 2)
    {
      return; // Language only needs Other and One (or just Other).
    }

    context.ReportDiagnostic(Diagnostic.Create(Rule, location, languageTag));
  }

  private static string? TryDetectLanguageTag(SyntaxNodeAnalysisContext context)
  {
    // Walk up to the containing class declaration.
    SyntaxNode? node = context.Node;
    while (node is not null and not ClassDeclarationSyntax)
    {
      node = node.Parent;
    }

    if (node is not ClassDeclarationSyntax classDecl)
    {
      return null;
    }

    // Look for a Culture property with CultureInfo.GetCultureInfo("...").
    foreach (MemberDeclarationSyntax member in classDecl.Members)
    {
      if (member is not PropertyDeclarationSyntax propDecl)
      {
        continue;
      }

      if (propDecl.Identifier.Text != "Culture")
      {
        continue;
      }

      // Check expression body: Culture => CultureInfo.GetCultureInfo("...");
      if (propDecl.ExpressionBody is not null)
      {
        string? tag = ExtractCultureTag(propDecl.ExpressionBody.Expression);
        if (tag is not null)
        {
          return NormaliseTag(tag);
        }
      }

      // Check getter body.
      if (propDecl.AccessorList is not null)
      {
        foreach (AccessorDeclarationSyntax accessor in propDecl.AccessorList.Accessors)
        {
          if (!accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
          {
            continue;
          }

          if (accessor.ExpressionBody is not null)
          {
            string? tag = ExtractCultureTag(accessor.ExpressionBody.Expression);
            if (tag is not null)
            {
              return NormaliseTag(tag);
            }
          }

          if (accessor.Body is not null)
          {
            foreach (SyntaxNode desc in accessor.Body.DescendantNodes())
            {
              if (desc is ReturnStatementSyntax ret && ret.Expression is not null)
              {
                string? tag = ExtractCultureTag(ret.Expression);
                if (tag is not null)
                {
                  return NormaliseTag(tag);
                }
              }
            }
          }
        }
      }
    }

    // Also check field declarations (e.g. static readonly CultureInfo Culture = ...).
    foreach (MemberDeclarationSyntax fieldMember in classDecl.Members)
    {
      if (fieldMember is not FieldDeclarationSyntax fieldDecl)
      {
        continue;
      }

      foreach (VariableDeclaratorSyntax variable in fieldDecl.Declaration.Variables)
      {
        if (variable.Identifier.Text != "Culture")
        {
          continue;
        }

        if (variable.Initializer is not null)
        {
          string? tag = ExtractCultureTag(variable.Initializer.Value);
          if (tag is not null)
          {
            return NormaliseTag(tag);
          }
        }
      }
    }

    return null;
  }

  private static string? ExtractCultureTag(ExpressionSyntax expression)
  {
    // Match CultureInfo.GetCultureInfo("tag") or similar patterns.
    if (expression is not InvocationExpressionSyntax invocation ||
        invocation.ArgumentList.Arguments.Count == 0)
    {
      return null;
    }

    string? methodName = invocation.Expression switch
    {
      MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
      _ => null,
    };

    if (methodName != "GetCultureInfo")
    {
      return null;
    }

    ExpressionSyntax firstArg = invocation.ArgumentList.Arguments[0].Expression;
    if (firstArg is LiteralExpressionSyntax literal &&
        literal.IsKind(SyntaxKind.StringLiteralExpression))
    {
      return literal.Token.ValueText;
    }

    return null;
  }

  /// <summary>
  /// Normalises a culture tag to a base language tag for rule lookup.
  /// E.g. "en-GB" becomes "en", "pt-PT" stays "pt-PT" (special case),
  /// "pt-BR" becomes "pt".
  /// </summary>
  private static string NormaliseTag(string tag)
  {
    // pt-PT is a special case - keep as-is.
    if (tag == "pt-PT")
    {
      return tag;
    }

    int dashIndex = tag.IndexOf('-');
    return dashIndex >= 0 ? tag[..dashIndex] : tag;
  }

  /// <summary>
  /// Returns the expected number of CLDR cardinal categories for a language.
  /// Hardcoded because analysers cannot reference the runtime assembly.
  /// </summary>
  private static int GetExpectedCategoryCount(string languageTag) => languageTag switch
  {
    // 6 categories: zero, one, two, few, many, other
    "ar" => 6,
    "cy" => 6,

    // 4 categories: one, few, many, other
    "ru" => 4,
    "uk" => 4,
    "pl" => 4,

    // 3 categories: one, few, other
    "cs" => 3,
    "sk" => 3,
    "ro" => 3,

    // 3 categories: one, many, other
    "es" => 3,
    "it" => 3,
    "pt-PT" => 3,
    "ca" => 3,

    // 3 categories: one, two, other
    "he" => 3,

    // 2 categories: one, other (or fewer)
    "en" => 2,
    "de" => 2,
    "nl" => 2,
    "sv" => 2,
    "nb" => 2,
    "da" => 2,
    "fr" => 2,
    "pt" => 2,

    // 1 category: other
    "zh" => 1,
    "ja" => 1,
    "ko" => 1,

    _ => 0, // Unknown - do not report.
  };
}
