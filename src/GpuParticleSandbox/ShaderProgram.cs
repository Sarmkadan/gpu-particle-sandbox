using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GpuParticleSandbox;

/// <summary>
/// Thin wrapper around a linked GL program plus a uniform-location cache.
/// Handles both the render pair (vert+frag) and standalone compute programs.
/// </summary>
public sealed class ShaderProgram : IDisposable
{
    public int Handle { get; }
    private readonly Dictionary<string, int> _uniformCache = new();

    private ShaderProgram(int handle) => Handle = handle;

    public static ShaderProgram FromCompute(string path)
    {
        int cs = Compile(ShaderType.ComputeShader, File.ReadAllText(path));
        int program = Link(cs);
        return new ShaderProgram(program);
    }

    public static ShaderProgram FromVertexFragment(string vertPath, string fragPath)
    {
        int vs = Compile(ShaderType.VertexShader, File.ReadAllText(vertPath));
        int fs = Compile(ShaderType.FragmentShader, File.ReadAllText(fragPath));
        int program = Link(vs, fs);
        return new ShaderProgram(program);
    }

    public void Use() => GL.UseProgram(Handle);

    public void SetFloat(string name, float value) => GL.Uniform1(Location(name), value);
    public void SetUInt(string name, uint value) => GL.Uniform1(Location(name), value);
    public void SetVector2(string name, Vector2 value) => GL.Uniform2(Location(name), value);
public void SetInt(string name, int value) => GL.Uniform1(Location(name), value);

    private int Location(string name)
    {
        if (_uniformCache.TryGetValue(name, out int cached))
            return cached;

        int loc = GL.GetUniformLocation(Handle, name);
        _uniformCache[name] = loc;
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

        // shaders can be detached/deleted once the program is linked
        foreach (int s in shaders)
        {
            GL.DetachShader(program, s);
            GL.DeleteShader(s);
        }

        return program;
    }

    public void Dispose() => GL.DeleteProgram(Handle);
}
