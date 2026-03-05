// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Lugha.Import.Gettext;

/// <summary>
/// Incremental source generator that reads <c>.po</c> / <c>.pot</c> files
/// from <see cref="AdditionalText"/> and emits typed <c>ITextScope</c>
/// interfaces and sealed locale implementations via
/// <see cref="GettextParser"/> and <see cref="CodeEmitter"/>.
/// </summary>
/// <remarks>
/// <para>
/// The generator scans <c>AdditionalFiles</c> for <c>.pot</c> (template) and
/// <c>.po</c> (locale) files. A <c>.pot</c> file emits contract interfaces.
/// Each <c>.po</c> file emits a sealed implementation class per text scope.
/// If no <c>.pot</c> is present, the first <c>.po</c> file is used as the
/// reference for contract emission.
/// </para>
/// <para>
/// The target namespace for generated code is determined by the
/// <c>LughaNamespace</c> MSBuild property. If not set, it defaults to
/// <c>"Lugha.Generated"</c>.
/// </para>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class GettextSourceGenerator : IIncrementalGenerator
{
  private const string DefaultNamespace = "Lugha.Generated";

  /// <inheritdoc />
  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    // Collect the target namespace from AnalyzerConfigOptions
    IncrementalValueProvider<string> targetNamespace = context.AnalyzerConfigOptionsProvider
        .Select(static (options, _) =>
        {
          options.GlobalOptions.TryGetValue("build_property.LughaNamespace", out string? ns);
          return ns ?? DefaultNamespace;
        });

    // Filter AdditionalFiles for .po/.pot extensions
    IncrementalValuesProvider<AdditionalText> poFiles = context.AdditionalTextsProvider
        .Where(static file =>
        {
          string ext = Path.GetExtension(file.Path);
          return ext.Equals(".po", StringComparison.OrdinalIgnoreCase)
                  || ext.Equals(".pot", StringComparison.OrdinalIgnoreCase);
        });

    // Collect all .po/.pot files and combine with namespace
    IncrementalValueProvider<(ImmutableArray<AdditionalText> Files, string Namespace)> combined =
        poFiles.Collect().Combine(targetNamespace);

    // Register source output
    context.RegisterSourceOutput(combined, static (spc, source) =>
        Execute(spc, source.Files, source.Namespace));
  }

  private static void Execute(
      SourceProductionContext context,
      ImmutableArray<AdditionalText> files,
      string targetNamespace)
  {
    if (files.IsDefaultOrEmpty)
    {
      return;
    }

    // Separate templates (.pot) from locale files (.po)
    AdditionalText? templateFile = null;
    var localeFiles = new System.Collections.Generic.List<AdditionalText>();

    foreach (AdditionalText file in files)
    {
      string ext = Path.GetExtension(file.Path);
      if (ext.Equals(".pot", StringComparison.OrdinalIgnoreCase))
      {
        templateFile ??= file;
      }
      else
      {
        localeFiles.Add(file);
      }
    }

    // Emit contracts from template, or first locale as fallback
    TranslationSet? referenceSet = null;
    if (templateFile is not null)
    {
      Microsoft.CodeAnalysis.Text.SourceText? sourceText =
          templateFile.GetText(context.CancellationToken);
      if (sourceText is not null)
      {
        referenceSet = GettextParser.ParseTemplate(sourceText.ToString());
      }
    }
    else if (localeFiles.Count > 0)
    {
      Microsoft.CodeAnalysis.Text.SourceText? sourceText =
          localeFiles[0].GetText(context.CancellationToken);
      if (sourceText is not null)
      {
        referenceSet = GettextParser.Parse(sourceText.ToString());
      }
    }

    if (referenceSet is not null)
    {
      System.Collections.Generic.IReadOnlyList<EmittedFile> contracts =
          CodeEmitter.EmitContracts(referenceSet, targetNamespace);

      foreach (EmittedFile contract in contracts)
      {
        context.AddSource(contract.FileName, contract.Content);
      }
    }

    // Emit implementations for each locale .po file
    foreach (AdditionalText localeFile in localeFiles)
    {
      context.CancellationToken.ThrowIfCancellationRequested();

      Microsoft.CodeAnalysis.Text.SourceText? sourceText =
          localeFile.GetText(context.CancellationToken);
      if (sourceText is null)
      {
        continue;
      }

      TranslationSet localeSet = GettextParser.Parse(sourceText.ToString());

      System.Collections.Generic.IReadOnlyList<EmittedFile> implementations =
          CodeEmitter.EmitImplementations(localeSet, targetNamespace, targetNamespace);

      foreach (EmittedFile impl in implementations)
      {
        context.AddSource(impl.FileName, impl.Content);
      }
    }
  }
}
