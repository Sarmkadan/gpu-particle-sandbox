using GpuParticleSandbox.Exceptions;

namespace GpuParticleSandbox.Tests;

public class SandboxInitializationExceptionTests
{
    [Fact]
    public void Constructor_SetsStageAndMessage()
    {
        var ex = new SandboxInitializationException(InitializationStage.ShaderLoading, "Failed to load shaders");
        Assert.Equal(InitializationStage.ShaderLoading, ex.Stage);
        Assert.Equal("Failed to load shaders", ex.Message);
    }

    [Fact]
    public void Constructor_WithInnerException_PropagatesInner()
    {
        var inner = new InvalidOperationException("Inner error");
        var ex = new SandboxInitializationException(InitializationStage.ContextCreation, "Context failed", inner);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Constructor_DefaultStage_IsUnknown()
    {
        var ex = new SandboxInitializationException(InitializationStage.Unknown, "Test");
        Assert.Equal(InitializationStage.Unknown, ex.Stage);
    }

    [Fact]
    public void Stage_PersistsThroughSerializationContract()
    {
        var ex = new SandboxInitializationException(InitializationStage.ResourceInitialization, "Resource init failed");
        Assert.Equal(InitializationStage.ResourceInitialization, ex.Stage);
    }
}
