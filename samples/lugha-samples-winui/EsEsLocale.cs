// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Globalization;
using Lugha.Rules.Cardinals;
using Lugha.Rules.Ordinals;

namespace Lugha.Samples.WinUI;

/// <summary>Spanish (Spain) locale.</summary>
public sealed class EsEsLocale : IAppLocale, ILocale<LatinEuropeanCardinal, SpanishOrdinal>
{
  /// <inheritdoc />
  public CultureInfo Culture { get; } = CultureInfo.GetCultureInfo("es-ES");

  /// <inheritdoc />
  public IConnectionText Connection { get; } = new EsEsConnectionText();

  /// <inheritdoc />
  public INavigationText Navigation { get; } = new EsEsNavigationText();

  /// <inheritdoc />
  public IStatusText Status { get; } = new EsEsStatusText();
}
