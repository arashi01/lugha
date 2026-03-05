// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Import.Resx;

namespace Lugha.Import.Tests;

/// <summary>
/// Tests for <see cref="ResxParser"/>.
/// </summary>
public sealed class ResxParserTests
{
  private const string MinimalResxWrapper = """
        <?xml version="1.0" encoding="utf-8"?>
        <root>
          <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>
          {0}
        </root>
        """;

  private static string WrapResx(string dataElements) =>
      MinimalResxWrapper.Replace("{0}", dataElements, StringComparison.Ordinal);

  [Fact]
  public void Parse_SimpleEntries_ReturnsCorrectEntries()
  {
    string resx = WrapResx("""
            <data name="Connection.Discovering" xml:space="preserve">
              <value>Discovering…</value>
            </data>
            <data name="Connection.Connected" xml:space="preserve">
              <value>Connected</value>
            </data>
            """);

    TranslationSet result = ResxParser.Parse(resx, "en-GB");

    result.Language.Should().Be("en-GB");
    result.Entries.Should().HaveCount(2);
    result.Entries[0].Key.Should().Be("Connection.Discovering");
    result.Entries[0].Value.Should().Be("Discovering\u2026");
    result.Entries[0].Parameters.Should().BeEmpty();
    result.Entries[0].PluralForms.Should().BeNull();
    result.Entries[1].Key.Should().Be("Connection.Connected");
    result.Entries[1].Value.Should().Be("Connected");
  }

  [Fact]
  public void Parse_UnderscoreKeySplitting_ConvertsToDotDelimited()
  {
    string resx = WrapResx("""
            <data name="Navigation_Dashboard" xml:space="preserve">
              <value>Dashboard</value>
            </data>
            """);

    TranslationSet result = ResxParser.Parse(resx, "en");

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Key.Should().Be("Navigation.Dashboard");
  }

  [Fact]
  public void Parse_DotDelimitedKey_PreservedAsIs()
  {
    string resx = WrapResx("""
            <data name="Navigation.Dashboard" xml:space="preserve">
              <value>Dashboard</value>
            </data>
            """);

    TranslationSet result = ResxParser.Parse(resx, "en");

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Key.Should().Be("Navigation.Dashboard");
  }

  [Fact]
  public void Parse_FormatHoles_ExtractsParameters()
  {
    string resx = WrapResx("""
            <data name="Connection.Connecting" xml:space="preserve">
              <value>Connecting to {0}…</value>
            </data>
            """);

    TranslationSet result = ResxParser.Parse(resx, "en");

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Parameters.Should().ContainSingle()
        .Which.Should().Be("arg0");
    result.Entries[0].Value.Should().Be("Connecting to {arg0}\u2026");
  }

  [Fact]
  public void Parse_MultipleFormatHoles_ExtractsAllParameters()
  {
    string resx = WrapResx("""
            <data name="Transfer.Progress" xml:space="preserve">
              <value>Transferring {0} ({1} of {2})</value>
            </data>
            """);

    TranslationSet result = ResxParser.Parse(resx, "en");

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Parameters.Should().HaveCount(3);
    result.Entries[0].Parameters[0].Should().Be("arg0");
    result.Entries[0].Parameters[1].Should().Be("arg1");
    result.Entries[0].Parameters[2].Should().Be("arg2");
    result.Entries[0].Value.Should().Be("Transferring {arg0} ({arg1} of {arg2})");
  }

  [Fact]
  public void Parse_CommentWithParameterNames_UsesNamedParameters()
  {
    string resx = WrapResx("""
            <data name="Connection.Connecting" xml:space="preserve">
              <value>Connecting to {0}…</value>
              <comment>{0} = host</comment>
            </data>
            """);

    TranslationSet result = ResxParser.Parse(resx, "en");

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Parameters.Should().ContainSingle()
        .Which.Should().Be("host");
    result.Entries[0].Value.Should().Be("Connecting to {host}\u2026");
  }

  [Fact]
  public void Parse_CommentWithMultipleParameterNames_UsesAllNamedParameters()
  {
    string resx = WrapResx("""
            <data name="Transfer.Progress" xml:space="preserve">
              <value>Transferring {0} ({1} of {2})</value>
              <comment>{0} = filename, {1} = current, {2} = total</comment>
            </data>
            """);

    TranslationSet result = ResxParser.Parse(resx, "en");

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Parameters.Should().HaveCount(3);
    result.Entries[0].Parameters[0].Should().Be("filename");
    result.Entries[0].Parameters[1].Should().Be("current");
    result.Entries[0].Parameters[2].Should().Be("total");
    result.Entries[0].Value.Should().Be("Transferring {filename} ({current} of {total})");
  }

  [Fact]
  public void Parse_LanguagePassedThrough_SetsLanguage()
  {
    string resx = WrapResx("""
            <data name="App.Title" xml:space="preserve">
              <value>My App</value>
            </data>
            """);

    TranslationSet result = ResxParser.Parse(resx, "ar-SA");

    result.Language.Should().Be("ar-SA");
  }

  [Fact]
  public void Parse_EmptyResx_ReturnsEmptyEntries()
  {
    string resx = WrapResx("");

    TranslationSet result = ResxParser.Parse(resx, "en");

    result.Language.Should().Be("en");
    result.Entries.Should().BeEmpty();
  }

  [Fact]
  public void Parse_NoValueElement_UsesEmptyString()
  {
    string resx = WrapResx("""
            <data name="App.Placeholder" xml:space="preserve">
            </data>
            """);

    TranslationSet result = ResxParser.Parse(resx, "en");

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Value.Should().BeEmpty();
  }

  [Fact]
  public void Parse_MissingNameAttribute_SkipsEntry()
  {
    string resx = WrapResx("""
            <data xml:space="preserve">
              <value>No name attribute</value>
            </data>
            <data name="App.Title" xml:space="preserve">
              <value>Valid</value>
            </data>
            """);

    TranslationSet result = ResxParser.Parse(resx, "en");

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Key.Should().Be("App.Title");
  }

#if NET10_0_OR_GREATER
  [Fact]
  public void Parse_SpanOverload_ProducesSameResult()
  {
    string resx = WrapResx("""
            <data name="App.Title" xml:space="preserve">
              <value>My App</value>
            </data>
            """);

    TranslationSet stringResult = ResxParser.Parse(resx, "en");
    TranslationSet spanResult = ResxParser.Parse(resx.AsSpan(), "en");

    spanResult.Language.Should().Be(stringResult.Language);
    spanResult.Entries.Should().HaveCount(stringResult.Entries.Count);
    spanResult.Entries[0].Key.Should().Be(stringResult.Entries[0].Key);
    spanResult.Entries[0].Value.Should().Be(stringResult.Entries[0].Value);
  }
#endif
}
