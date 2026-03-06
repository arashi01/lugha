// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

namespace Lugha.Samples.WinUI;

/// <summary>
/// Composite locale interface for the sample application.
/// Composes all generated text scopes into a single contract.
/// </summary>
public interface IAppLocale : ILocale
{
  /// <summary>Connection status text scope.</summary>
  public IConnectionText Connection { get; }

  /// <summary>Navigation text scope.</summary>
  public INavigationText Navigation { get; }

  /// <summary>Status text scope.</summary>
  public IStatusText Status { get; }
}
