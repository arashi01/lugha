// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Lugha.Import.Resx;

/// <summary>
/// Parses .NET <c>.resx</c> / <c>.resw</c> XML resource files into
/// <see cref="TranslationSet"/> instances.
/// </summary>
/// <remarks>
/// <para>Key derivation: the resource <c>name</c> attribute is used directly.
/// The conventional <c>Uid.Property</c> format (e.g. <c>TestPage_Button.Content</c>)
/// is split into scope + member using <c>.</c> or <c>_</c> as delimiters.</para>
/// <para>Parameters: <c>{0}</c>-style format holes in values are converted
/// to named parameters (<c>{arg0}</c>, <c>{arg1}</c>) or detected from
/// developer comments in the <c>&lt;comment&gt;</c> element if
/// parameter names are provided there.</para>
/// </remarks>
public static class ResxParser
{
  private static readonly Regex FormatHolePattern = new(@"\{(\d+)\}", RegexOptions.Compiled);
  private static readonly Regex CommentParamPattern = new(@"\{(\d+)\}\s*[=:]\s*(\w+)", RegexOptions.Compiled);

  /// <summary>
  /// Parses a single <c>.resx</c> or <c>.resw</c> file.
  /// </summary>
  /// <param name="content">Complete XML text content.</param>
  /// <param name="language">BCP 47 language tag for this file.</param>
  /// <returns>Parsed translation set.</returns>
  public static TranslationSet Parse(string content, string language)
  {
    return ParseCore(content, language);
  }

#if NET10_0_OR_GREATER
    /// <summary>
    /// Parses a single <c>.resx</c> or <c>.resw</c> file from a span.
    /// </summary>
    /// <param name="content">Complete XML text content.</param>
    /// <param name="language">BCP 47 language tag for this file.</param>
    /// <returns>Parsed translation set.</returns>
    public static TranslationSet Parse(ReadOnlySpan<char> content, string language)
    {
        return ParseCore(content.ToString(), language);
    }
#endif

  private static TranslationSet ParseCore(string content, string language)
  {
    List<TranslationEntry> entries = [];

    using var reader = XmlReader.Create(
        new StringReader(content),
        new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });

    while (reader.Read())
    {
      if (reader.NodeType != XmlNodeType.Element || reader.Name != "data")
      {
        continue;
      }

      string? name = reader.GetAttribute("name");
      if (name is null)
      {
        continue;
      }

      string value = "";
      string? comment = null;

      // Read the data element's children
      if (!reader.IsEmptyElement)
      {
        while (reader.Read())
        {
          if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "data")
          {
            break;
          }

          if (reader.NodeType == XmlNodeType.Element && reader.Name == "value")
          {
            value = reader.ReadElementContentAsString();
          }
          else if (reader.NodeType == XmlNodeType.Element && reader.Name == "comment")
          {
            comment = reader.ReadElementContentAsString();
          }
        }
      }

      // Convert the key: replace _ with . for scope.member derivation
      string key = NormaliseKey(name);

      // Extract parameters
      string[] parameters = ExtractParameters(value, comment);

      // Convert {0}-style format holes to {argN} in the value
      string normalisedValue = NormaliseFormatHoles(value, parameters);

      entries.Add(new TranslationEntry(key, normalisedValue, parameters, null));
    }

    return new TranslationSet(language, entries);
  }

  private static string NormaliseKey(string name)
  {
    // Handle WinUI Uid.Property convention: "TestPage_Button.Content"
    // Split on first _ to get scope, then use . for member
    // If there's no _ or ., use the name as-is (single scope)
    // Replace _ with . for consistent dot-delimited keys
#if NET10_0_OR_GREATER
        int underscoreIndex = name.IndexOf('_', StringComparison.Ordinal);
        if (underscoreIndex > 0 && name.IndexOf('.', StringComparison.Ordinal) < 0)
        {
            return string.Concat(name.AsSpan(0, underscoreIndex), ".", name.AsSpan(underscoreIndex + 1));
        }
#else
    int underscoreIndex = name.IndexOf('_');
    if (underscoreIndex > 0 && name.IndexOf('.') < 0)
    {
      return name[..underscoreIndex] + "." + name[(underscoreIndex + 1)..];
    }
#endif

    return name;
  }

  private static string[] ExtractParameters(string value, string? comment)
  {
    MatchCollection holes = FormatHolePattern.Matches(value);
    if (holes.Count == 0)
    {
      return [];
    }

    // Determine the max index to size the parameter list
    int maxIndex = -1;
    foreach (Match match in holes)
    {
      if (int.TryParse(match.Groups[1].Value, out int idx) && idx > maxIndex)
      {
        maxIndex = idx;
      }
    }

    string[] parameters = new string[maxIndex + 1];

    // Default names
    for (int i = 0; i <= maxIndex; i++)
    {
      parameters[i] = "arg" + i;
    }

    // Try to extract named parameters from comment
    if (comment is not null)
    {
      MatchCollection commentMatches = CommentParamPattern.Matches(comment);
      foreach (Match match in commentMatches)
      {
        if (int.TryParse(match.Groups[1].Value, out int idx) && idx <= maxIndex)
        {
          parameters[idx] = match.Groups[2].Value;
        }
      }
    }

    return parameters;
  }

  private static string NormaliseFormatHoles(string value, string[] parameters)
  {
    if (parameters.Length == 0)
    {
      return value;
    }

    return FormatHolePattern.Replace(value, match =>
    {
      if (int.TryParse(match.Groups[1].Value, out int idx) && idx < parameters.Length)
      {
        return "{" + parameters[idx] + "}";
      }

      return match.Value;
    });
  }
}
