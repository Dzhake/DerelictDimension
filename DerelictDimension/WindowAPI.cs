using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using MonoPlus.Logging;
using Serilog;

namespace DerelictDimension;

/// <summary>
/// Class for native windows API calls using <see cref="LibraryImportAttribute"/>
/// </summary>
[SupportedOSPlatform("windows")]
public static partial class WindowsAPI
{
    /// <summary>
    /// Allocates a new console for the calling process.
    /// </summary>
    /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AllocConsole();
}
