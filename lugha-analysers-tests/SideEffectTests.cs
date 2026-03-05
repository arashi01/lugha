// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Lugha.Analysers.Tests;

/// <summary>
/// Tests for <see cref="SideEffectAnalyser"/> (LGH008).
/// </summary>
public sealed class SideEffectTests
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

  [Fact]
  public async Task Console_call_in_text_scope_produces_LGH008()
  {
    const string Source = """
            namespace Lugha
            {
                public interface ITextScope { }
            }

            public interface IMyScope : Lugha.ITextScope
            {
                string Label { get; }
                string Format(string arg);
            }

            public sealed class MyImpl : IMyScope
            {
                public string Label
                {
                    get
                    {
                        {|#0:System.Console.WriteLine("side effect")|};
                        return "Hello";
                    }
                }

                public string Format(string arg) => arg;
            }
            """;

    var test = new CSharpAnalyzerTest<SideEffectAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ExpectedDiagnostics =
            {
                new DiagnosticResult("LGH008", DiagnosticSeverity.Warning)
                    .WithLocation(0)
                    .WithArguments("Label", "Console.WriteLine"),
            },
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };

    await test.RunAsync();
  }

  [Fact]
  public async Task Throw_non_argument_exception_in_text_scope_produces_LGH008()
  {
    const string Source = """
            namespace Lugha
            {
                public interface ITextScope { }
            }

            public interface IMyScope : Lugha.ITextScope
            {
                string Label { get; }
                string Format(string arg);
            }

            public sealed class MyImpl : IMyScope
            {
                public string Label => {|#0:throw new System.InvalidOperationException("bad")|};
                public string Format(string arg) => arg;
            }
            """;

    var test = new CSharpAnalyzerTest<SideEffectAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ExpectedDiagnostics =
            {
                new DiagnosticResult("LGH008", DiagnosticSeverity.Warning)
                    .WithLocation(0)
                    .WithArguments("Label", "throw"),
            },
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };

    await test.RunAsync();
  }

  [Fact]
  public async Task Pure_implementation_produces_no_LGH008()
  {
    const string Source = Stubs + """

            public sealed class MyImpl : IMyScope
            {
                public string Label => "Hello";
                public string Format(string arg) => $"Value: {arg}";
            }
            """;

    var test = new CSharpAnalyzerTest<SideEffectAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };

    await test.RunAsync();
  }

  [Fact]
  public async Task Throw_ArgumentException_in_text_scope_produces_no_LGH008()
  {
    const string Source = Stubs + """

            public sealed class MyImpl : IMyScope
            {
                public string Label => "Hello";
                public string Format(string arg) =>
                    string.IsNullOrEmpty(arg) ? throw new System.ArgumentException("arg") : arg;
            }
            """;

    var test = new CSharpAnalyzerTest<SideEffectAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };

    await test.RunAsync();
  }

  [Fact]
  public async Task Non_text_scope_with_side_effect_produces_no_diagnostic()
  {
    const string Source = Stubs + """

            public sealed class Unrelated
            {
                public string Label
                {
                    get
                    {
                        System.Console.WriteLine("side effect");
                        return "Hello";
                    }
                }
            }
            """;

    var test = new CSharpAnalyzerTest<SideEffectAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };

    await test.RunAsync();
  }
}
