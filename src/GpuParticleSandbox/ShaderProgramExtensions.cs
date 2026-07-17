using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GpuParticleSandbox;

/// <summary>
/// Provides extension methods for <see cref="ShaderProgram"/> to simplify common shader operations.
/// </summary>
public static class ShaderProgramExtensions
{
	/// <summary>
	/// Sets a uniform float array value in the shader program.
	/// </summary>
	/// <param name="program">The shader program.</param>
	/// <param name="name">The name of the uniform variable.</param>
	/// <param name="values">The float values to set.</param>
	/// <exception cref="ArgumentNullException"><paramref name="program"/> or <paramref name="name"/> is <see langword="null"/></exception>
	/// <exception cref="ArgumentException"><paramref name="name"/> is empty or whitespace.</exception>
	public static void SetFloatArray(this ShaderProgram program, string name, ReadOnlySpan<float> values)
	{
		ArgumentNullException.ThrowIfNull(program);
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		if (values.Length == 0)
		{
			return;
		}

		program.Use();
		int location = GL.GetUniformLocation(program.Handle, name);
		for (int i = 0; i < values.Length; i++)
		{
			GL.Uniform1(location + i, values[i]);
		}
	}

	/// <summary>
	/// Sets a uniform uint array value in the shader program.
	/// </summary>
	/// <param name="program">The shader program.</param>
	/// <param name="name">The name of the uniform variable.</param>
	/// <param name="values">The uint values to set.</param>
	/// <exception cref="ArgumentNullException"><paramref name="program"/> or <paramref name="name"/> is <see langword="null"/></exception>
	/// <exception cref="ArgumentException"><paramref name="name"/> is empty or whitespace.</exception>
	public static void SetUIntArray(this ShaderProgram program, string name, ReadOnlySpan<uint> values)
	{
		ArgumentNullException.ThrowIfNull(program);
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		if (values.Length == 0)
		{
			return;
		}

		program.Use();
		int location = GL.GetUniformLocation(program.Handle, name);
		for (int i = 0; i < values.Length; i++)
		{
			GL.Uniform1(location + i, values[i]);
		}
	}

	/// <summary>
	/// Sets a uniform Vector2 array value in the shader program.
	/// </summary>
	/// <param name="program">The shader program.</param>
	/// <param name="name">The name of the uniform variable.</param>
	/// <param name="values">The Vector2 values to set.</param>
	/// <exception cref="ArgumentNullException"><paramref name="program"/> or <paramref name="name"/> is <see langword="null"/></exception>
	/// <exception cref="ArgumentException"><paramref name="name"/> is empty or whitespace.</exception>
	public static void SetVector2Array(this ShaderProgram program, string name, ReadOnlySpan<Vector2> values)
	{
		ArgumentNullException.ThrowIfNull(program);
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		if (values.Length == 0)
		{
			return;
		}

		program.Use();
		int location = GL.GetUniformLocation(program.Handle, name);
		for (int i = 0; i < values.Length; i++)
		{
			GL.Uniform2(location + i, values[i]);
		}
	}

	/// <summary>
	/// Checks if a uniform with the given name exists in the shader program.
	/// </summary>
	/// <param name="program">The shader program.</param>
	/// <param name="name">The name of the uniform variable to check.</param>
	/// <returns><see langword="true"/> if the uniform exists; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="program"/> or <paramref name="name"/> is <see langword="null"/></exception>
	/// <exception cref="ArgumentException"><paramref name="name"/> is empty or whitespace.</exception>
	public static bool HasUniform(this ShaderProgram program, string name)
	{
		ArgumentNullException.ThrowIfNull(program);
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		return GL.GetUniformLocation(program.Handle, name) != -1;
	}

	/// <summary>
	/// Sets a uniform Matrix4 value in the shader program.
	/// </summary>
	/// <param name="program">The shader program.</param>
	/// <param name="name">The name of the uniform variable.</param>
	/// <param name="matrix">The Matrix4 value to set.</param>
	/// <exception cref="ArgumentNullException"><paramref name="program"/> or <paramref name="name"/> is <see langword="null"/></exception>
	/// <exception cref="ArgumentException"><paramref name="name"/> is empty or whitespace.</exception>
	public static void SetMatrix4(this ShaderProgram program, string name, Matrix4 matrix)
	{
		ArgumentNullException.ThrowIfNull(program);
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		program.Use();
		GL.UniformMatrix4(GL.GetUniformLocation(program.Handle, name), false, ref matrix);
	}

	/// <summary>
	/// Sets a uniform Vector3 value in the shader program.
	/// </summary>
	/// <param name="program">The shader program.</param>
	/// <param name="name">The name of the uniform variable.</param>
	/// <param name="value">The Vector3 value to set.</param>
	/// <exception cref="ArgumentNullException"><paramref name="program"/> or <paramref name="name"/> is <see langword="null"/></exception>
	/// <exception cref="ArgumentException"><paramref name="name"/> is empty or whitespace.</exception>
	public static void SetVector3(this ShaderProgram program, string name, Vector3 value)
	{
		ArgumentNullException.ThrowIfNull(program);
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		program.Use();
		GL.Uniform3(GL.GetUniformLocation(program.Handle, name), value);
	}

	/// <summary>
	/// Sets a uniform int value in the shader program.
	/// </summary>
	/// <param name="program">The shader program.</param>
	/// <param name="name">The name of the uniform variable.</param>
	/// <param name="value">The int value to set.</param>
	/// <exception cref="ArgumentNullException"><paramref name="program"/> or <paramref name="name"/> is <see langword="null"/></exception>
	/// <exception cref="ArgumentException"><paramref name="name"/> is empty or whitespace.</exception>
	public static void SetInt(this ShaderProgram program, string name, int value)
	{
		ArgumentNullException.ThrowIfNull(program);
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		program.Use();
		GL.Uniform1(GL.GetUniformLocation(program.Handle, name), value);
	}
}