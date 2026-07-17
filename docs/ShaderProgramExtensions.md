# ShaderProgramExtensions

`ShaderProgramExtensions` provides a collection of convenience extension methods for the `ShaderProgram` class, simplifying the process of uploading uniform data to GPU shaders. These helpers cover common uniform types such as scalar arrays, vectors, matrices, and also include a utility to query uniform existence.

## API

### `public static void SetFloatArray(this ShaderProgram program, string name, ReadOnlySpan<float> values)`

Uploads an array of `float` values to the uniform identified by `name`.  
- **Parameters**  
  - `program`: The `ShaderProgram` instance on which the uniform is set.  
  - `name`: The exact name of the uniform variable in the shader.  
  - `values`: A read‑only span containing the float values to upload.  
- **Exceptions**  
  - Throws `ArgumentNullException` if `program` or `name` is `null`.  
  - Throws `InvalidOperationException` if the uniform does not exist or its type does not match a float array.

### `public static void SetUIntArray(this ShaderProgram program, string name, ReadOnlySpan<uint> values)`

Uploads an array of `uint` values to the specified uniform.  
- **Parameters**  
  - `program`: Target `ShaderProgram`.  
  - `name`: Uniform name.  
  - `values`: Span of unsigned integers.  
- **Exceptions**  
  - `ArgumentNullException` for `null` `program` or `name`.  
  - `InvalidOperationException` if the uniform is missing or has an incompatible type.

### `public static void SetVector2Array(this ShaderProgram program, string name, ReadOnlySpan<Vector2> values)`

Uploads an array of `Vector2` structs to a `vec2` uniform array.  
- **Parameters**  
  - `program`: The shader program.  
  - `name`: Uniform identifier.  
  - `values`: Span of `Vector2` values.  
- **Exceptions**  
  - `ArgumentNullException` for `null` arguments.  
  - `InvalidOperationException` if the uniform is not a `vec2` array.

### `public static bool HasUniform(this ShaderProgram program, string name)`

Checks whether the shader program contains a uniform with the given name.  
- **Parameters**  
  - `program`: The shader program to query.  
  - `name`: Uniform name to look for.  
- **Returns**  
  - `true` if the uniform exists; otherwise `false`.  
- **Exceptions**  
  - `ArgumentNullException` if `program` or `name` is `null`.

### `public static void SetMatrix4(this ShaderProgram program, string name, Matrix4 matrix)`

Uploads a 4×4 matrix to a `mat4` uniform.  
- **Parameters**  
  - `program`: Target `ShaderProgram`.  
  - `name`: Uniform name.  
  - `matrix`: The `Matrix4` value to upload.  
- **Exceptions**  
  - `ArgumentNullException` for `null` `program` or `name`.  
  - `InvalidOperationException` if the uniform is missing or not a `mat4`.

### `public static void SetVector3(this ShaderProgram program, string name, Vector3 value)`

Uploads a single `Vector3` to a `vec3` uniform.  
- **Parameters**  
  - `program`: The shader program.  
  - `name`: Uniform name.  
  - `value`: The `Vector3` value.  
- **Exceptions**  
  - `ArgumentNullException` for `null` `program` or `name`.  
  - `InvalidOperationException` if the uniform does not exist or is not a `vec3`.

### `public static void SetInt(this ShaderProgram program, string name, int value)`

Uploads an integer to an `int` uniform.  
- **Parameters**  
  - `program`: The shader program.  
  - `name`: Uniform name.  
  - `value`: Integer value to set.  
- **Exceptions**  
  - `ArgumentNullException` for `null` `program` or `name`.  
  - `InvalidOperationException` if the uniform is missing or has a different type.

## Usage

### Example 1 – Updating particle system uniforms

