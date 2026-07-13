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
    public Vector2 Position;
    public Vector2 Velocity;
    public float Life;
    public float Pad0;
    public float Pad1;
    public float Pad2;

    public const int SizeInBytes = 32;
}
