// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Text;
using System.Text.RegularExpressions;

namespace Lugha.Import.Gettext;

/// <summary>
/// Parses GNU gettext <c>.po</c> / <c>.pot</c> files into
/// <see cref="TranslationSet"/> instances.
/// </summary>
/// <remarks>
/// <para>Key derivation: <c>msgctxt</c> is used as the scope prefix if present.
/// Otherwise, the <c>msgid</c> is used directly as a dot-delimited key.</para>
/// <para>Plural forms: <c>msgid_plural</c> / <c>msgstr[N]</c> entries are
/// mapped to <see cref="TranslationEntry.PluralForms"/> with CLDR category
/// names derived from the file's <c>Plural-Forms</c> header.</para>
/// <para>Parameters: <c>{name}</c> placeholders in <c>msgstr</c> are extracted
/// as named parameters. Positional <c>%s</c> / <c>%d</c> style placeholders
/// are converted to <c>{arg0}</c>, <c>{arg1}</c>, etc.</para>
/// </remarks>
public static class GettextParser
{
  private static readonly Regex NamedPlaceholderPattern = new(@"\{(\w+)\}", RegexOptions.Compiled);
  private static readonly Regex PositionalPlaceholderPattern = new(@"%[sd]", RegexOptions.Compiled);

  /// <summary>
  /// Default CLDR category names mapped by plural form index.
  /// Used when the <c>Plural-Forms</c> header specifies <c>nplurals</c>
  /// but does not provide explicit category names.
  /// </summary>
  private static readonly string[][] DefaultPluralCategories =
  [
      ["other"],
        ["one", "other"],
        ["one", "two", "other"],
        ["one", "two", "few", "other"],
        ["one", "two", "few", "many", "other"],
        ["zero", "one", "two", "few", "many", "other"],
    ];

  /// <summary>
  /// Parses a single <c>.po</c> file.
  /// </summary>
  /// <param name="content">Complete text content of the <c>.po</c> file.</param>
  /// <returns>Parsed translation set.</returns>
  public static TranslationSet Parse(string content)
  {
    return ParseCore(content, isTemplate: false);
  }

  /// <summary>
  /// Parses a <c>.pot</c> template file (source-language reference).
  /// Uses <c>msgid</c> values as translation values since template
  /// <c>msgstr</c> entries are typically empty.
  /// </summary>
  /// <param name="content">Complete text content of the <c>.pot</c> file.</param>
  /// <returns>Parsed translation set.</returns>
  public static TranslationSet ParseTemplate(string content)
  {
    return ParseCore(content, isTemplate: true);
  }

#if NET10_0_OR_GREATER
    /// <summary>
    /// Parses a single <c>.po</c> file from a span.
    /// </summary>
    /// <param name="content">Complete text content of the <c>.po</c> file.</param>
    /// <returns>Parsed translation set.</returns>
    public static TranslationSet Parse(ReadOnlySpan<char> content)
    {
        return ParseCore(content.ToString(), isTemplate: false);
    }

    /// <summary>
    /// Parses a <c>.pot</c> template file from a span.
    /// </summary>
    /// <param name="content">Complete text content of the <c>.pot</c> file.</param>
    /// <returns>Parsed translation set.</returns>
    public static TranslationSet ParseTemplate(ReadOnlySpan<char> content)
    {
        return ParseCore(content.ToString(), isTemplate: true);
    }
#endif

  private static TranslationSet ParseCore(string content, bool isTemplate)
  {
    List<TranslationEntry> entries = [];
    string language = "";
    int nplurals = 2;

    string? msgctxt = null;
    string? msgid = null;
    string? msgidPlural = null;
    string? msgstr = null;
    Dictionary<int, string> pluralForms = [];
    string? currentField = null;

    string[] lines = content.Split('\n');

    for (int i = 0; i < lines.Length; i++)
    {
      string line = lines[i].TrimEnd('\r');

      // Skip comments and obsolete entries
      if (line.Length > 0 && line[0] == '#')
      {
        if (line.Length >= 2 && line[1] == '~')
        {
          continue;
        }

        if (line.Length < 2 || (line[1] != ':' && line[1] != '.'))
        {
          continue;
        }

        // Reference and extracted comments - skip
        continue;
      }

      // Empty line - emit current entry
      if (string.IsNullOrWhiteSpace(line))
      {
        if (msgid is not null)
        {
          EmitEntry(entries, ref language, ref nplurals, msgctxt, msgid, msgidPlural, msgstr, pluralForms, isTemplate);
        }

        msgctxt = null;
        msgid = null;
        msgidPlural = null;
        msgstr = null;
        pluralForms.Clear();
        currentField = null;
        continue;
      }

      // Continuation line (starts with ")
      if (line.Length > 0 && line[0] == '"')
      {
        string continued = ExtractQuotedString(line);

        switch (currentField)
        {
          case "msgctxt":
            msgctxt += continued;
            break;
          case "msgid":
            msgid += continued;
            break;
          case "msgid_plural":
            msgidPlural += continued;
            break;
          case "msgstr":
            msgstr += continued;
            break;
          default:
            if (currentField is not null && currentField.StartsWith("msgstr[", StringComparison.Ordinal))
            {
              int idx = ParsePluralIndex(currentField);
              if (idx >= 0 && pluralForms.ContainsKey(idx))
              {
                pluralForms[idx] += continued;
              }
            }
            break;
        }

        continue;
      }

      // Field lines
      if (line.StartsWith("msgctxt ", StringComparison.Ordinal))
      {
        msgctxt = ExtractQuotedString(line[8..]);
        currentField = "msgctxt";
      }
      else if (line.StartsWith("msgid_plural ", StringComparison.Ordinal))
      {
        msgidPlural = ExtractQuotedString(line[13..]);
        currentField = "msgid_plural";
      }
      else if (line.StartsWith("msgid ", StringComparison.Ordinal))
      {
        msgid = ExtractQuotedString(line[6..]);
        currentField = "msgid";
      }
      else if (line.StartsWith("msgstr[", StringComparison.Ordinal))
      {
#if NET10_0_OR_GREATER
                int bracketEnd = line.IndexOf(']', StringComparison.Ordinal);
#else
        int bracketEnd = line.IndexOf(']');
#endif
        if (bracketEnd > 7)
        {
          string indexStr = line[7..bracketEnd];
          if (int.TryParse(indexStr, out int idx))
          {
            string value = ExtractQuotedString(line[(bracketEnd + 2)..]);
            pluralForms[idx] = value;
            currentField = "msgstr[" + idx + "]";
          }
        }
      }
      else if (line.StartsWith("msgstr ", StringComparison.Ordinal))
      {
        msgstr = ExtractQuotedString(line[7..]);
        currentField = "msgstr";
      }
    }

    // Emit final entry if file doesn't end with blank line
    if (msgid is not null)
    {
      EmitEntry(entries, ref language, ref nplurals, msgctxt, msgid, msgidPlural, msgstr, pluralForms, isTemplate);
    }

    return new TranslationSet(language, entries);
  }

  private static void EmitEntry(
      List<TranslationEntry> entries,
      ref string language,
      ref int nplurals,
      string? msgctxt,
      string msgid,
      string? msgidPlural,
      string? msgstr,
      Dictionary<int, string> pluralForms,
      bool isTemplate)
  {
    // Header entry (empty msgid)
    if (msgid.Length == 0)
    {
      ParseHeader(msgstr ?? "", ref language, ref nplurals);
      return;
    }

    // Build the key
    string key = msgctxt is not null
        ? msgctxt + "." + msgid
        : msgid;

    // Determine the value
    string value;
    if (isTemplate)
    {
      value = msgid;
    }
    else
    {
      value = msgstr ?? "";
    }

    // Extract parameters from the value (or msgid for templates)
    string paramSource = isTemplate ? msgid : (msgstr ?? msgid);
    List<string> parameters = ExtractParameters(paramSource);

    // Build plural forms if present
    IReadOnlyDictionary<string, string>? cldrPluralForms = null;
    if (msgidPlural is not null && pluralForms.Count > 0)
    {
      Dictionary<string, string> categoryMap = [];
      string[] categories = nplurals <= DefaultPluralCategories.Length
          ? DefaultPluralCategories[nplurals - 1]
          : DefaultPluralCategories[^1];

      foreach (KeyValuePair<int, string> kvp in pluralForms)
      {
        if (kvp.Key < categories.Length)
        {
          categoryMap[categories[kvp.Key]] = kvp.Value;
        }
      }

      if (categoryMap.Count > 0)
      {
        cldrPluralForms = categoryMap;

        // If this is a plural entry, ensure "count" is in parameters
        var paramList = new List<string>(parameters);
        if (!paramList.Contains("count"))
        {
          paramList.Insert(0, "count");
        }

        parameters = paramList;
      }
    }

    entries.Add(new TranslationEntry(key, value, parameters, cldrPluralForms));
  }

  private static void ParseHeader(string headerText, ref string language, ref int nplurals)
  {
    string[] lines = headerText.Split('\n');

    foreach (string rawLine in lines)
    {
      string line = rawLine.TrimEnd('\r').Trim();

      if (line.StartsWith("Language:", StringComparison.OrdinalIgnoreCase))
      {
        language = line[9..].Trim();
      }
      else if (line.StartsWith("Plural-Forms:", StringComparison.OrdinalIgnoreCase))
      {
        string pluralSpec = line[13..].Trim();
        int npStart = pluralSpec.IndexOf("nplurals=", StringComparison.Ordinal);
        if (npStart >= 0)
        {
          int numStart = npStart + 9;
          int numEnd = pluralSpec.IndexOf(';', numStart);
          if (numEnd < 0)
          {
            numEnd = pluralSpec.Length;
          }

          string numStr = pluralSpec[numStart..numEnd].Trim();
          if (int.TryParse(numStr, out int n))
          {
            nplurals = n;
          }
        }
      }
    }
  }

  private static List<string> ExtractParameters(string text)
  {
    List<string> parameters = [];

    // Check for named placeholders {name} first
    MatchCollection namedMatches = NamedPlaceholderPattern.Matches(text);
    if (namedMatches.Count > 0)
    {
      foreach (Match match in namedMatches)
      {
        string name = match.Groups[1].Value;
        if (!parameters.Contains(name))
        {
          parameters.Add(name);
        }
      }

      return parameters;
    }

    // Check for positional placeholders %s, %d
    MatchCollection positionalMatches = PositionalPlaceholderPattern.Matches(text);
    for (int i = 0; i < positionalMatches.Count; i++)
    {
      parameters.Add("arg" + i);
    }

    return parameters;
  }

  private static string ExtractQuotedString(string text)
  {
    text = text.Trim();

    if (text.Length < 2 || text[0] != '"')
    {
      return text;
    }

    // Find the closing quote
    int end = text.LastIndexOf('"');
    if (end <= 0)
    {
      return text;
    }

    string inner = text[1..end];
    return UnescapeGettext(inner);
  }

  private static string UnescapeGettext(string value)
  {
    var sb = new StringBuilder(value.Length);

    for (int i = 0; i < value.Length; i++)
    {
      if (value[i] == '\\' && i + 1 < value.Length)
      {
        switch (value[i + 1])
        {
          case 'n':
            sb.Append('\n');
            i++;
            break;
          case 't':
            sb.Append('\t');
            i++;
            break;
          case '\\':
            sb.Append('\\');
            i++;
            break;
          case '"':
            sb.Append('"');
            i++;
            break;
          default:
            sb.Append(value[i]);
            break;
        }
      }
      else
      {
        sb.Append(value[i]);
      }
    }

    return sb.ToString();
  }

  private static int ParsePluralIndex(string field)
  {
    // field = "msgstr[N]"
#if NET10_0_OR_GREATER
        int bracketStart = field.IndexOf('[', StringComparison.Ordinal);
        int bracketEnd = field.IndexOf(']', StringComparison.Ordinal);
#else
    int bracketStart = field.IndexOf('[');
    int bracketEnd = field.IndexOf(']');
#endif

    if (bracketStart < 0 || bracketEnd < 0 || bracketEnd <= bracketStart + 1)
    {
      return -1;
    }

    string numStr = field[(bracketStart + 1)..bracketEnd];
    return int.TryParse(numStr, out int idx) ? idx : -1;
  }
}
