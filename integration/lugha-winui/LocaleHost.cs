// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.ComponentModel;
using Microsoft.UI.Dispatching;

namespace Lugha.WinUI;

/// <summary>
/// Reactive locale host for WinUI 3. Wraps an <see cref="ILocale"/> and
/// raises <see cref="INotifyPropertyChanged.PropertyChanged"/> when the
/// active locale changes, enabling <c>x:Bind</c> with <c>Mode=OneWay</c>
/// to automatically re-evaluate all text bindings.
/// </summary>
/// <remarks>
/// <para>Thread-safe. <see cref="SetLocale"/> may be called from any thread;
/// property change notifications are dispatched to the UI thread via
/// <see cref="DispatcherQueue"/>.</para>
/// <para>
/// This class is the single point of mutable state in the entire Lugha
/// ecosystem. Its purpose is narrow and explicit: bridging Lugha's pure,
/// immutable locale model with WinUI's reactive binding system.
/// All text resolution remains pure - only the <em>selection</em> of
/// which locale is active is mutable.
/// </para>
/// </remarks>
/// <typeparam name="TLocale">
/// The composite locale interface (e.g. <c>IAppLocale</c>).
/// </typeparam>
public sealed class LocaleHost<TLocale> : INotifyPropertyChanged
    where TLocale : class, ILocale
{
  private readonly DispatcherQueue _dispatcher;

  /// <summary>
  /// Initialises a new <see cref="LocaleHost{TLocale}"/> with the
  /// specified initial locale and dispatcher.
  /// </summary>
  /// <param name="initial">The initial active locale.</param>
  /// <param name="dispatcher">
  /// The <see cref="DispatcherQueue"/> for the UI thread. Property
  /// change notifications are dispatched through this queue.
  /// </param>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="initial"/> or <paramref name="dispatcher"/> is <see langword="null"/>.
  /// </exception>
  public LocaleHost(TLocale initial, DispatcherQueue dispatcher)
  {
    ArgumentNullException.ThrowIfNull(initial);
    ArgumentNullException.ThrowIfNull(dispatcher);
    Current = initial;
    _dispatcher = dispatcher;
  }

  /// <inheritdoc />
  public event PropertyChangedEventHandler? PropertyChanged;

  /// <summary>
  /// The active locale. Bind to this in XAML:
  /// <c>{x:Bind Host.Current.Navigation.Dashboard, Mode=OneWay}</c>.
  /// </summary>
  public TLocale Current
  {
    get => field;
    private set
    {
      if (ReferenceEquals(field, value))
      {
        return;
      }

      field = value;
      NotifyChanged();
    }
  }

  /// <summary>
  /// Switches the active locale. May be called from any thread.
  /// Property change notification is dispatched to the UI thread.
  /// </summary>
  /// <remarks>
  /// This is the only mutating operation in the Lugha ecosystem.
  /// The locale instance itself is immutable and pure - this method
  /// merely selects which immutable instance is active.
  /// </remarks>
  /// <param name="locale">The locale to switch to.</param>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="locale"/> is <see langword="null"/>.
  /// </exception>
  public void SetLocale(TLocale locale)
  {
    ArgumentNullException.ThrowIfNull(locale);

    if (_dispatcher.HasThreadAccess)
    {
      Current = locale;
    }
    else
    {
      _dispatcher.TryEnqueue(() => Current = locale);
    }
  }

  private void NotifyChanged()
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Current)));
  }
}
