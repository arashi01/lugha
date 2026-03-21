// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha;

/// <summary>
/// Two locales share the same BCP 47 language tag.
/// Returned by <see cref="LocaleRegistry{TLocale}.Create"/> factory methods.
/// </summary>
/// <param name="Tag">The duplicated language tag.</param>
public readonly record struct DuplicateLanguageTag(string Tag);
