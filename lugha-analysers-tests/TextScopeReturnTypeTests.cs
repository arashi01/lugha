// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Lugha.Analysers.Tests;

/// <summary>
/// Tests for <see cref="TextScopeReturnTypeAnalyser"/> (LGH001).
/// </summary>
public sealed class TextScopeReturnTypeTests
{
  private const string TextScopeStub = """
        namespace Lugha
        {
            public interface ITextScope { }
        }
        """;

  [Fact]
  public async Task Non_string_property_on_text_scope_produces_LGH001()
  {
    const string Source = """
            namespace Lugha
            {
                public interface ITextScope { }
            }

            public interface IMyScope : Lugha.ITextScope
            {
                int {|#0:BadMember|} { get; }
            }
            """;

    var test = new CSharpAnalyzerTest<TextScopeReturnTypeAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ExpectedDiagnostics =
            {
                new DiagnosticResult("LGH001", DiagnosticSeverity.Error)
                    .WithLocation(0)
                    .WithArguments("BadMember"),
            },
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };

    await test.RunAsync();
  }

  [Fact]
  public async Task Non_string_method_on_text_scope_produces_LGH001()
  {
    const string Source = """
            namespace Lugha
            {
                public interface ITextScope { }
            }

            public interface IMyScope : Lugha.ITextScope
            {
                int {|#0:BadMethod|}(string arg);
            }
            """;

    var test = new CSharpAnalyzerTest<TextScopeReturnTypeAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ExpectedDiagnostics =
            {
                new DiagnosticResult("LGH001", DiagnosticSeverity.Error)
                    .WithLocation(0)
                    .WithArguments("BadMethod"),
            },
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };

    await test.RunAsync();
  }

  [Fact]
  public async Task String_property_on_text_scope_produces_no_diagnostic()
  {
    const string Source = TextScopeStub + """

            public interface IMyScope : Lugha.ITextScope
            {
                string GoodMember { get; }
            }
            """;

    var test = new CSharpAnalyzerTest<TextScopeReturnTypeAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };

    await test.RunAsync();
  }

  [Fact]
  public async Task String_method_on_text_scope_produces_no_diagnostic()
  {
    const string Source = TextScopeStub + """

            public interface IMyScope : Lugha.ITextScope
            {
                string GoodMethod(string arg);
            }
            """;

    var test = new CSharpAnalyzerTest<TextScopeReturnTypeAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };

    await test.RunAsync();
  }

  [Fact]
  public async Task Non_text_scope_interface_with_non_string_member_produces_no_diagnostic()
  {
    const string Source = TextScopeStub + """

            public interface IUnrelated
            {
                int NotATextScopeMember { get; }
            }
            """;

    var test = new CSharpAnalyzerTest<TextScopeReturnTypeAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };

    await test.RunAsync();
  }
}
