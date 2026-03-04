// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System;

namespace Lugha.Tests;

public sealed class BidiTests
{
  // ── Constants ──────────────────────────────────────────────────────

  [Fact]
  public void Lri_is_U2066() =>
    Bidi.Lri.Should().Be('\u2066');

  [Fact]
  public void Rli_is_U2067() =>
    Bidi.Rli.Should().Be('\u2067');

  [Fact]
  public void Fsi_is_U2068() =>
    Bidi.Fsi.Should().Be('\u2068');

  [Fact]
  public void Pdi_is_U2069() =>
    Bidi.Pdi.Should().Be('\u2069');

  [Fact]
  public void Lrm_is_U200E() =>
    Bidi.Lrm.Should().Be('\u200E');

  [Fact]
  public void Rlm_is_U200F() =>
    Bidi.Rlm.Should().Be('\u200F');

  // ── String methods ─────────────────────────────────────────────────

  [Fact]
  public void IsolateLtr_wraps_value_with_Lri_and_Pdi()
  {
    string result = Bidi.IsolateLtr("hello");
    result.Should().Be("\u2066hello\u2069");
  }

  [Fact]
  public void IsolateRtl_wraps_value_with_Rli_and_Pdi()
  {
    string result = Bidi.IsolateRtl("hello");
    result.Should().Be("\u2067hello\u2069");
  }

  [Fact]
  public void Isolate_wraps_value_with_Fsi_and_Pdi()
  {
    string result = Bidi.Isolate("hello");
    result.Should().Be("\u2068hello\u2069");
  }

  [Fact]
  public void IsolateLtr_with_empty_string_produces_isolate_pair_only()
  {
    string result = Bidi.IsolateLtr("");
    result.Should().Be("\u2066\u2069");
    result.Length.Should().Be(2);
  }

  [Fact]
  public void IsolateRtl_with_empty_string_produces_isolate_pair_only()
  {
    string result = Bidi.IsolateRtl("");
    result.Should().Be("\u2067\u2069");
    result.Length.Should().Be(2);
  }

  [Fact]
  public void Isolate_with_empty_string_produces_isolate_pair_only()
  {
    string result = Bidi.Isolate("");
    result.Should().Be("\u2068\u2069");
    result.Length.Should().Be(2);
  }

  /// <summary>
  /// Real-world scenario: embedding an LTR product name inside RTL Arabic text.
  /// The isolate characters prevent the Bidi algorithm from visually reordering
  /// the Latin characters into the surrounding RTL run.
  /// </summary>
  [Fact]
  public void IsolateLtr_preserves_Rtl_text_embedding()
  {
    // Arabic text: "the price of" ... "is 100"
    string product = "Azure DevOps";
    string isolated = Bidi.IsolateLtr(product);

    isolated.Should().StartWith("\u2066");
    isolated.Should().EndWith("\u2069");
    isolated.Should().Contain("Azure DevOps");
  }

  /// <summary>
  /// Embedding RTL text (Arabic) inside an LTR host string.
  /// </summary>
  [Fact]
  public void IsolateRtl_wraps_Arabic_text()
  {
    // Arabic: "language"
    string arabic = "\u0644\u063A\u0629";
    string isolated = Bidi.IsolateRtl(arabic);

    isolated[0].Should().Be(Bidi.Rli);
    isolated[^1].Should().Be(Bidi.Pdi);
    isolated[1..^1].Should().Be(arabic);
  }

  /// <summary>
  /// Surrogate pairs (emoji, supplementary plane characters) must pass through
  /// without corruption. Each surrogate pair is two <see cref="char"/> units but
  /// one Unicode scalar.
  /// </summary>
  [Fact]
  public void IsolateLtr_preserves_surrogate_pairs()
  {
    // U+1F600 (grinning face) — two UTF-16 code units
    string emoji = "\U0001F600";
    emoji.Length.Should().Be(2, "surrogate pair is two chars");

    string result = Bidi.IsolateLtr(emoji);
    result.Length.Should().Be(4); // LRI + 2 surrogates + PDI
    result[0].Should().Be(Bidi.Lri);
    result[^1].Should().Be(Bidi.Pdi);
    result[1..^1].Should().Be(emoji);
  }

  /// <summary>
  /// Combining character sequences (base + combining diacritic) must not be
  /// split. The isolate wraps the entire sequence.
  /// </summary>
  [Fact]
  public void Isolate_preserves_combining_characters()
  {
    // "e" + combining acute accent (U+0301) = "e\u0301" visually renders as e with accent
    string combining = "e\u0301";
    string result = Bidi.Isolate(combining);

    result.Length.Should().Be(4); // FSI + 'e' + U+0301 + PDI
    result[0].Should().Be(Bidi.Fsi);
    result[^1].Should().Be(Bidi.Pdi);
    result[1..^1].Should().Be(combining);
  }

  /// <summary>
  /// ZWJ sequences (used in emoji and complex scripts) must pass through intact.
  /// </summary>
  [Fact]
  public void IsolateLtr_preserves_Zwj_sequences()
  {
    // Family emoji: person + ZWJ + person + ZWJ + child
    string zwj = "\U0001F468\u200D\U0001F469\u200D\U0001F467";
    string result = Bidi.IsolateLtr(zwj);

    result[0].Should().Be(Bidi.Lri);
    result[^1].Should().Be(Bidi.Pdi);
    result[1..^1].Should().Be(zwj);
  }

  // ── Span methods — success path ────────────────────────────────────

  [Fact]
  public void TryIsolateLtr_writes_to_exact_size_buffer()
  {
    ReadOnlySpan<char> value = "test".AsSpan();
    Span<char> buffer = stackalloc char[6]; // 4 + 2

    Bidi.TryIsolateLtr(value, buffer, out int written).Should().BeTrue();
    written.Should().Be(6);
    buffer[0].Should().Be(Bidi.Lri);
    buffer[5].Should().Be(Bidi.Pdi);
    buffer[1..5].SequenceEqual("test".AsSpan()).Should().BeTrue();
  }

  [Fact]
  public void TryIsolateRtl_writes_to_exact_size_buffer()
  {
    ReadOnlySpan<char> value = "test".AsSpan();
    Span<char> buffer = stackalloc char[6];

    Bidi.TryIsolateRtl(value, buffer, out int written).Should().BeTrue();
    written.Should().Be(6);
    buffer[0].Should().Be(Bidi.Rli);
    buffer[5].Should().Be(Bidi.Pdi);
    buffer[1..5].SequenceEqual("test".AsSpan()).Should().BeTrue();
  }

  [Fact]
  public void TryIsolate_writes_to_exact_size_buffer()
  {
    ReadOnlySpan<char> value = "test".AsSpan();
    Span<char> buffer = stackalloc char[6];

    Bidi.TryIsolate(value, buffer, out int written).Should().BeTrue();
    written.Should().Be(6);
    buffer[0].Should().Be(Bidi.Fsi);
    buffer[5].Should().Be(Bidi.Pdi);
    buffer[1..5].SequenceEqual("test".AsSpan()).Should().BeTrue();
  }

  [Fact]
  public void TryIsolateLtr_empty_value_needs_two_char_buffer()
  {
    ReadOnlySpan<char> value = [];
    Span<char> buffer = stackalloc char[2];

    Bidi.TryIsolateLtr(value, buffer, out int written).Should().BeTrue();
    written.Should().Be(2);
    buffer[0].Should().Be(Bidi.Lri);
    buffer[1].Should().Be(Bidi.Pdi);
  }

  // ── Span methods — insufficient buffer ─────────────────────────────

  [Fact]
  public void TryIsolateLtr_returns_false_when_buffer_too_small()
  {
    ReadOnlySpan<char> value = "test".AsSpan();
    Span<char> buffer = stackalloc char[5]; // needs 6

    Bidi.TryIsolateLtr(value, buffer, out int written).Should().BeFalse();
    written.Should().Be(0);
  }

  [Fact]
  public void TryIsolateRtl_returns_false_when_buffer_too_small()
  {
    ReadOnlySpan<char> value = "test".AsSpan();
    Span<char> buffer = stackalloc char[5];

    Bidi.TryIsolateRtl(value, buffer, out int written).Should().BeFalse();
    written.Should().Be(0);
  }

  [Fact]
  public void TryIsolate_returns_false_when_buffer_too_small()
  {
    ReadOnlySpan<char> value = "test".AsSpan();
    Span<char> buffer = stackalloc char[5];

    Bidi.TryIsolate(value, buffer, out int written).Should().BeFalse();
    written.Should().Be(0);
  }

  [Fact]
  public void TryIsolateLtr_returns_false_for_empty_destination()
  {
    ReadOnlySpan<char> value = "x".AsSpan();
    Span<char> buffer = [];

    Bidi.TryIsolateLtr(value, buffer, out int written).Should().BeFalse();
    written.Should().Be(0);
  }

  /// <summary>
  /// A buffer that is exactly one char short demonstrates the boundary
  /// condition: <c>value.Length + 1</c> is insufficient.
  /// </summary>
  [Fact]
  public void TryIsolateRtl_one_short_returns_false()
  {
    ReadOnlySpan<char> value = "ab".AsSpan();
    Span<char> buffer = stackalloc char[3]; // needs 4

    Bidi.TryIsolateRtl(value, buffer, out int written).Should().BeFalse();
    written.Should().Be(0);
  }

  // ── Span methods — oversized buffer ────────────────────────────────

  [Fact]
  public void TryIsolateLtr_in_oversized_buffer_writes_only_required_chars()
  {
    ReadOnlySpan<char> value = "hi".AsSpan();
    Span<char> buffer = stackalloc char[100];
    buffer.Clear();

    Bidi.TryIsolateLtr(value, buffer, out int written).Should().BeTrue();
    written.Should().Be(4); // LRI + 'h' + 'i' + PDI
    buffer[4].Should().Be('\0', "chars beyond written count are untouched");
  }

  // ── Span methods — Unicode edge cases ──────────────────────────────

  [Fact]
  public void TryIsolate_preserves_surrogate_pairs_in_span()
  {
    string emoji = "\U0001F600";
    ReadOnlySpan<char> value = emoji.AsSpan();
    Span<char> buffer = stackalloc char[4]; // 2 surrogates + 2

    Bidi.TryIsolate(value, buffer, out int written).Should().BeTrue();
    written.Should().Be(4);
    buffer[0].Should().Be(Bidi.Fsi);
    buffer[^1].Should().Be(Bidi.Pdi);
    new string(buffer[1..^1]).Should().Be(emoji);
  }

  // ── String / span output equivalence ──────────────────────────────

  /// <summary>
  /// The string and span paths must produce identical character sequences
  /// for the same input.
  /// </summary>
  [Theory]
  [InlineData("")]
  [InlineData("hello")]
  [InlineData("\u0644\u063A\u0629")] // Arabic
  public void IsolateLtr_string_and_span_produce_identical_output(string input)
  {
    string expected = Bidi.IsolateLtr(input);
    Span<char> buffer = new char[input.Length + 2];
    Bidi.TryIsolateLtr(input.AsSpan(), buffer, out int written).Should().BeTrue();

    new string(buffer[..written]).Should().Be(expected);
  }

  [Theory]
  [InlineData("")]
  [InlineData("hello")]
  [InlineData("\u0644\u063A\u0629")]
  public void IsolateRtl_string_and_span_produce_identical_output(string input)
  {
    string expected = Bidi.IsolateRtl(input);
    Span<char> buffer = new char[input.Length + 2];
    Bidi.TryIsolateRtl(input.AsSpan(), buffer, out int written).Should().BeTrue();

    new string(buffer[..written]).Should().Be(expected);
  }

  [Theory]
  [InlineData("")]
  [InlineData("hello")]
  [InlineData("\u0644\u063A\u0629")]
  public void Isolate_string_and_span_produce_identical_output(string input)
  {
    string expected = Bidi.Isolate(input);
    Span<char> buffer = new char[input.Length + 2];
    Bidi.TryIsolate(input.AsSpan(), buffer, out int written).Should().BeTrue();

    new string(buffer[..written]).Should().Be(expected);
  }
}
