using System.Reflection;
using System.Runtime.InteropServices;

namespace AngelVisionLprDemo.AngelVisionLpr;

internal static class NativeLibraryLoader
{
    private static bool _configured;

    public static void Configure()
    {
        if (_configured)
        {
            return;
        }

        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), Resolve);
        _configured = true;
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!string.Equals(libraryName, NativeMethods.LibraryName, StringComparison.Ordinal))
        {
            return IntPtr.Zero;
        }

        foreach (var candidate in GetCandidates())
        {
            if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out var handle))
            {
                return handle;
            }
        }

        return IntPtr.Zero;
    }

    private static IEnumerable<string> GetCandidates()
    {
        var baseDirectory = AppContext.BaseDirectory;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            yield return Path.Combine(baseDirectory, "libs", "win-x64", "libav_lpr_c.dll");
            yield return Path.Combine(baseDirectory, "libs", "libav_lpr_c.dll");
            yield return Path.Combine(baseDirectory, "libav_lpr_c.dll");
            yield break;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            yield return Path.Combine(baseDirectory, "libs", "linux-x64", "libav_lpr_c.so");
            yield return Path.Combine(baseDirectory, "libs", "libav_lpr_c.so");
            yield return Path.Combine(baseDirectory, "libav_lpr_c.so");
            yield break;
        }

        yield return Path.Combine(baseDirectory, "libav_lpr_c");
    }
}
