// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Microsoft.UI.Dispatching;

namespace Lugha.WinUI;

/// <summary>
/// Factory for creating <see cref="WinUILocaleHost{TLocale}"/> instances
/// bound to a WinUI <see cref="DispatcherQueue"/>.
/// </summary>
public static class LocaleHostFactory
{
  /// <summary>
  /// Creates a <see cref="WinUILocaleHost{TLocale}"/> that dispatches
  /// property change notifications via <paramref name="dispatcher"/>.
  /// The returned host exposes <see cref="WinUILocaleHost{TLocale}.FlowDirection"/>
  /// as a reactive property suitable for direct <c>x:Bind</c>.
  /// </summary>
  /// <param name="initial">The initial active locale.</param>
  /// <param name="dispatcher">The UI thread dispatcher.</param>
  public static WinUILocaleHost<TLocale> Create<TLocale>(
      TLocale initial,
      DispatcherQueue dispatcher)
      where TLocale : class, ILocale
  {
    return new WinUILocaleHost<TLocale>(initial, action =>
    {
      if (dispatcher.HasThreadAccess)
      {
        action();
      }
      else
      {
        dispatcher.TryEnqueue(() => action());
      }
    });
  }
}
