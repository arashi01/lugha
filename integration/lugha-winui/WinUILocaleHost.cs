// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.ComponentModel;
using Microsoft.UI.Xaml;

namespace Lugha.WinUI;

/// <summary>
/// WinUI-specific <see cref="LocaleHost{TLocale}"/> that exposes
/// <see cref="FlowDirection"/> as a reactive property. When the active
/// locale changes, <see cref="INotifyPropertyChanged.PropertyChanged"/>
/// fires for both <c>Current</c> and <c>FlowDirection</c>, enabling
/// direct <c>x:Bind</c> without converters or code-behind bridges.
/// </summary>
/// <typeparam name="TLocale">The composite locale interface.</typeparam>
public sealed class WinUILocaleHost<TLocale> : LocaleHost<TLocale>
    where TLocale : class, ILocale
{
  private static readonly PropertyChangedEventArgs FlowDirectionChangedArgs =
      new(nameof(FlowDirection));

  internal WinUILocaleHost(TLocale initial, Action<Action> dispatch)
      : base(initial, dispatch)
  {
  }

  /// <summary>
  /// The <see cref="Microsoft.UI.Xaml.FlowDirection"/> for the active
  /// locale's writing system. Re-evaluated automatically when
  /// <see cref="LocaleHost{TLocale}.Current"/> changes.
  /// </summary>
  public FlowDirection FlowDirection =>
      Current.FlowDirection();

  /// <inheritdoc />
  protected override void OnCurrentChanged() =>
      OnPropertyChanged(FlowDirectionChangedArgs);
}
