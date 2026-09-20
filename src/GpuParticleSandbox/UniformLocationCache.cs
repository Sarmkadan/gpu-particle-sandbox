using System.Collections.Generic;

namespace GpuParticleSandbox;

/// <summary>
/// Caches and resolves OpenGL uniform locations without requiring a GL context.
/// </summary>
public sealed class UniformLocationCache
{
    private readonly Dictionary<string, int> _cache = new();

    /// <summary>
    /// Retrieves the cached location for <paramref name="name"/>, or resolves it using <paramref name="resolver"/> and caches the result.
    /// </summary>
    public int GetOrResolve(string name, System.Func<string, int> resolver)
    {
        if (_cache.TryGetValue(name, out int cached))
            return cached;

        int loc = resolver(name);
        _cache[name] = loc;
        if (loc == -1)
            System.Console.Error.WriteLine($"[shader] Uniform '{name}' was not found.");

        return loc;
    }
}
