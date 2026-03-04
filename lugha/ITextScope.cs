// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha;

/// <summary>
/// Marker interface for text scope contracts.
/// Members fall into two categories:
/// <list type="bullet">
///   <item>Properties — invariant text (labels, titles, static messages).</item>
///   <item>Methods — parameterised text (formatted messages, interpolated values).</item>
/// </list>
/// All members must return <see cref="string"/>. Non-string returns are a
/// compile error via the Lugha analyser (LGH001).
/// </summary>
#pragma warning disable CA1040 // Marker interface — scanned by Lugha analyser and source generator
public interface ITextScope;
#pragma warning restore CA1040
