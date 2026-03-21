// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.ComponentModel;

namespace Lugha;

/// <summary>
/// Reactive locale host. Wraps an <see cref="ILocale"/> and raises
/// <see cref="INotifyPropertyChanged.PropertyChanged"/> when the active
/// locale changes.
/// </summary>
/// <remarks>
/// <para>Thread-safe. <see cref="SetLocale"/> may be called from any thread;
/// property change notifications are dispatched via the delegate provided
/// at construction.</para>
/// <para>
/// This class is the single point of mutable state in the Lugha ecosystem.
/// Its purpose is narrow and explicit: bridging Lugha's pure, immutable
/// locale model with a reactive binding system. All text resolution
/// remains pure - only the <em>selection</em> of which locale is active
/// is mutable.
/// </para>
/// <para>
/// Framework integrations may derive from this class to expose
/// framework-specific computed properties (e.g. <c>FlowDirection</c>
/// in WinUI). Override <see cref="OnCurrentChanged"/> to raise
/// <see cref="INotifyPropertyChanged.PropertyChanged"/> for derived
/// properties when the active locale changes.
/// </para>
/// </remarks>
/// <typeparam name="TLocale">The composite locale interface.</typeparam>
public class LocaleHost<TLocale> : INotifyPropertyChanged
    where TLocale : class, ILocale
{
  private static readonly PropertyChangedEventArgs CurrentChangedArgs =
      new(nameof(Current));

  private readonly Action<Action> _dispatch;

  /// <summary>
  /// Initialises a new <see cref="LocaleHost{TLocale}"/>.
  /// </summary>
  /// <param name="initial">The initial active locale.</param>
  /// <param name="dispatch">
  /// Delegate that executes an action on the target thread (e.g. UI thread).
  /// For WinUI, use <c>LocaleHostFactory.Create</c> in Lugha.WinUI.
  /// For tests, pass <c>action => action()</c> for synchronous execution.
  /// </param>
  public LocaleHost(TLocale initial, Action<Action> dispatch)
  {
    Current = initial;
    _dispatch = dispatch;
  }

  /// <inheritdoc />
  public event PropertyChangedEventHandler? PropertyChanged;

  /// <summary>The active locale.</summary>
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
      OnPropertyChanged(CurrentChangedArgs);
      OnCurrentChanged();
    }
  }

  /// <summary>
  /// Switches the active locale. May be called from any thread.
  /// The mutation is dispatched via the delegate provided at construction.
  /// </summary>
  public void SetLocale(TLocale locale) =>
      _dispatch(() => Current = locale);

  /// <summary>
  /// Called after <see cref="Current"/> changes and
  /// <see cref="PropertyChanged"/> has been raised for
  /// <see cref="Current"/>. Override in framework-specific subclasses
  /// to raise <see cref="PropertyChanged"/> for derived properties.
  /// </summary>
  protected virtual void OnCurrentChanged()
  {
  }

  /// <summary>
  /// Raises <see cref="PropertyChanged"/> for the specified property.
  /// </summary>
  protected void OnPropertyChanged(PropertyChangedEventArgs e) =>
      PropertyChanged?.Invoke(this, e);
}
