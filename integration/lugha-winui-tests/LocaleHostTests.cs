// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using Microsoft.UI.Dispatching;

namespace Lugha.WinUI.Tests;

public sealed class LocaleHostTests
{
  /// <summary>
  /// Creates a <see cref="DispatcherQueue"/> on a dedicated thread,
  /// invokes the test action, then shuts down the queue.
  /// </summary>
  private static async Task RunWithDispatcherAsync(Func<DispatcherQueue, Task> action)
  {
    var controller = DispatcherQueueController.CreateOnDedicatedThread();
    try
    {
      await action(controller.DispatcherQueue);
    }
    finally
    {
      await controller.ShutdownQueueAsync();
    }
  }

  [Fact]
  public async Task Constructor_sets_initial_current()
  {
    await RunWithDispatcherAsync(dispatcher =>
    {
      var locale = new TestEnGbLocale();
      var host = new LocaleHost<ILocale>(locale, dispatcher);

      host.Current.Should().BeSameAs(locale);
      return Task.CompletedTask;
    });
  }

  [Fact]
  public async Task Constructor_rejects_null_initial()
  {
    await RunWithDispatcherAsync(dispatcher =>
    {
      Action act = () => _ = new LocaleHost<ILocale>(null!, dispatcher);

      act.Should().Throw<ArgumentNullException>()
              .WithParameterName("initial");
      return Task.CompletedTask;
    });
  }

  [Fact]
  public void Constructor_rejects_null_dispatcher()
  {
    var locale = new TestEnGbLocale();
    Action act = () => _ = new LocaleHost<ILocale>(locale, null!);

    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("dispatcher");
  }

  [Fact]
  public async Task SetLocale_rejects_null()
  {
    await RunWithDispatcherAsync(dispatcher =>
    {
      var host = new LocaleHost<ILocale>(new TestEnGbLocale(), dispatcher);
      Action act = () => host.SetLocale(null!);

      act.Should().Throw<ArgumentNullException>()
              .WithParameterName("locale");
      return Task.CompletedTask;
    });
  }

  [Fact]
  public async Task SetLocale_from_dispatcher_thread_updates_current()
  {
    await RunWithDispatcherAsync(async dispatcher =>
    {
      var initial = new TestEnGbLocale();
      var replacement = new TestArSaLocale();
      var host = new LocaleHost<ILocale>(initial, dispatcher);

      var tcs = new TaskCompletionSource();
      dispatcher.TryEnqueue(() =>
          {
          host.SetLocale(replacement);
          tcs.SetResult();
        });

      await tcs.Task;
      host.Current.Should().BeSameAs(replacement);
    });
  }

  [Fact]
  public async Task SetLocale_fires_PropertyChanged_for_Current()
  {
    await RunWithDispatcherAsync(async dispatcher =>
    {
      var host = new LocaleHost<ILocale>(new TestEnGbLocale(), dispatcher);
      List<string> firedProperties = [];

      host.PropertyChanged += (_, e) =>
          {
          if (e.PropertyName is not null)
          {
            firedProperties.Add(e.PropertyName);
          }
        };

      var tcs = new TaskCompletionSource();
      dispatcher.TryEnqueue(() =>
          {
          host.SetLocale(new TestArSaLocale());
          tcs.SetResult();
        });

      await tcs.Task;
      firedProperties.Should().ContainSingle()
              .Which.Should().Be(nameof(LocaleHost<ILocale>.Current));
    });
  }

  [Fact]
  public async Task SetLocale_with_same_instance_does_not_fire_PropertyChanged()
  {
    await RunWithDispatcherAsync(async dispatcher =>
    {
      var locale = new TestEnGbLocale();
      var host = new LocaleHost<ILocale>(locale, dispatcher);
      bool fired = false;

      host.PropertyChanged += (_, _) => fired = true;

      var tcs = new TaskCompletionSource();
      dispatcher.TryEnqueue(() =>
          {
          host.SetLocale(locale);
          tcs.SetResult();
        });

      await tcs.Task;
      fired.Should().BeFalse();
    });
  }

  [Fact]
  public async Task SetLocale_from_background_thread_dispatches_to_dispatcher()
  {
    await RunWithDispatcherAsync(async dispatcher =>
    {
      var initial = new TestEnGbLocale();
      var replacement = new TestArSaLocale();
      var host = new LocaleHost<ILocale>(initial, dispatcher);

      var tcs = new TaskCompletionSource();
      host.PropertyChanged += (_, _) => tcs.SetResult();

      // Call from a background thread (not the dispatcher thread)
      await Task.Run(() => host.SetLocale(replacement));

      // Wait for the dispatcher to process the enqueued callback
      Task completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(5)));
      completed.Should().BeSameAs(tcs.Task, "PropertyChanged should fire within timeout");

      host.Current.Should().BeSameAs(replacement);
    });
  }
}
