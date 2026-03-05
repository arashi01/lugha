// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.IO;
using Lugha.Import;
using Lugha.Import.Gettext;
using Lugha.Import.Resx;

namespace Lugha.Cli;

/// <summary>
/// Entry point for the <c>lugha</c> CLI global tool.
/// </summary>
internal static class Program
{
  private static int Main(string[] args)
  {
    if (args.Length == 0)
    {
      PrintUsage();
      return 1;
    }

    string command = args[0];

    if (string.Equals(command, "import", StringComparison.OrdinalIgnoreCase))
    {
      return RunImport(args.AsSpan(1));
    }

    Console.Error.WriteLine($"Unknown command: {command}");
    PrintUsage();
    return 1;
  }

  private static int RunImport(ReadOnlySpan<string> args)
  {
    string? format = null;
    string? source = null;
    string? targetNamespace = null;
    string? output = null;
    string? language = null;
    bool contracts = false;

    for (int i = 0; i < args.Length; i++)
    {
      string arg = args[i];

      if (string.Equals(arg, "--contracts", StringComparison.OrdinalIgnoreCase))
      {
        contracts = true;
      }
      else if (string.Equals(arg, "--format", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
      {
        format = args[++i];
      }
      else if (string.Equals(arg, "--source", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
      {
        source = args[++i];
      }
      else if (string.Equals(arg, "--namespace", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
      {
        targetNamespace = args[++i];
      }
      else if (string.Equals(arg, "--output", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
      {
        output = args[++i];
      }
      else if (string.Equals(arg, "--language", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
      {
        language = args[++i];
      }
      else
      {
        Console.Error.WriteLine($"Unknown argument: {arg}");
        PrintImportUsage();
        return 1;
      }
    }

    if (format is null || source is null || targetNamespace is null || output is null)
    {
      Console.Error.WriteLine("Missing required arguments.");
      PrintImportUsage();
      return 1;
    }

    if (!File.Exists(source))
    {
      Console.Error.WriteLine($"Source file not found: {source}");
      return 1;
    }

    string content = File.ReadAllText(source);
    TranslationSet translationSet;

    switch (format.ToUpperInvariant())
    {
      case "PO":
        translationSet = contracts && source.EndsWith(".pot", StringComparison.OrdinalIgnoreCase)
            ? GettextParser.ParseTemplate(content.AsSpan())
            : GettextParser.Parse(content.AsSpan());
        break;
      case "RESX":
        if (language is null && !contracts)
        {
          Console.Error.WriteLine("--language is required for resx format when not emitting contracts.");
          return 1;
        }

        translationSet = ResxParser.Parse(content.AsSpan(), language ?? "en");
        break;
      default:
        Console.Error.WriteLine($"Unsupported format: {format}. Supported formats: po, resx");
        return 1;
    }

    IReadOnlyList<EmittedFile> files;

    if (contracts)
    {
      files = CodeEmitter.EmitContracts(translationSet, targetNamespace);
    }
    else
    {
      // For implementations, use the translation set's language to derive
      // the contract namespace. By convention the contract namespace is the
      // parent of the implementation namespace.
      int lastDot = targetNamespace.LastIndexOf('.');
      string contractNamespace = lastDot > 0
          ? targetNamespace[..lastDot]
          : targetNamespace;
      files = CodeEmitter.EmitImplementations(translationSet, contractNamespace, targetNamespace);
    }

    Directory.CreateDirectory(output);

    foreach (EmittedFile file in files)
    {
      string path = Path.Combine(output, file.FileName);
      File.WriteAllText(path, file.Content);
      Console.WriteLine(path);
    }

    return 0;
  }

  private static void PrintUsage()
  {
    Console.Error.WriteLine("Usage: lugha <command>");
    Console.Error.WriteLine();
    Console.Error.WriteLine("Commands:");
    Console.Error.WriteLine("  import    Import translations from external sources");
  }

  private static void PrintImportUsage()
  {
    Console.Error.WriteLine("Usage: lugha import --format <po|resx> --source <file> --namespace <ns> --output <dir> [--contracts] [--language <tag>]");
    Console.Error.WriteLine();
    Console.Error.WriteLine("Options:");
    Console.Error.WriteLine("  --format       Translation file format (po, resx)");
    Console.Error.WriteLine("  --source       Path to the translation source file");
    Console.Error.WriteLine("  --namespace    Target C# namespace for generated code");
    Console.Error.WriteLine("  --output       Output directory for generated files");
    Console.Error.WriteLine("  --contracts    Emit ITextScope interfaces (contracts) from a reference locale");
    Console.Error.WriteLine("  --language     BCP 47 language tag (required for resx when not emitting contracts)");
  }
}
