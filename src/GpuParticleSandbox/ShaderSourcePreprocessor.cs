using System.Text.RegularExpressions;

namespace GpuParticleSandbox;

/// <summary>
/// Handles preprocessing of GLSL shader sources, such as resolving <c>#include</c> directives.
/// </summary>
public static class ShaderSourcePreprocessor
{
    private static readonly Regex IncludeRegex = new(@"#include\s+""([^""]+)""", RegexOptions.Compiled);

    /// <summary>
    /// Processes <c>#include</c> directives in the shader source.
    /// </summary>
    /// <param name="source">The raw shader source.</param>
    /// <param name="includeResolver">A function that resolves an include path to its source content.</param>
    /// <returns>The processed shader source with includes inlined.</returns>
    public static string ProcessIncludes(string source, System.Func<string, string> includeResolver)
    {
        if (string.IsNullOrEmpty(source))
            return source;

        return IncludeRegex.Replace(source, match =>
        {
            string includePath = match.Groups[1].Value;
            try
            {
                return includeResolver(includePath);
            }
            catch
            {
                return $"// Error including {includePath}";
            }
        });
    }
}
