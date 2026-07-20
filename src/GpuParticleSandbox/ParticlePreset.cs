using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTK.Mathematics;

namespace GpuParticleSandbox;

/// <summary>
/// Represents a saved preset for particle system configuration.
/// </summary>
public readonly record struct ParticlePreset
{
    /// <summary>
    /// Particle count in the system
    /// </summary>
    public int ParticleCount { get; init; }

    /// <summary>
    /// Color mode for rendering (0 = default, 1 = gradient, 2 = random)
    /// </summary>
    public int ColorMode { get; init; }

    /// <summary>
    /// Emitter shape type
    /// </summary>
    public ParticleSystem.EmitterShape Shape { get; init; }

    /// <summary>
    /// Emitter parameters (shape-specific)
    /// </summary>
    public Vector2 EmitterParams { get; init; }

    /// <summary>
    /// Simulation speed multiplier
    /// </summary>
    public float SimulationSpeed { get; init; }

    /// <summary>
    /// Gravity well strength
    /// </summary>
    public float WellStrength { get; init; }

    /// <summary>
    /// Gravity well radius
    /// </summary>
    public float WellRadius { get; init; }

    /// <summary>
    /// Whether the simulation is paused
    /// </summary>
    public bool IsPaused { get; init; }

    /// <summary>
    /// Default preset with reasonable values
    /// </summary>
    public static ParticlePreset Default => new ParticlePreset
    {
        ParticleCount = 100_000,
        ColorMode = 0,
        Shape = ParticleSystem.EmitterShape.Point,
        EmitterParams = Vector2.Zero,
        SimulationSpeed = 1.0f,
        WellStrength = 0.15f,
        WellRadius = 0.0f,
        IsPaused = false
    };

    /// <summary>
    /// Saves the preset to a JSON file
    /// </summary>
    /// <param name="preset">The preset to save</param>
    /// <param name="filePath">Path to the presets file</param>
    public static void Save(ParticlePreset preset, string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(preset, options);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Loads a preset from a JSON file
    /// </summary>
    /// <param name="filePath">Path to the presets file</param>
    /// <returns>The loaded preset, or Default if file doesn't exist</returns>
    public static ParticlePreset Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return Default;
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var preset = JsonSerializer.Deserialize<ParticlePreset>(json, options);
            if (preset.Equals(default(ParticlePreset)))
            {
                return Default;
            }
            return preset;
        }
        catch
        {
            // If loading fails, return default
            return Default;
        }
    }

    /// <summary>
    /// Creates a preset from the current system state
    /// </summary>
    public static ParticlePreset FromSystem(
        int particleCount,
        int colorMode,
        ParticleSystem.EmitterShape shape,
        Vector2 emitterParams,
        float simulationSpeed,
        float wellStrength,
        float wellRadius,
        bool isPaused)
    {
        return new ParticlePreset
        {
            ParticleCount = particleCount,
            ColorMode = colorMode,
            Shape = shape,
            EmitterParams = emitterParams,
            SimulationSpeed = simulationSpeed,
            WellStrength = wellStrength,
            WellRadius = wellRadius,
            IsPaused = isPaused
        };
    }
}