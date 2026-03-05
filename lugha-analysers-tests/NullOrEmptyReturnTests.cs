// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Lugha.Analysers.Tests;

/// <summary>
/// Tests for <see cref="NullOrEmptyReturnAnalyser"/> (LGH003).
/// </summary>
public sealed class NullOrEmptyReturnTests
{
  private const string Stubs = """
        namespace Lugha
        {
            public interface ITextScope { }
        }

        public interface IMyScope : Lugha.ITextScope
        {
            string Label { get; }
            string Format(string arg);
        }
        """;

  [Theory]
  [InlineData("null")]
  [InlineData("null!")]
  [InlineData("default")]
  [InlineData("default!")]
  [InlineData("\"\"")]
  [InlineData("string.Empty")]
  public async Task Prohibited_return_expression_produces_LGH003(string prohibitedExpression)
  {
    string source = Stubs + $$"""

            public sealed class MyImpl : IMyScope
            {
                public string Label => {|#0:{{prohibitedExpression}}|};
                public string Format(string arg) => arg;
            }
            """;

    var test = new CSharpAnalyzerTest<NullOrEmptyReturnAnalyser, DefaultVerifier>
    {
      TestCode = source,
      ExpectedDiagnostics =
            {
                new DiagnosticResult("LGH003", DiagnosticSeverity.Error)
                    .WithLocation(0)
                    .WithArguments("Label"),
            },
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };

    await test.RunAsync();
  }

  [Fact]
  public async Task Valid_string_return_produces_no_LGH003()
  {
    const string Source = Stubs + """

            public sealed class MyImpl : IMyScope
            {
                public string Label => "Hello";
                public string Format(string arg) => $"Value: {arg}";
            }
            """;

    var test = new CSharpAnalyzerTest<NullOrEmptyReturnAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };

    await test.RunAsync();
  }

  [Fact]
  public async Task Non_text_scope_returning_null_produces_no_LGH003()
  {
    const string Source = Stubs + """

            public sealed class Unrelated
            {
                public string? GetValue() => null;
            }
            """;

    var test = new CSharpAnalyzerTest<NullOrEmptyReturnAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };

    await test.RunAsync();
  }
}
