// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.


namespace Lugha.Import;

/// <summary>
/// A single translated text entry extracted from an external source.
/// Immutable. Used as the intermediate representation between parsers
/// and the code emitter.
/// </summary>
/// <param name="Key">
///   Dot-delimited key path. The first segment becomes the text scope name,
///   the remainder becomes the member name.
///   E.g. <c>"Connection.Discovering"</c> -> scope <c>IConnectionText</c>,
///   member <c>Discovering</c>.
/// </param>
/// <param name="Value">The translated string value.</param>
/// <param name="Parameters">
///   Ordered parameter names extracted from format placeholders.
///   Empty for invariant strings (properties). Non-empty for
///   parameterised strings (methods).
/// </param>
/// <param name="PluralForms">
///   If this entry has plural variants, the keyed forms (one, other, few, etc.).
///   Null for non-plural entries.
/// </param>
public sealed record TranslationEntry(
    string Key,
    string Value,
    IReadOnlyList<string> Parameters,
    IReadOnlyDictionary<string, string>? PluralForms);
