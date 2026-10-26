namespace GpuParticleSandbox.Exceptions;

/// <summary>
/// Phase of startup in which a <see cref="SandboxInitializationException"/> occurred.
/// </summary>
public enum InitializationStage
{
    /// <summary>Configuring the OpenGL context and global state.</summary>
    ContextCreation,

    /// <summary>Compiling or linking shader programs on their own.</summary>
    ShaderLoading,

    /// <summary>Creating GPU resources, including shaders and the particle system.</summary>
    ResourceInitialization,

    /// <summary>Stage could not be determined.</summary>
    Unknown
}

/// <summary>
/// Thrown when the sandbox fails to initialize. <see cref="Stage"/> says which phase failed.
/// </summary>
public sealed class SandboxInitializationException : Exception
{
    /// <summary>Startup phase that failed.</summary>
    public InitializationStage Stage { get; }

    /// <summary>
    /// Creates a new initialization exception.
    /// </summary>
    /// <param name="stage">Startup phase that failed.</param>
    /// <param name="message">Description of the failure.</param>
    /// <param name="inner">Underlying exception, if any.</param>
    public SandboxInitializationException(InitializationStage stage, string message, Exception? inner = null)
        : base(message, inner)
    {
        Stage = stage;
    }
}
