// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Lugha.Import.Resx;

/// <summary>
/// Incremental source generator that reads <c>.resx</c> / <c>.resw</c> files
/// from <see cref="AdditionalText"/> and emits typed <c>ITextScope</c>
/// interfaces and sealed locale implementations via
/// <see cref="ResxParser"/> and <see cref="CodeEmitter"/>.
/// </summary>
/// <remarks>
/// <para>
/// The generator scans <c>AdditionalFiles</c> for <c>.resx</c> and <c>.resw</c>
/// files. The language tag is derived from the file name convention
/// (e.g. <c>Strings.es-ES.resx</c> -> <c>"es-ES"</c>). A file without a language
/// segment (e.g. <c>Strings.resx</c>) is treated as the reference locale for
/// contract emission.
/// </para>
/// <para>
/// The target namespace for generated code is determined by the
/// <c>LughaNamespace</c> MSBuild property. If not set, it defaults to
/// <c>"Lugha.Generated"</c>. The default reference language is determined by
/// the <c>LughaDefaultLanguage</c> property (defaults to <c>"en"</c>).
/// </para>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class ResxSourceGenerator : IIncrementalGenerator
{
  private const string DefaultNamespace = "Lugha.Generated";
  private const string DefaultLanguage = "en";

  /// <inheritdoc />
  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    // Collect the target namespace from AnalyzerConfigOptions
    IncrementalValueProvider<(string Namespace, string DefaultLang)> options =
        context.AnalyzerConfigOptionsProvider
            .Select(static (opts, _) =>
            {
              opts.GlobalOptions.TryGetValue("build_property.LughaNamespace", out string? ns);
              opts.GlobalOptions.TryGetValue("build_property.LughaDefaultLanguage", out string? lang);
              return (ns ?? DefaultNamespace, lang ?? DefaultLanguage);
            });

    // Filter AdditionalFiles for .resx/.resw extensions
    IncrementalValuesProvider<AdditionalText> resxFiles = context.AdditionalTextsProvider
        .Where(static file =>
        {
          string ext = Path.GetExtension(file.Path);
          return ext.Equals(".resx", StringComparison.OrdinalIgnoreCase)
                  || ext.Equals(".resw", StringComparison.OrdinalIgnoreCase);
        });

    // Collect all files and combine with options
    IncrementalValueProvider<(ImmutableArray<AdditionalText> Files, (string Namespace, string DefaultLang) Options)> combined =
        resxFiles.Collect().Combine(options);

    // Register source output
    context.RegisterSourceOutput(combined, static (spc, source) =>
        Execute(spc, source.Files, source.Options.Namespace, source.Options.DefaultLang));
  }

  private static void Execute(
      SourceProductionContext context,
      ImmutableArray<AdditionalText> files,
      string targetNamespace,
      string defaultLanguage)
  {
    if (files.IsDefaultOrEmpty)
    {
      return;
    }

    // Separate reference file from locale files
    AdditionalText? referenceFile = null;
    var localeFiles = new System.Collections.Generic.List<(AdditionalText File, string Language)>();

    foreach (AdditionalText file in files)
    {
      string language = ExtractLanguageFromPath(file.Path);
      if (language.Length == 0)
      {
        // No language segment - this is the reference file
        referenceFile ??= file;
      }
      else
      {
        localeFiles.Add((file, language));
      }
    }

    // Emit contracts from reference file
    if (referenceFile is not null)
    {
      Microsoft.CodeAnalysis.Text.SourceText? sourceText =
          referenceFile.GetText(context.CancellationToken);
      if (sourceText is not null)
      {
        EmitContracts(context, ResxParser.Parse(sourceText.ToString(), defaultLanguage), targetNamespace);
      }
    }
    else if (localeFiles.Count > 0)
    {
      // Use first locale file as reference
      (AdditionalText firstFile, string firstLang) = localeFiles[0];
      Microsoft.CodeAnalysis.Text.SourceText? sourceText =
          firstFile.GetText(context.CancellationToken);
      if (sourceText is not null)
      {
        EmitContracts(context, ResxParser.Parse(sourceText.ToString(), firstLang), targetNamespace);
      }
    }

    // Emit implementations for each locale file
    foreach ((AdditionalText localeFile, string language) in localeFiles)
    {
      context.CancellationToken.ThrowIfCancellationRequested();

      Microsoft.CodeAnalysis.Text.SourceText? sourceText =
          localeFile.GetText(context.CancellationToken);
      if (sourceText is null)
      {
        continue;
      }

      TranslationSet localeSet = ResxParser.Parse(sourceText.ToString(), language);

      System.Collections.Generic.IReadOnlyList<EmittedFile> implementations =
          CodeEmitter.EmitImplementations(localeSet, targetNamespace, targetNamespace);

      foreach (EmittedFile impl in implementations)
      {
        context.AddSource(impl.FileName, impl.Content);
      }
    }
  }

  private static void EmitContracts(
      SourceProductionContext context,
      TranslationSet referenceSet,
      string targetNamespace)
  {
    System.Collections.Generic.IReadOnlyList<EmittedFile> contracts =
        CodeEmitter.EmitContracts(referenceSet, targetNamespace);

    foreach (EmittedFile contract in contracts)
    {
      context.AddSource(contract.FileName, contract.Content);
    }
  }

  /// <summary>
  /// Extracts the language tag from a resx file path.
  /// E.g. <c>"Strings.es-ES.resx"</c> -> <c>"es-ES"</c>,
  /// <c>"Strings.resx"</c> -> <c>""</c> (reference file).
  /// </summary>
  private static string ExtractLanguageFromPath(string path)
  {
    string fileName = Path.GetFileNameWithoutExtension(path);

    // Look for a language segment: "Name.lang" or "Name.lang-REGION"
    int lastDot = fileName.LastIndexOf('.');
    if (lastDot < 0)
    {
      return "";
    }

    string candidate = fileName[(lastDot + 1)..];

    // Validate it looks like a language tag (2-3 letter code, optionally with region)
    if (candidate.Length >= 2 && char.IsLetter(candidate[0]) && char.IsLetter(candidate[1]))
    {
      return candidate;
    }

    return "";
  }
}
