using System.Diagnostics;

namespace GpuParticleSandbox;

/// <summary>
/// Tracks and smooths frame rate statistics using an exponential moving average (EMA) over a 1-second sampling window.
/// </summary>
/// <remarks>
/// <para>
/// <b>Thread-Safety:</b> This class is not thread-safe. All public members must be called from a single thread,
/// typically the main rendering thread. Concurrent access from multiple threads will result in undefined behavior.
/// </para>
/// <para>
/// <b>Usage:</b> Call <see cref="Tick"/> (or its alias <see cref="Update"/>) exactly once per rendered frame,
/// ideally at the end of the frame loop after rendering commands have been issued but before presenting the frame.
/// The method measures the elapsed time since the previous call to calculate instantaneous frame time and updates
/// the smoothed averages based on the configured <see cref="FpsCounter(double)"/>.
/// </para>
/// </remarks>
public sealed class FpsCounter
{
    private readonly double _smoothingFactor;
    private readonly Stopwatch _stopwatch;
    private double _frameTimeAccumulator;
    private int _frameCount;
    private double _averageFrameTimeMs;
    private double _averageFps;

    /// <summary>
    /// Creates a new <see cref="FpsCounter"/> with the specified smoothing factor.
    /// </summary>
    /// <param name="smoothingFactor">
    /// Smoothing factor between 0.0 and 1.0. Lower values result in slower adaptation (smoother readings),
    /// while higher values result in faster adaptation (more responsive to sudden frame rate changes).
    /// </param>
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
    /// Records a new frame and updates the smoothed FPS and frame time averages.
    /// </summary>
    /// <remarks>
    /// Should be called exactly once per frame. Measures the time elapsed since the previous call to <see cref="Tick"/>
    /// or <see cref="Update"/>. The internal sampling window is fixed at 1.0 second.
    /// </remarks>
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
    /// Alias for <see cref="Tick"/> to match common game loop naming conventions.
    /// </summary>
    public void Update() => Tick();

    /// <summary>
    /// Gets the current smoothed average frames per second.
    /// </summary>
    /// <value>
    /// The average FPS over the 1-second sampling window, smoothed by the configured factor.
    /// Units: frames per second (Hz).
    /// </value>
    public double AverageFps => _averageFps;

    /// <summary>
    /// Gets the current smoothed average frame time.
    /// </summary>
    /// <value>
    /// The average frame time over the 1-second sampling window, smoothed by the configured factor.
    /// Units: milliseconds (ms).
    /// </value>
    public double AverageFrameTimeMs => _averageFrameTimeMs * 1000.0;

    /// <summary>
    /// Gets a formatted string suitable for display in a window title or heads-up display (HUD).
    /// </summary>
    /// <returns>A string containing the current average FPS and average frame time in milliseconds.</returns>
    public string GetDisplayString()
    {
        return $"FPS: {_averageFps,5:F1} | Frame: {AverageFrameTimeMs,5:F2}ms";
    }

    /// <summary>
    /// Resets the internal state of the counter to its initial values.
    /// </summary>
    /// <remarks>
    /// Resets the stopwatch, accumulators, and averages. The next call to <see cref="Tick"/> will start a new measurement cycle.
    /// </remarks>
    public void Reset()
    {
        _stopwatch.Restart();
        _frameTimeAccumulator = 0.0;
        _frameCount = 0;
        _averageFrameTimeMs = 0.0;
        _averageFps = 0.0;
    }
}
