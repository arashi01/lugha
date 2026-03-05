// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Lugha.Analysers.Tests;

/// <summary>
/// Tests for <see cref="PluralCategoryCompletenessAnalyser"/> (LGH006).
/// Opt-in diagnostic — must be explicitly enabled via editor config.
/// </summary>
public sealed class PluralCategoryCompletenessTests
{
  // Simplified PluralForms stub — avoids init/required which need modern reference assemblies.
  private const string Stubs = """
        using System.Globalization;

        namespace Lugha
        {
            public interface ITextScope { }

            public struct PluralForms
            {
                public string Other { get; set; }
                public string One { get; set; }
                public string Two { get; set; }
                public string Few { get; set; }
                public string Many { get; set; }
                public string Zero { get; set; }
            }
        }
        """;

  private static readonly (string Key, string Value) EnableLgh006 =
      ("/.editorconfig", """
            root = true
            [*]
            dotnet_diagnostic.LGH006.severity = suggestion
            """);

  [Fact]
  public async Task Arabic_plural_forms_with_only_two_categories_produces_LGH006()
  {
    const string Source = """
            using System.Globalization;

            namespace Lugha
            {
                public interface ITextScope { }

                public struct PluralForms
                {
                    public string Other { get; set; }
                    public string One { get; set; }
                    public string Two { get; set; }
                    public string Few { get; set; }
                    public string Many { get; set; }
                    public string Zero { get; set; }
                }
            }

            public interface IMyScope : Lugha.ITextScope
            {
                string PeopleFound(int count);
            }

            public sealed class ArImpl : IMyScope
            {
                private static readonly CultureInfo Culture =
                    CultureInfo.GetCultureInfo("ar");

                private static readonly Lugha.PluralForms PersonForms = {|#0:new Lugha.PluralForms
                {
                    Other = "أشخاص",
                    One = "شخص",
                }|};

                public string PeopleFound(int count) => PersonForms.Other;
            }
            """;

    var test = new CSharpAnalyzerTest<PluralCategoryCompletenessAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ExpectedDiagnostics =
            {
                new DiagnosticResult("LGH006", DiagnosticSeverity.Info)
                    .WithLocation(0)
                    .WithArguments("ar"),
            },
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };
    test.TestState.AnalyzerConfigFiles.Add(EnableLgh006);

    await test.RunAsync();
  }

  [Fact]
  public async Task English_plural_forms_with_two_categories_produces_no_diagnostic()
  {
    const string Source = Stubs + """

            public interface IMyScope : Lugha.ITextScope
            {
                string PeopleFound(int count);
            }

            public sealed class EnImpl : IMyScope
            {
                private static readonly System.Globalization.CultureInfo Culture =
                    System.Globalization.CultureInfo.GetCultureInfo("en");

                private static readonly Lugha.PluralForms PersonForms = new Lugha.PluralForms
                {
                    Other = "people",
                    One = "person",
                };

                public string PeopleFound(int count) => PersonForms.Other;
            }
            """;

    var test = new CSharpAnalyzerTest<PluralCategoryCompletenessAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };
    test.TestState.AnalyzerConfigFiles.Add(EnableLgh006);

    await test.RunAsync();
  }

}
