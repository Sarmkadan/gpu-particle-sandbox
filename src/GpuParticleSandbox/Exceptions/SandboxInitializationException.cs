namespace GpuParticleSandbox.Exceptions;

public enum InitializationStage
{
    ContextCreation,
    ShaderLoading,
    ResourceInitialization,
    Unknown
}

public sealed class SandboxInitializationException : Exception
{
    public InitializationStage Stage { get; }

    public SandboxInitializationException(InitializationStage stage, string message, Exception? inner = null)
        : base(message, inner)
    {
        Stage = stage;
    }
}
