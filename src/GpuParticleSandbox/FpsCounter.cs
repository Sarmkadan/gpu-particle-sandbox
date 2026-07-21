using System.Diagnostics;

namespace GpuParticleSandbox;

/// <summary>
/// Tracks frames per second using exponential moving average for smooth updates.
/// The smoothing factor determines how quickly the average adapts to changes:
/// - 0.1 = slow adaptation (good for stable measurements)
/// - 0.5 = medium adaptation
/// - 0.9 = fast adaptation (good for rapid changes)
/// </summary>
public sealed class FpsCounter
{
    private readonly double _smoothingFactor;
    private readonly Stopwatch _stopwatch;
    private double _frameTimeAccumulator;
    private int _frameCount;
    private double _averageFrameTimeMs;
    private double _averageFps;

    /// <summary>
    /// Creates a new FpsCounter with the specified smoothing factor.
    /// </summary>
    /// <param name="smoothingFactor">Smoothing factor between 0.0 and 1.0. Lower values smooth more.</param>
    public FpsCounter(double smoothingFactor = 0.1)
    {
        if (smoothingFactor < 0.0 || smoothingFactor > 1.0)
            throw new ArgumentOutOfRangeException(nameof(smoothingFactor), "Smoothing factor must be between 0.0 and 1.0");

        _smoothingFactor = smoothingFactor;
        _stopwatch = Stopwatch.StartNew();
        _frameTimeAccumulator = 0.0;
        _frameCount = 0;
        _averageFrameTimeMs = 0.0;
        _averageFps = 0.0;
    }

    /// <summary>
    /// Updates the FPS counter with a new frame.
    /// </summary>
    public void Tick()
    {
        var elapsed = _stopwatch.Elapsed.TotalSeconds;
        _stopwatch.Restart();

        _frameCount++;
        _frameTimeAccumulator += elapsed;

        // Calculate average frame time every second
        if (_frameTimeAccumulator >= 1.0)
        {
            double averageFrameTime = _frameTimeAccumulator / _frameCount;
            double fps = 1.0 / averageFrameTime;

            // Update exponential moving average
            _averageFrameTimeMs = _averageFrameTimeMs * (1.0 - _smoothingFactor) + averageFrameTime * _smoothingFactor;
            _averageFps = _averageFps * (1.0 - _smoothingFactor) + fps * _smoothingFactor;

            _frameCount = 0;
            _frameTimeAccumulator = 0.0;
        }
    }

    /// <summary>
    /// Gets the current average frames per second.
    /// </summary>
    public double AverageFps => _averageFps;

    /// <summary>
    /// Gets the current average frame time in milliseconds.
    /// </summary>
    public double AverageFrameTimeMs => _averageFrameTimeMs * 1000.0;

    /// <summary>
    /// Gets a formatted string for display in window title or HUD.
    /// </summary>
    public string GetDisplayString()
    {
        return $"FPS: {_averageFps,5:F1} | Frame: {AverageFrameTimeMs,5:F2}ms";
    }

    /// <summary>
    /// Resets the FPS counter to initial state.
    /// </summary>
    public void Reset()
    {
        _stopwatch.Restart();
        _frameTimeAccumulator = 0.0;
        _frameCount = 0;
        _averageFrameTimeMs = 0.0;
        _averageFps = 0.0;
    }
}
