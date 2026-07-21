namespace GpuParticleSandbox;

/// <summary>
/// Unit tests for FpsCounter exponential moving average algorithm.
/// These tests verify the core math and behavior without requiring a graphics context.
/// </summary>
public static class FpsCounterTests
{
    /// <summary>
    /// Test that the FpsCounter initializes correctly with default smoothing.
    /// </summary>
    public static void TestInitialization()
    {
        var counter = new FpsCounter();
        Assert(counter.AverageFps == 0.0, "Initial FPS should be 0");
        Assert(counter.AverageFrameTimeMs == 0.0, "Initial frame time should be 0");
    }

    /// <summary>
    /// Test that custom smoothing factors are accepted and clamped correctly.
    /// </summary>
    public static void TestSmoothingFactorValidation()
    {
        // Valid smoothing factors
        var counter1 = new FpsCounter(0.0);
        var counter2 = new FpsCounter(0.5);
        var counter3 = new FpsCounter(1.0);

        // Invalid smoothing factors should throw
        try
        {
            var counterInvalid = new FpsCounter(-0.1);
            Assert(false, "Negative smoothing factor should throw exception");
        }
        catch (ArgumentOutOfRangeException)
        {
            // Expected
        }

        try
        {
            var counterInvalid = new FpsCounter(1.1);
            Assert(false, "Smoothing factor > 1.0 should throw exception");
        }
        catch (ArgumentOutOfRangeException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Test exponential moving average calculation with known values.
    /// </summary>
    public static void TestExponentialMovingAverage()
    {
        // Use a slow smoothing factor for predictable results
        var counter = new FpsCounter(0.1);

        // Simulate frames coming in at 60 FPS (16.67ms per frame)
        // After 10 frames at 60 FPS, the average should be close to 60
        for (int i = 0; i < 10; i++)
        {
            counter.Tick();
            // Simulate frame time of ~16.67ms (60 FPS)
            System.Threading.Thread.Sleep(16);
        }

        // Give it time to calculate the average
        System.Threading.Thread.Sleep(1100);

        // The average should be close to 60 FPS
        var fps = counter.AverageFps;
        Assert(fps > 55 && fps < 65, $"FPS should be ~60, got {fps}");

        // Frame time should be ~16.67ms
        var frameTime = counter.AverageFrameTimeMs;
        Assert(frameTime > 15 && frameTime < 18, $"Frame time should be ~16.67ms, got {frameTime}");
    }

    /// <summary>
    /// Test that display string formatting works correctly.
    /// </summary>
    public static void TestDisplayString()
    {
        var counter = new FpsCounter(0.1);
        var display = counter.GetDisplayString();

        Assert(display.Contains("FPS:"), "Display string should contain 'FPS:'");
        Assert(display.Contains("Frame:"), "Display string should contain 'Frame:'");
        Assert(display.Contains("ms"), "Display string should contain 'ms'");
    }

    /// <summary>
    /// Test reset functionality.
    /// </summary>
    public static void TestReset()
    {
        var counter = new FpsCounter(0.1);

        // Simulate some frames
        for (int i = 0; i < 5; i++)
        {
            counter.Tick();
            System.Threading.Thread.Sleep(16);
        }
        System.Threading.Thread.Sleep(1100);

        // Should have non-zero values
        Assert(counter.AverageFps > 0, "FPS should be > 0 after frames");

        // Reset
        counter.Reset();

        // Should be back to zero
        Assert(counter.AverageFps == 0.0, "FPS should be 0 after reset");
        Assert(counter.AverageFrameTimeMs == 0.0, "Frame time should be 0 after reset");
    }

    /// <summary>
    /// Test rapid frame rate changes.
    /// </summary>
    public static void TestRapidChanges()
    {
        var counter = new FpsCounter(0.5); // Faster smoothing for responsiveness

        // Simulate high FPS (100 FPS = 10ms per frame)
        for (int i = 0; i < 5; i++)
        {
            counter.Tick();
            System.Threading.Thread.Sleep(10);
        }
        System.Threading.Thread.Sleep(1100);

        double fps1 = counter.AverageFps;

        // Simulate low FPS (30 FPS = 33ms per frame)
        for (int i = 0; i < 5; i++)
        {
            counter.Tick();
            System.Threading.Thread.Sleep(33);
        }
        System.Threading.Thread.Sleep(1100);

        double fps2 = counter.AverageFps;

        // The average should have moved from ~100 to ~30
        Assert(Math.Abs(fps1 - fps2) > 20, "FPS should change significantly with different frame rates");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Assertion failed: {message}");
        }
    }
}
