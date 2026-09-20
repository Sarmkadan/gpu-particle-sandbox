using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GpuParticleSandbox;

/// <summary>
/// Thin wrapper around a linked GL program plus a uniform-location cache.
/// Handles both the render pair (vert+frag) and standalone compute programs.
/// </summary>
/// <remarks>
/// All operations require a current OpenGL context. Failing to set a context before calling
/// any method will result in undefined behavior or exceptions.
/// </remarks>
public sealed class ShaderProgram : IDisposable
{
    /// <summary>
    /// Gets the underlying OpenGL program handle.
    /// </summary>
    public int Handle { get; }
    private readonly Dictionary<string, int> _uniformCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderProgram"/> class.
    /// </summary>
    /// <param name="handle">The OpenGL program handle.</param>
    private ShaderProgram(int handle) => Handle = handle;

    /// <summary>
    /// Creates and loads a compute shader program from the specified file path.
    /// </summary>
    /// <param name="path">The path to the compute shader file.</param>
    /// <returns>A linked <see cref="ShaderProgram"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the shader fails to compile or the program fails to link.</exception>
    public static ShaderProgram FromCompute(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        int cs = Compile(ShaderType.ComputeShader, File.ReadAllText(path));
        int program = Link(cs);
        return new ShaderProgram(program);
    }

    /// <summary>
    /// Creates and loads a vertex and fragment shader program from the specified file paths.
    /// </summary>
    /// <param name="vertPath">The path to the vertex shader file.</param>
    /// <param name="fragPath">The path to the fragment shader file.</param>
    /// <returns>A linked <see cref="ShaderProgram"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the shaders fail to compile or the program fails to link.</exception>
    public static ShaderProgram FromVertexFragment(string vertPath, string fragPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(vertPath);
        ArgumentException.ThrowIfNullOrEmpty(fragPath);
        int vs = Compile(ShaderType.VertexShader, File.ReadAllText(vertPath));
        int fs = Compile(ShaderType.FragmentShader, File.ReadAllText(fragPath));
        int program = Link(vs, fs);
        return new ShaderProgram(program);
    }

    /// <summary>
    /// Binds this program as the current rendering program.
    /// </summary>
    public void Use() => GL.UseProgram(Handle);

    /// <summary>
    /// Sets a float uniform value.
    /// </summary>
    /// <param name="name">The name of the uniform variable.</param>
    /// <param name="value">The value to set.</param>
    public void SetFloat(string name, float value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        GL.Uniform1(Location(name), value);
    }

    /// <summary>
    /// Sets an unsigned integer uniform value.
    /// </summary>
    /// <param name="name">The name of the uniform variable.</param>
    /// <param name="value">The value to set.</param>
    public void SetUInt(string name, uint value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        GL.Uniform1(Location(name), value);
    }

    /// <summary>
    /// Sets a 2D vector uniform value.
    /// </summary>
    /// <param name="name">The name of the uniform variable.</param>
    /// <param name="value">The value to set.</param>
    public void SetVector2(string name, Vector2 value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        GL.Uniform2(Location(name), value);
    }

    /// <summary>
    /// Sets an integer uniform value.
    /// </summary>
    /// <param name="name">The name of the uniform variable.</param>
    /// <param name="value">The value to set.</param>
    public void SetInt(string name, int value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        GL.Uniform1(Location(name), value);
    }

    private int Location(string name)
    {
        if (_uniformCache.TryGetValue(name, out int cached))
            return cached;

        int loc = GL.GetUniformLocation(Handle, name);
        _uniformCache[name] = loc;
        if (loc == -1)
            Console.Error.WriteLine($"[shader] Uniform '{name}' was not found in program {Handle}.");

        return loc;
    }

    private static int Compile(ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
        if (status == 0)
        {
            string log = GL.GetShaderInfoLog(shader);
            throw new InvalidOperationException($"{type} failed to compile:\n{log}");
        }

        string warningLog = GL.GetShaderInfoLog(shader);
        if (!string.IsNullOrWhiteSpace(warningLog))
            Console.Error.WriteLine($"[shader] {type} warning: {warningLog}");

        return shader;
    }

    private static int Link(params int[] shaders)
    {
        int program = GL.CreateProgram();
        foreach (int s in shaders)
            GL.AttachShader(program, s);

        GL.LinkProgram(program);
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int status);
        if (status == 0)
        {
            string log = GL.GetProgramInfoLog(program);
            throw new InvalidOperationException($"Program link failed:\n{log}");
        }

        string warningLog = GL.GetProgramInfoLog(program);
        if (!string.IsNullOrWhiteSpace(warningLog))
            Console.Error.WriteLine($"[shader] Program link warning: {warningLog}");

        // shaders can be detached/deleted once the program is linked
        foreach (int s in shaders)
        {
            GL.DetachShader(program, s);
            GL.DeleteShader(s);
        }

        return program;
    }

    /// <summary>
    /// Releases all resources used by the <see cref="ShaderProgram"/>.
    /// </summary>
    public void Dispose() => GL.DeleteProgram(Handle);
}
