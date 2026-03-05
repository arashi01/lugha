// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Lugha.Import.Gettext;

namespace Lugha.Import.Tests;

/// <summary>
/// Tests for <see cref="GettextParser"/>.
/// </summary>
public sealed class GettextParserTests
{
  [Fact]
  public void Parse_SimpleMessages_ReturnsCorrectEntries()
  {
    string po = """
            msgid ""
            msgstr ""
            "Language: en-GB\n"

            msgid "Connection.Discovering"
            msgstr "Discovering…"

            msgid "Connection.Connected"
            msgstr "Connected"
            """;

    TranslationSet result = GettextParser.Parse(po);

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
  public void Parse_WithMsgctxt_UsesScopePrefix()
  {
    string po = """
            msgid ""
            msgstr ""
            "Language: fr\n"

            msgctxt "Navigation"
            msgid "Dashboard"
            msgstr "Tableau de bord"
            """;

    TranslationSet result = GettextParser.Parse(po);

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Key.Should().Be("Navigation.Dashboard");
    result.Entries[0].Value.Should().Be("Tableau de bord");
  }

  [Fact]
  public void Parse_NamedPlaceholders_ExtractsParameters()
  {
    string po = """
            msgid ""
            msgstr ""
            "Language: en\n"

            msgid "Connection.Connecting"
            msgstr "Connecting to {host}…"
            """;

    TranslationSet result = GettextParser.Parse(po);

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Parameters.Should().ContainSingle()
        .Which.Should().Be("host");
  }

  [Fact]
  public void Parse_PositionalPlaceholders_ConvertsToArgN()
  {
    string po = """
            msgid ""
            msgstr ""
            "Language: en\n"

            msgid "Error.Details"
            msgstr "Error %s at line %d"
            """;

    TranslationSet result = GettextParser.Parse(po);

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Parameters.Should().HaveCount(2);
    result.Entries[0].Parameters[0].Should().Be("arg0");
    result.Entries[0].Parameters[1].Should().Be("arg1");
  }

  [Fact]
  public void Parse_PluralForms_MapsToCLDRCategories()
  {
    string po = """
            msgid ""
            msgstr ""
            "Language: en\n"
            "Plural-Forms: nplurals=2; plural=(n != 1);\n"

            msgid "Directory.FilesFound"
            msgid_plural "Directory.FilesFound_plural"
            msgstr[0] "file found"
            msgstr[1] "files found"
            """;

    TranslationSet result = GettextParser.Parse(po);

    result.Entries.Should().HaveCount(1);
    TranslationEntry entry = result.Entries[0];
    entry.PluralForms.Should().NotBeNull();
    entry.PluralForms!.Should().ContainKey("one").WhoseValue.Should().Be("file found");
    entry.PluralForms!.Should().ContainKey("other").WhoseValue.Should().Be("files found");
    entry.Parameters.Should().Contain("count");
  }

  [Fact]
  public void Parse_PluralFormsThreeCategories_MapsCorrectly()
  {
    string po = """
            msgid ""
            msgstr ""
            "Language: ro\n"
            "Plural-Forms: nplurals=3; plural=(n==1?0:(((n%100>19)||((n%100==0)&&(n!=0)))?2:1));\n"

            msgid "Items.Count"
            msgid_plural "Items.Count_plural"
            msgstr[0] "un element"
            msgstr[1] "elemente"
            msgstr[2] "de elemente"
            """;

    TranslationSet result = GettextParser.Parse(po);

    TranslationEntry entry = result.Entries[0];
    entry.PluralForms.Should().NotBeNull();
    entry.PluralForms!.Should().HaveCount(3);
    entry.PluralForms!.Should().ContainKey("one").WhoseValue.Should().Be("un element");
    entry.PluralForms!.Should().ContainKey("two").WhoseValue.Should().Be("elemente");
    entry.PluralForms!.Should().ContainKey("other").WhoseValue.Should().Be("de elemente");
  }

  [Fact]
  public void ParseTemplate_UsesMessageIdAsValue()
  {
    string pot = """
            msgid ""
            msgstr ""
            "Content-Type: text/plain; charset=UTF-8\n"

            msgid "Connection.Discovering"
            msgstr ""

            msgid "Connection.Connecting"
            msgstr ""
            """;

    TranslationSet result = GettextParser.ParseTemplate(pot);

    result.Entries.Should().HaveCount(2);
    result.Entries[0].Key.Should().Be("Connection.Discovering");
    result.Entries[0].Value.Should().Be("Connection.Discovering");
    result.Entries[1].Key.Should().Be("Connection.Connecting");
    result.Entries[1].Value.Should().Be("Connection.Connecting");
  }

  [Fact]
  public void Parse_MultiLineStrings_ConcatenatesCorrectly()
  {
    string po = """
            msgid ""
            msgstr ""
            "Language: en\n"

            msgid "Help.LongText"
            msgstr ""
            "This is a very long "
            "message that spans "
            "multiple lines."
            """;

    TranslationSet result = GettextParser.Parse(po);

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Value.Should().Be("This is a very long message that spans multiple lines.");
  }

  [Fact]
  public void Parse_LanguageHeader_SetsLanguage()
  {
    string po = """
            msgid ""
            msgstr ""
            "Language: ar-SA\n"
            "Content-Type: text/plain; charset=UTF-8\n"

            msgid "App.Title"
            msgstr "التطبيق"
            """;

    TranslationSet result = GettextParser.Parse(po);

    result.Language.Should().Be("ar-SA");
  }

  [Fact]
  public void Parse_SkipsComments()
  {
    string po = """
            # Translator comment
            #. Developer comment
            #: reference.cs:42
            msgid ""
            msgstr ""
            "Language: en\n"

            # Another comment
            msgid "App.Title"
            msgstr "My App"
            """;

    TranslationSet result = GettextParser.Parse(po);

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Key.Should().Be("App.Title");
  }

  [Fact]
  public void Parse_SkipsObsoleteEntries()
  {
    string po = """
            msgid ""
            msgstr ""
            "Language: en\n"

            msgid "App.Active"
            msgstr "Active"

            #~ msgid "App.Obsolete"
            #~ msgstr "Obsolete entry"
            """;

    TranslationSet result = GettextParser.Parse(po);

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Key.Should().Be("App.Active");
  }

  [Fact]
  public void Parse_EscapeSequences_UnescapesCorrectly()
  {
    string po = """
            msgid ""
            msgstr ""
            "Language: en\n"

            msgid "Messages.Welcome"
            msgstr "Hello\tworld\n\"quoted\""
            """;

    TranslationSet result = GettextParser.Parse(po);

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Value.Should().Be("Hello\tworld\n\"quoted\"");
  }

  [Fact]
  public void Parse_MultipleNamedParameters_ExtractsAll()
  {
    string po = """
            msgid ""
            msgstr ""
            "Language: en\n"

            msgid "Transfer.Progress"
            msgstr "Transferring {filename} ({current} of {total})"
            """;

    TranslationSet result = GettextParser.Parse(po);

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Parameters.Should().HaveCount(3);
    result.Entries[0].Parameters.Should().ContainInOrder("filename", "current", "total");
  }

  [Fact]
  public void Parse_EmptyFile_ReturnsEmptySet()
  {
    TranslationSet result = GettextParser.Parse("");

    result.Language.Should().BeEmpty();
    result.Entries.Should().BeEmpty();
  }

  [Fact]
  public void Parse_NoTrailingNewline_EmitsFinalEntry()
  {
    string po = "msgid \"\"\nmsgstr \"\"\n\"Language: en\\n\"\n\nmsgid \"App.Title\"\nmsgstr \"Title\"";

    TranslationSet result = GettextParser.Parse(po);

    result.Entries.Should().HaveCount(1);
    result.Entries[0].Key.Should().Be("App.Title");
    result.Entries[0].Value.Should().Be("Title");
  }

#if NET10_0_OR_GREATER
  [Fact]
  public void Parse_SpanOverload_ProducesSameResult()
  {
    string po = """
            msgid ""
            msgstr ""
            "Language: en\n"

            msgid "App.Title"
            msgstr "My App"
            """;

    TranslationSet stringResult = GettextParser.Parse(po);
    TranslationSet spanResult = GettextParser.Parse(po.AsSpan());

    spanResult.Language.Should().Be(stringResult.Language);
    spanResult.Entries.Should().HaveCount(stringResult.Entries.Count);
    spanResult.Entries[0].Key.Should().Be(stringResult.Entries[0].Key);
    spanResult.Entries[0].Value.Should().Be(stringResult.Entries[0].Value);
  }
#endif
}
