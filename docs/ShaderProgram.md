# ShaderProgram

Represents a compiled and linked OpenGL shader program, encapsulating the program handle and providing uniform-setting utilities. Instances are created through static factory methods that compile and link shaders from source strings, and must be disposed to release GPU resources.

## API

### `public int Handle`
Gets the OpenGL program object name. Valid only after successful creation and before disposal. Used internally by `Use` and uniform setters; rarely needed directly.

### `public static ShaderProgram FromCompute(string computeSource)`
Creates a program containing only a compute shader.

**Parameters**  
- `computeSource`: GLSL source code for the compute stage.

**Returns**  
A linked `ShaderProgram` ready for compute dispatch.

**Throws**  
- `ShaderCompilationException` if the compute shader fails to compile.  
- `ShaderLinkException` if program linking fails.  
- `ArgumentNullException` if `computeSource` is null.

### `public static ShaderProgram FromVertexFragment(string vertexSource, string fragmentSource)`
Creates a program containing a vertex and a fragment shader.

**Parameters**  
- `vertexSource`: GLSL source code for the vertex stage.  
- `fragmentSource`: GLSL source code for the fragment stage.

**Returns**  
A linked `ShaderProgram` ready for graphics rendering.

**Throws**  
- `ShaderCompilationException` if either shader fails to compile.  
- `ShaderLinkException` if program linking fails.  
- `ArgumentNullException` if either source is null.

### `public void Use()`
Binds this program as the current OpenGL program. Subsequent uniform calls affect this program. Must be called before any `Set*` method.

**Throws**  
- `ObjectDisposedException` if the program has been disposed.  
- `InvalidOperationException` if no OpenGL context is current.

### `public void SetFloat(string name, float value)`
Sets a `float` uniform in the currently bound program.

**Parameters**  
- `name`: Uniform name as declared in GLSL.  
- `value`: Value to assign.

**Throws**  
- `ObjectDisposedException` if the program has been disposed.  
- `ArgumentException` if the uniform does not exist or type mismatch occurs.  
- `InvalidOperationException` if `Use` has not been called on this program.

### `public void SetUInt(string name, uint value)`
Sets a `uint` uniform in the currently bound program.

**Parameters**  
- `name`: Uniform name as declared in GLSL.  
- `value`: Value to assign.

**Throws**  
- `ObjectDisposedException` if the program has been disposed.  
- `ArgumentException` if the uniform does not exist or type mismatch occurs.  
- `InvalidOperationException` if `Use` has not been called on this program.

### `public void SetVector2(string name, Vector2 value)`
Sets a `vec2` uniform in the currently bound program.

**Parameters**  
- `name`: Uniform name as declared in GLSL.  
- `value`: `Vector2` value to assign.

**Throws**  
- `ObjectDisposedException` if the program has been disposed.  
- `ArgumentException` if the uniform does not exist or type mismatch occurs.  
- `InvalidOperationException` if `Use` has not been called on this program.

### `public void Dispose()`
Deletes the underlying OpenGL program object and releases associated resources. Safe to call multiple times. After disposal, `Handle` becomes zero and all other members throw `ObjectDisposedException`.

## Usage

### Compute shader for particle simulation
```csharp
string computeSrc = @"
#version 430
layout(local_size_x = 64) in;
layout(std430, binding = 0) buffer Particles { vec4 pos[]; };
uniform float dt;
void main() {
    uint i = gl_GlobalInvocationID.x;
    pos[i].x += dt * 0.1;
}
";

using var compute = ShaderProgram.FromCompute(computeSrc);
compute.Use();
compute.SetFloat("dt", 0.016f);
GL.DispatchCompute(particleCount / 64, 1, 1);
GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
```

### Vertex/fragment shader for rendering particles
```csharp
string vertSrc = @"
#version 430
layout(std430, binding = 0) buffer Particles { vec4 pos[]; };
void main() {
    gl_Position = pos[gl_VertexID];
    gl_PointSize = 4.0;
}
";

string fragSrc = @"
#version 430
out vec4 color;
void main() {
    color = vec4(1.0, 0.5, 0.2, 1.0);
}
";

using var render = ShaderProgram.FromVertexFragment(vertSrc, fragSrc);
render.Use();
GL.DrawArrays(PrimitiveType.Points, 0, particleCount);
```

## Notes

- **Thread safety**: `ShaderProgram` is not thread-safe. All methods must be called from the thread that owns the OpenGL context. Creating, using, or disposing a program from multiple threads without external synchronization causes undefined behavior.
- **Context lifetime**: The OpenGL context must be current during construction, `Use`, all `Set*` calls, and `Dispose`. Creating a program without a current context throws `InvalidOperationException`.
- **Uniform location caching**: The `Set*` methods resolve uniform locations on each call. For hot paths, consider caching locations externally via `GL.GetUniformLocation(Handle, name)`.
- **Disposal order**: Dispose programs after all commands referencing them have been submitted and completed. Deleting a program while it is bound or in use by a pending dispatch/draw may cause driver errors.
- **Error handling**: Factory methods throw descriptive exceptions containing shader info logs. Catch `ShaderCompilationException` and `ShaderLinkException` to log or display GLSL diagnostics.
- **Handle reuse**: After `Dispose`, `Handle` returns 0. Creating a new program may reuse the same integer value; do not retain `Handle` across disposal boundaries.
