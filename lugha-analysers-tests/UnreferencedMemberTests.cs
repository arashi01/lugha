// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Lugha.Analysers.Tests;

/// <summary>
/// Tests for <see cref="UnreferencedMemberAnalyser"/> (LGH007).
/// Opt-in diagnostic — must be explicitly enabled via editor config.
/// </summary>
public sealed class UnreferencedMemberTests
{
  private const string Stubs = """
        namespace Lugha
        {
            public interface ITextScope { }
        }
        """;

  private static readonly (string Key, string Value) EnableLgh007 =
      ("/.editorconfig", """
            root = true
            [*]
            dotnet_diagnostic.LGH007.severity = suggestion
            """);

  [Fact]
  public async Task Unreferenced_text_scope_member_produces_LGH007()
  {
    const string Source = """
            namespace Lugha
            {
                public interface ITextScope { }
            }

            public interface IMyScope : Lugha.ITextScope
            {
                string UsedMember { get; }
                string {|#0:UnusedMember|} { get; }
            }

            public sealed class MyImpl : IMyScope
            {
                public string UsedMember => "Hello";
                public string UnusedMember => "World";
            }

            public static class Consumer
            {
                public static string Get(IMyScope scope) => scope.UsedMember;
            }
            """;

    var test = new CSharpAnalyzerTest<UnreferencedMemberAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ExpectedDiagnostics =
            {
                new DiagnosticResult("LGH007", DiagnosticSeverity.Info)
                    .WithLocation(0)
                    .WithArguments("IMyScope", "UnusedMember"),
            },
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };
    test.TestState.AnalyzerConfigFiles.Add(EnableLgh007);

    await test.RunAsync();
  }

  [Fact]
  public async Task All_members_referenced_produces_no_diagnostic()
  {
    const string Source = Stubs + """

            public interface IMyScope : Lugha.ITextScope
            {
                string Label { get; }
            }

            public sealed class MyImpl : IMyScope
            {
                public string Label => "Hello";
            }

            public static class Consumer
            {
                public static string Get(IMyScope scope) => scope.Label;
            }
            """;

    var test = new CSharpAnalyzerTest<UnreferencedMemberAnalyser, DefaultVerifier>
    {
      TestCode = Source,
      ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
    };
    test.TestState.AnalyzerConfigFiles.Add(EnableLgh007);

    await test.RunAsync();
  }

}
