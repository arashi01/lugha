// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System;

namespace Lugha;

/// <summary>
/// Unicode bidirectional isolation utilities conforming to
/// <a href="https://www.unicode.org/reports/tr9/">UAX #9</a>.
/// Wraps interpolated values with directional isolate characters to prevent
/// visual reordering when embedding LTR content in RTL host text (or vice versa).
/// </summary>
/// <remarks>
/// <para>
/// When an RTL locale interpolates an LTR value (a number, a product name,
/// a URL), the Unicode Bidirectional Algorithm may reorder characters if
/// the embedding is not isolated. These utilities insert the correct
/// isolate / pop-directional-isolate pair around the value.
/// </para>
/// <para>
/// String methods allocate a new string. Span methods (<c>TryIsolateLtr</c>,
/// <c>TryIsolateRtl</c>, <c>TryIsolate</c>) are zero-allocation: they write
/// into a caller-provided <see cref="Span{T}"/> and return <see langword="false"/>
/// when the destination is too small (exactly <c>value.Length + 2</c> chars are
/// required).
/// </para>
/// </remarks>
public static class Bidi
{
  /// <summary>Left-to-Right Isolate (U+2066). Opens an LTR isolation scope.</summary>
  public const char Lri = '\u2066';

  /// <summary>Right-to-Left Isolate (U+2067). Opens an RTL isolation scope.</summary>
  public const char Rli = '\u2067';

  /// <summary>First Strong Isolate (U+2068). Opens an isolation scope whose
  /// direction is determined by the first strong directional character.</summary>
  public const char Fsi = '\u2068';

  /// <summary>Pop Directional Isolate (U+2069). Closes the most recently
  /// opened isolation scope.</summary>
  public const char Pdi = '\u2069';

  /// <summary>Left-to-Right Mark (U+200E). Zero-width LTR strong character.</summary>
  public const char Lrm = '\u200E';

  /// <summary>Right-to-Left Mark (U+200F). Zero-width RTL strong character.</summary>
  public const char Rlm = '\u200F';

  /// <summary>
  /// Wraps <paramref name="value"/> in a Left-to-Right Isolate pair
  /// (<see cref="Lri"/> ... <see cref="Pdi"/>).
  /// </summary>
  /// <param name="value">The text to isolate.</param>
  /// <returns>The isolated string.</returns>
  public static string IsolateLtr(string value) =>
    $"{Lri}{value}{Pdi}";

  /// <summary>
  /// Wraps <paramref name="value"/> in a Right-to-Left Isolate pair
  /// (<see cref="Rli"/> ... <see cref="Pdi"/>).
  /// </summary>
  /// <param name="value">The text to isolate.</param>
  /// <returns>The isolated string.</returns>
  public static string IsolateRtl(string value) =>
    $"{Rli}{value}{Pdi}";

  /// <summary>
  /// Wraps <paramref name="value"/> in a First Strong Isolate pair
  /// (<see cref="Fsi"/> ... <see cref="Pdi"/>). The direction of
  /// the isolated run is determined by the first character with a
  /// strong directional property.
  /// </summary>
  /// <param name="value">The text to isolate.</param>
  /// <returns>The isolated string.</returns>
  public static string Isolate(string value) =>
    $"{Fsi}{value}{Pdi}";

  /// <summary>
  /// Writes <paramref name="value"/> wrapped in a Left-to-Right Isolate pair
  /// into <paramref name="destination"/> without allocating.
  /// </summary>
  /// <param name="value">The text to isolate.</param>
  /// <param name="destination">Target buffer. Must be at least
  /// <c>value.Length + 2</c> characters.</param>
  /// <param name="written">Number of characters written on success; zero on failure.</param>
  /// <returns><see langword="true"/> if the buffer was large enough;
  /// <see langword="false"/> otherwise.</returns>
  public static bool TryIsolateLtr(
    ReadOnlySpan<char> value, Span<char> destination, out int written)
  {
    if (destination.Length < value.Length + 2)
    {
      written = 0;
      return false;
    }

    destination[0] = Lri;
    value.CopyTo(destination[1..]);
    destination[value.Length + 1] = Pdi;
    written = value.Length + 2;
    return true;
  }

  /// <summary>
  /// Writes <paramref name="value"/> wrapped in a Right-to-Left Isolate pair
  /// into <paramref name="destination"/> without allocating.
  /// </summary>
  /// <param name="value">The text to isolate.</param>
  /// <param name="destination">Target buffer. Must be at least
  /// <c>value.Length + 2</c> characters.</param>
  /// <param name="written">Number of characters written on success; zero on failure.</param>
  /// <returns><see langword="true"/> if the buffer was large enough;
  /// <see langword="false"/> otherwise.</returns>
  public static bool TryIsolateRtl(
    ReadOnlySpan<char> value, Span<char> destination, out int written)
  {
    if (destination.Length < value.Length + 2)
    {
      written = 0;
      return false;
    }

    destination[0] = Rli;
    value.CopyTo(destination[1..]);
    destination[value.Length + 1] = Pdi;
    written = value.Length + 2;
    return true;
  }

  /// <summary>
  /// Writes <paramref name="value"/> wrapped in a First Strong Isolate pair
  /// into <paramref name="destination"/> without allocating.
  /// </summary>
  /// <param name="value">The text to isolate.</param>
  /// <param name="destination">Target buffer. Must be at least
  /// <c>value.Length + 2</c> characters.</param>
  /// <param name="written">Number of characters written on success; zero on failure.</param>
  /// <returns><see langword="true"/> if the buffer was large enough;
  /// <see langword="false"/> otherwise.</returns>
  public static bool TryIsolate(
    ReadOnlySpan<char> value, Span<char> destination, out int written)
  {
    if (destination.Length < value.Length + 2)
    {
      written = 0;
      return false;
    }

    destination[0] = Fsi;
    value.CopyTo(destination[1..]);
    destination[value.Length + 1] = Pdi;
    written = value.Length + 2;
    return true;
  }
}
