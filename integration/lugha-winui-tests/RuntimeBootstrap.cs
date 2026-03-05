// Copyright (c) 2026 Ali Rashid. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for licence information.

using System.Runtime.CompilerServices;
using Microsoft.Windows.ApplicationModel.DynamicDependency;

namespace Lugha.WinUI.Tests;

/// <summary>
/// Initialises the Windows App Runtime at assembly load time so that
/// WinUI types (e.g. <see cref="Microsoft.UI.Dispatching.DispatcherQueueController"/>)
/// are available in the test process.
/// </summary>
internal static class RuntimeBootstrap
{
#pragma warning disable CA2255 // ModuleInitializer is intentional - bootstraps WinUI runtime for tests
  [ModuleInitializer]
  internal static void Initialise()
#pragma warning restore CA2255
  {
    Bootstrap.Initialize(0x00010008); // WindowsAppSDK 1.8
  }
}
