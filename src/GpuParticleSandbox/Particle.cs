using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace GpuParticleSandbox;

/// <summary>
/// CPU-side mirror of the GLSL Particle struct. The padding fields are here on
/// purpose - std430 rounds the struct up to 32 bytes and the layouts have to
/// match byte-for-byte or the SSBO upload gets misaligned.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Particle
{
    /// <summary>Particle position.</summary>
    public Vector2 Position;

    /// <summary>Particle velocity.</summary>
    public Vector2 Velocity;

    /// <summary>Remaining lifetime in seconds.</summary>
    public float Life;

    /// <summary>std430 padding; keeps the struct at <see cref="SizeInBytes"/>.</summary>
    public float Pad0;

    /// <summary>std430 padding; keeps the struct at <see cref="SizeInBytes"/>.</summary>
    public float Pad1;

    /// <summary>std430 padding; keeps the struct at <see cref="SizeInBytes"/>.</summary>
    public float Pad2;

    /// <summary>Size of one particle in the SSBO, in bytes.</summary>
    public const int SizeInBytes = 32;
}
