// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Lugha.Analysers;

/// <summary>
/// <b>LGH001</b> - Error: every member of an <c>ITextScope</c>-derived interface
/// must return <see langword="string"/>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TextScopeReturnTypeAnalyser : DiagnosticAnalyzer
{
  /// <summary>Diagnostic identifier.</summary>
  public const string DiagnosticId = "LGH001";

  private static readonly DiagnosticDescriptor Rule = new(
      DiagnosticId,
      title: "Text scope member must return string",
      messageFormat: "Text scope member '{0}' must return string",
      category: "Lugha.Design",
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "All members of an ITextScope-derived interface must return string.");

  /// <inheritdoc />
  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
      [Rule];

  /// <inheritdoc />
  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();

    context.RegisterSymbolAction(AnalyseNamedType, SymbolKind.NamedType);
  }

  private static void AnalyseNamedType(SymbolAnalysisContext context)
  {
    var typeSymbol = (INamedTypeSymbol)context.Symbol;

    if (!TextScopeHelper.IsTextScopeInterface(typeSymbol))
    {
      return;
    }

    INamedTypeSymbol stringType = context.Compilation.GetSpecialType(SpecialType.System_String);

    foreach (ISymbol member in typeSymbol.GetMembers())
    {
      if (member.IsImplicitlyDeclared)
      {
        continue;
      }

      ITypeSymbol? returnType = member switch
      {
        IPropertySymbol property => property.Type,
        IMethodSymbol method when method.MethodKind is MethodKind.Ordinary => method.ReturnType,
        _ => null,
      };

      if (returnType is null)
      {
        continue;
      }

      if (!SymbolEqualityComparer.Default.Equals(returnType, stringType))
      {
        context.ReportDiagnostic(
            Diagnostic.Create(Rule, member.Locations[0], member.Name));
      }
    }
  }
}
