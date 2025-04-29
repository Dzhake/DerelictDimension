using System.Runtime.InteropServices;

namespace DerelictDimension;

/// <summary>
/// Class for native windows API calls using <see cref="LibraryImportAttribute"/>
/// </summary>
public static partial class WindowsAPI
{
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AllocConsole();
}
