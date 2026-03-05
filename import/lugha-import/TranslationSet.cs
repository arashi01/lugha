// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.


namespace Lugha.Import;

/// <summary>
/// A complete set of translations for one locale, extracted from
/// an external source. Produced by format-specific parsers,
/// consumed by the code emitter.
/// </summary>
/// <param name="Language">BCP 47 language tag (e.g. <c>"en-GB"</c>, <c>"ar-SA"</c>).</param>
/// <param name="Entries">All translation entries for this locale.</param>
public sealed record TranslationSet(
    string Language,
    IReadOnlyList<TranslationEntry> Entries);
