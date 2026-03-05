// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Lugha.Analysers.Tests;

/// <summary>
/// Tests for <see cref="Generators.LocaleCompletenessGenerator"/> (LGH002).
/// Uses <see cref="CSharpGeneratorDriver"/> directly since the project does
/// not reference the source-generator-specific testing package.
/// </summary>
public sealed class LocaleCompletenessTests
{
  private const string StubTypes = """
        using System.Globalization;

        namespace Lugha
        {
            public interface ITextScope { }

            public interface ILocale
            {
                CultureInfo Culture { get; }
            }
        }
        """;

  [Fact]
  public void Missing_scope_implementation_produces_LGH002()
  {
    const string Source = StubTypes + """

            public interface IConnectionText : Lugha.ITextScope
            {
                string Discovering { get; }
            }

            public interface IAppLocale : Lugha.ILocale
            {
                IConnectionText Connection { get; }
            }

            public sealed class EnGbLocale : IAppLocale
            {
                public System.Globalization.CultureInfo Culture =>
                    System.Globalization.CultureInfo.GetCultureInfo("en-GB");

                // Missing Connection property - should trigger LGH002.
            }
            """;

    ImmutableArray<Diagnostic> diagnostics = RunGenerator(Source);

    diagnostics.Should().ContainSingle(d => d.Id == "LGH002");
  }

  [Fact]
  public void Complete_locale_produces_no_LGH002()
  {
    const string Source = StubTypes + """

            public interface IConnectionText : Lugha.ITextScope
            {
                string Discovering { get; }
            }

            public sealed class EnGbConnectionText : IConnectionText
            {
                public string Discovering => "Discovering\u2026";
            }

            public interface IAppLocale : Lugha.ILocale
            {
                IConnectionText Connection { get; }
            }

            public sealed class EnGbLocale : IAppLocale
            {
                public System.Globalization.CultureInfo Culture =>
                    System.Globalization.CultureInfo.GetCultureInfo("en-GB");

                public IConnectionText Connection { get; } = new EnGbConnectionText();
            }
            """;

    ImmutableArray<Diagnostic> diagnostics = RunGenerator(Source);

    diagnostics.Should().NotContain(d => d.Id == "LGH002");
  }

  [Fact]
  public void Abstract_locale_class_is_not_checked()
  {
    const string Source = StubTypes + """

            public interface IConnectionText : Lugha.ITextScope
            {
                string Discovering { get; }
            }

            public interface IAppLocale : Lugha.ILocale
            {
                IConnectionText Connection { get; }
            }

            public abstract class BaseLocale : IAppLocale
            {
                public abstract System.Globalization.CultureInfo Culture { get; }
                public abstract IConnectionText Connection { get; }
            }
            """;

    ImmutableArray<Diagnostic> diagnostics = RunGenerator(Source);

    diagnostics.Should().NotContain(d => d.Id == "LGH002");
  }

  [Fact]
  public void Non_locale_class_is_not_checked()
  {
    const string Source = StubTypes + """

            public interface IConnectionText : Lugha.ITextScope
            {
                string Discovering { get; }
            }

            public sealed class NotALocale
            {
                public string Name => "Test";
            }
            """;

    ImmutableArray<Diagnostic> diagnostics = RunGenerator(Source);

    diagnostics.Should().NotContain(d => d.Id == "LGH002");
  }

  private static ImmutableArray<Diagnostic> RunGenerator(string source)
  {
    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

    string[] trustedAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty)
        .Split(Path.PathSeparator);

    var references = new List<MetadataReference>();
    foreach (string path in trustedAssemblies)
    {
      if (File.Exists(path))
      {
        references.Add(MetadataReference.CreateFromFile(path));
      }
    }

    var compilation = CSharpCompilation.Create(
        "TestAssembly",
        [syntaxTree],
        references,
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    var generator = new Generators.LocaleCompletenessGenerator();
    GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
    driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out ImmutableArray<Diagnostic> diagnostics);

    return diagnostics;
  }
}
