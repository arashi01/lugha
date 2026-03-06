// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Import.Tests;

/// <summary>
/// Tests for <see cref="CodeEmitter"/>.
/// </summary>
public sealed class CodeEmitterTests
{
  private const string TestNamespace = "TestApp.Localisation";

  [Fact]
  public void EmitContracts_SimpleProperties_GeneratesInterface()
  {
    var set = new TranslationSet("en", [
        new TranslationEntry("Connection.Discovering", "Discovering…", [], null),
            new TranslationEntry("Connection.Connected", "Connected", [], null),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitContracts(set, TestNamespace);

    files.Should().HaveCount(1);
    files[0].FileName.Should().Be("IConnectionText.g.cs");
    files[0].Content.Should().Contain("public interface IConnectionText : ITextScope");
    files[0].Content.Should().Contain("string Discovering { get; }");
    files[0].Content.Should().Contain("string Connected { get; }");
    files[0].Content.Should().Contain($"namespace {TestNamespace};");
  }

  [Fact]
  public void EmitContracts_ParameterisedEntries_GeneratesMethods()
  {
    var set = new TranslationSet("en", [
        new TranslationEntry("Connection.Connecting", "Connecting to {host}…", ["host"], null),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitContracts(set, TestNamespace);

    files.Should().HaveCount(1);
    files[0].Content.Should().Contain("string Connecting(string host);");
  }

  [Fact]
  public void EmitContracts_CountParameter_GeneratesIntType()
  {
    var set = new TranslationSet("en", [
        new TranslationEntry("Directory.FilesFound", "files found", ["count"], new Dictionary<string, string>
            {
                ["one"] = "file found",
                ["other"] = "files found",
            }),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitContracts(set, TestNamespace);

    files.Should().HaveCount(1);
    files[0].Content.Should().Contain("string FilesFound(int count);");
  }

  [Fact]
  public void EmitContracts_MultipleScopes_GeneratesMultipleFiles()
  {
    var set = new TranslationSet("en", [
        new TranslationEntry("Connection.Discovering", "Discovering…", [], null),
            new TranslationEntry("Navigation.Dashboard", "Dashboard", [], null),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitContracts(set, TestNamespace);

    files.Should().HaveCount(2);
    files.Should().Contain(f => f.FileName == "IConnectionText.g.cs");
    files.Should().Contain(f => f.FileName == "INavigationText.g.cs");
  }

  [Fact]
  public void EmitImplementations_SimpleProperties_GeneratesSealedClass()
  {
    var set = new TranslationSet("es-ES", [
        new TranslationEntry("Connection.Discovering", "Descubriendo…", [], null),
            new TranslationEntry("Connection.Connected", "Conectado", [], null),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitImplementations(
        set, TestNamespace, TestNamespace);

    files.Should().HaveCount(1);
    files[0].FileName.Should().Be("EsEsConnectionText.g.cs");
    files[0].Content.Should().Contain("public sealed class EsEsConnectionText : IConnectionText");
    files[0].Content.Should().Contain("public string Discovering => \"Descubriendo\u2026\";");
    files[0].Content.Should().Contain("public string Connected => \"Conectado\";");
  }

  [Fact]
  public void EmitImplementations_ParameterisedEntries_GeneratesMethodsWithInterpolation()
  {
    var set = new TranslationSet("es-ES", [
        new TranslationEntry("Connection.Connecting", "Conectando a {host}…", ["host"], null),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitImplementations(
        set, TestNamespace, TestNamespace);

    files.Should().HaveCount(1);
    files[0].Content.Should().Contain("public string Connecting(string host) => $\"Conectando a {host}\u2026\";");
  }

  [Fact]
  public void EmitImplementations_PluralEntry_GeneratesPluralSelectCall()
  {
    var set = new TranslationSet("es-ES", [
        new TranslationEntry("Directory.FilesFound", "files found", ["count"], new Dictionary<string, string>
            {
                ["one"] = "{count} archivo encontrado",
                ["other"] = "{count} archivos encontrados",
            }),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitImplementations(
        set, TestNamespace, TestNamespace);

    files.Should().HaveCount(1);
    string content = files[0].Content;
    content.Should().Contain("Plural.Select<LatinEuropeanCardinal>");
    content.Should().Contain("new PluralForms");
    content.Should().Contain("One = $\"{count.ToString(\"N0\", Culture)} archivo encontrado\"");
    content.Should().Contain("Other = $\"{count.ToString(\"N0\", Culture)} archivos encontrados\"");
    content.Should().Contain("CultureInfo.GetCultureInfo(\"es-ES\")");
    content.Should().Contain("using Lugha.Rules.Cardinals;");
  }

  [Fact]
  public void EmitImplementations_DifferentNamespaces_AddsUsingDirective()
  {
    var set = new TranslationSet("en", [
        new TranslationEntry("App.Title", "My App", [], null),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitImplementations(
        set, "Contracts.Namespace", "Impl.Namespace");

    files.Should().HaveCount(1);
    files[0].Content.Should().Contain("using Contracts.Namespace;");
    files[0].Content.Should().Contain("namespace Impl.Namespace;");
  }

  [Fact]
  public void EmitImplementations_SameNamespaces_NoRedundantUsing()
  {
    var set = new TranslationSet("en", [
        new TranslationEntry("App.Title", "My App", [], null),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitImplementations(
        set, TestNamespace, TestNamespace);

    files.Should().HaveCount(1);
    files[0].Content.Should().NotContain($"using {TestNamespace};");
  }

  [Fact]
  public void ToInterfaceName_DerivesCorrectly()
  {
    CodeEmitter.ToInterfaceName("Connection").Should().Be("IConnectionText");
    CodeEmitter.ToInterfaceName("Navigation").Should().Be("INavigationText");
    CodeEmitter.ToInterfaceName("App").Should().Be("IAppText");
  }

  [Fact]
  public void ToClassPrefix_DerivesCorrectly()
  {
    CodeEmitter.ToClassPrefix("es-ES").Should().Be("EsEs");
    CodeEmitter.ToClassPrefix("en").Should().Be("En");
    CodeEmitter.ToClassPrefix("pt-BR").Should().Be("PtBr");
    CodeEmitter.ToClassPrefix("zh-Hant-TW").Should().Be("ZhHantTw");
  }

  [Fact]
  public void EmitContracts_GeneratesAutoGeneratedComment()
  {
    var set = new TranslationSet("en", [
        new TranslationEntry("App.Title", "Title", [], null),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitContracts(set, TestNamespace);

    files[0].Content.Should().StartWith("// <auto-generated/>");
  }

  [Fact]
  public void EmitImplementations_GeneratesAutoGeneratedComment()
  {
    var set = new TranslationSet("en", [
        new TranslationEntry("App.Title", "Title", [], null),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitImplementations(
        set, TestNamespace, TestNamespace);

    files[0].Content.Should().StartWith("// <auto-generated/>");
  }

  [Fact]
  public void EmitImplementations_EscapesStringLiterals()
  {
    var set = new TranslationSet("en", [
        new TranslationEntry("App.Quote", "She said \"hello\"", [], null),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitImplementations(
        set, TestNamespace, TestNamespace);

    files[0].Content.Should().Contain("She said \\\"hello\\\"");
  }

  [Fact]
  public void EmitImplementations_MissingTranslation_OmitsMember()
  {
    // When a locale is missing a translation that the contract defines,
    // the generated class simply won't have the member - causing a
    // downstream compile error.
    var set = new TranslationSet("es-ES", [
        new TranslationEntry("Connection.Discovering", "Descubriendo…", [], null),
            // Connection.Connected is deliberately missing
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitImplementations(
        set, TestNamespace, TestNamespace);

    files.Should().HaveCount(1);
    files[0].Content.Should().Contain("Discovering");
    files[0].Content.Should().NotContain("Connected");
  }

  [Fact]
  public void EmitImplementations_UnknownLanguage_EmitsWithoutPluralRule()
  {
    // For an unrecognised language tag, plural entries should still emit
    // but without Plural.Select (fallback to simple property)
    var set = new TranslationSet("xx-YY", [
        new TranslationEntry("App.Title", "Title", [], null),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitImplementations(
        set, TestNamespace, TestNamespace);

    files.Should().HaveCount(1);
    files[0].Content.Should().Contain("public string Title => \"Title\";");
  }

  [Fact]
  public void EmitImplementations_PluralEntry_FormsWithoutCount_EmitsPlainStrings()
  {
    var set = new TranslationSet("ar-SA", [
        new TranslationEntry("Status.OnlineUsers", "users online", ["count"], new Dictionary<string, string>
            {
                ["zero"] = "لا مستخدمين متصلين",
                ["one"] = "مستخدم متصل",
                ["two"] = "مستخدمان متصلان",
                ["few"] = "{count} مستخدمين متصلين",
                ["many"] = "{count} مستخدمًا متصلًا",
                ["other"] = "{count} مستخدم متصل",
            }),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitImplementations(
        set, TestNamespace, TestNamespace);

    files.Should().HaveCount(1);
    string content = files[0].Content;

    // Forms without {count} are plain string literals
    content.Should().Contain("Zero = \"لا مستخدمين متصلين\"");
    content.Should().Contain("One = \"مستخدم متصل\"");
    content.Should().Contain("Two = \"مستخدمان متصلان\"");

    // Forms with {count} are interpolated strings
    content.Should().Contain("Few = $\"{count.ToString(\"N0\", Culture)} مستخدمين متصلين\"");
    content.Should().Contain("Many = $\"{count.ToString(\"N0\", Culture)} مستخدمًا متصلًا\"");
    content.Should().Contain("Other = $\"{count.ToString(\"N0\", Culture)} مستخدم متصل\"");
  }

  [Fact]
  public void EmitImplementations_PluralEntry_WithMixedParams_EmitsCorrectInterpolation()
  {
    var set = new TranslationSet("en", [
        new TranslationEntry("Status.PostCount", "posts", ["count", "author"],
            new Dictionary<string, string>
            {
                ["one"] = "{count} post by {author}",
                ["other"] = "{count} posts by {author}",
            }),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitImplementations(
        set, TestNamespace, TestNamespace);

    string content = files[0].Content;
    content.Should().Contain("One = $\"{count.ToString(\"N0\", Culture)} post by {author}\"");
    content.Should().Contain("Other = $\"{count.ToString(\"N0\", Culture)} posts by {author}\"");
    content.Should().Contain("string PostCount(int count, string author)");
  }

  [Fact]
  public void EmitImplementations_PluralEntry_SingleForm_EmitsCorrectly()
  {
    var set = new TranslationSet("zh", [
        new TranslationEntry("Status.FileCount", "files", ["count"],
            new Dictionary<string, string>
            {
                ["other"] = "{count} 个文件",
            }),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitImplementations(
        set, TestNamespace, TestNamespace);

    string content = files[0].Content;
    content.Should().Contain("Plural.Select<OtherOnlyCardinal>");
    content.Should().Contain("Other = $\"{count.ToString(\"N0\", Culture)} 个文件\"");
  }

  [Fact]
  public void EmitContracts_MixedPropertiesAndMethods()
  {
    var set = new TranslationSet("en", [
        new TranslationEntry("Connection.Discovering", "Discovering…", [], null),
            new TranslationEntry("Connection.Connecting", "Connecting to {host}…", ["host"], null),
            new TranslationEntry("Connection.Unavailable", "Unavailable: {reason}", ["reason"], null),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitContracts(set, TestNamespace);

    files.Should().HaveCount(1);
    string content = files[0].Content;
    content.Should().Contain("string Discovering { get; }");
    content.Should().Contain("string Connecting(string host);");
    content.Should().Contain("string Unavailable(string reason);");
  }

  [Fact]
  public void EmitContracts_UsesLughaNamespace()
  {
    var set = new TranslationSet("en", [
        new TranslationEntry("App.Title", "Title", [], null),
        ]);

    IReadOnlyList<EmittedFile> files = CodeEmitter.EmitContracts(set, TestNamespace);

    files[0].Content.Should().Contain("using Lugha;");
  }
}
