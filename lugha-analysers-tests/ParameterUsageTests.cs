// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Lugha.Analysers.Tests;

/// <summary>
/// Tests for <see cref="ParameterUsageAnalyser"/> (LGH004).
/// </summary>
public sealed class ParameterUsageTests
{
  private const string Stubs = """
        namespace Lugha
        {
            public interface ITextScope { }
        }

        public interface IMyScope : Lugha.ITextScope
        {
            string Format(string host, string port);
        }
        """;

  [Fact]
  public async Task Unused_parameter_produces_LGH004()
  {
    const string Source = Stubs + """

            public sealed class MyImpl : IMyScope
            {
                public string Format(string host, string {|#0:port|}) => $"Host: {host}";
            }
            """;

    var test = new CSharpAnalyzerTest<ParameterUsageAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ExpectedDiagnostics =
            {
                new DiagnosticResult("LGH004", DiagnosticSeverity.Warning)
                    .WithLocation(0)
                    .WithArguments("port", "Format"),
            },
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };

    await test.RunAsync();
  }

  [Fact]
  public async Task All_parameters_used_produces_no_diagnostic()
  {
    const string Source = Stubs + """

            public sealed class MyImpl : IMyScope
            {
                public string Format(string host, string port) => $"{host}:{port}";
            }
            """;

    var test = new CSharpAnalyzerTest<ParameterUsageAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };

    await test.RunAsync();
  }

  [Fact]
  public async Task Non_text_scope_method_with_unused_parameter_produces_no_diagnostic()
  {
    const string Source = Stubs + """

            public sealed class Unrelated
            {
                public string Format(string host, string port) => $"Host: {host}";
            }
            """;

    var test = new CSharpAnalyzerTest<ParameterUsageAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };

    await test.RunAsync();
  }
}
