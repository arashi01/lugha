// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lugha.Generators;

/// <summary>
/// <b>LGH002</b> - Error: every concrete locale class implementing <c>ILocale</c>
/// must provide all <c>ITextScope</c> properties declared on its composite locale
/// interface.
/// </summary>
/// <remarks>
/// Uses the incremental generator pipeline for compile-time performance.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class LocaleCompletenessGenerator : IIncrementalGenerator
{
  /// <summary>Diagnostic identifier.</summary>
  public const string DiagnosticId = "LGH002";

  private static readonly DiagnosticDescriptor Rule = new(
      DiagnosticId,
      title: "Locale does not implement required text scope",
      messageFormat: "Locale '{0}' does not implement text scope '{1}' required by '{2}'",
      category: "Lugha.Design",
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "Every concrete locale class implementing ILocale must provide all ITextScope properties declared on its composite locale interface.");

  /// <inheritdoc />
  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    // Step 1: Filter class declarations that might be locale implementations.
    IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations =
        context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => node is ClassDeclarationSyntax { BaseList: not null },
            transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node);

    // Step 2: Combine with compilation for semantic analysis.
    IncrementalValueProvider<(Compilation Compilation, ImmutableArray<ClassDeclarationSyntax> Classes)> combined =
        context.CompilationProvider.Combine(classDeclarations.Collect());

    // Step 3: Emit diagnostics.
    context.RegisterSourceOutput(combined, static (spc, source) =>
    {
      (Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes) = source;
      Execute(compilation, classes, spc);
    });
  }

  private static void Execute(
      Compilation compilation,
      ImmutableArray<ClassDeclarationSyntax> classes,
      SourceProductionContext context)
  {
    if (classes.IsDefaultOrEmpty)
    {
      return;
    }

    INamedTypeSymbol? textScopeSymbol = compilation.GetTypeByMetadataName(GeneratorTextScopeHelper.TextScopeFullName);
    if (textScopeSymbol is null)
    {
      return;
    }

    foreach (ClassDeclarationSyntax classDecl in classes.Distinct())
    {
      SemanticModel model = compilation.GetSemanticModel(classDecl.SyntaxTree);
      if (model.GetDeclaredSymbol(classDecl, context.CancellationToken) is not INamedTypeSymbol classSymbol)
      {
        continue;
      }

      // Skip abstract classes.
      if (classSymbol.IsAbstract)
      {
        continue;
      }

      // Must implement ILocale.
      if (!GeneratorTextScopeHelper.ImplementsLocale(classSymbol))
      {
        continue;
      }

      // Find all composite locale interfaces (interfaces with ITextScope-returning properties).
      foreach (INamedTypeSymbol iface in classSymbol.AllInterfaces)
      {
        // Skip ILocale itself and ITextScope-derived interfaces.
        string ifaceDisplay = iface.OriginalDefinition.ToDisplayString();
        if (ifaceDisplay == GeneratorTextScopeHelper.LocaleFullName ||
            ifaceDisplay.StartsWith(GeneratorTextScopeHelper.LocaleFullName + "<", System.StringComparison.Ordinal))
        {
          continue;
        }

        if (GeneratorTextScopeHelper.IsTextScopeInterface(iface) ||
            GeneratorTextScopeHelper.IsTextScopeMarker(iface))
        {
          continue;
        }

        // Check if this interface has ITextScope-returning properties (composite locale interface).
        foreach (ISymbol member in iface.GetMembers())
        {
          if (member is not IPropertySymbol property)
          {
            continue;
          }

          if (property.Type is not INamedTypeSymbol propertyType)
          {
            continue;
          }

          if (!GeneratorTextScopeHelper.IsTextScopeInterface(propertyType))
          {
            continue;
          }

          // This is a text scope property on a composite locale interface.
          // Check that the concrete class provides an implementation for this property.
          ISymbol? implementation = classSymbol.FindImplementationForInterfaceMember(property);

          if (implementation is null)
          {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    Rule,
                    classDecl.Identifier.GetLocation(),
                    classSymbol.Name,
                    propertyType.Name,
                    iface.Name));
          }
        }
      }
    }
  }
}
