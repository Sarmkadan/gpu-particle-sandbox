# ParticleSystemExtensions
The `ParticleSystemExtensions` class provides a set of static methods for interacting with and manipulating particle systems in the gpu-particle-sandbox project. These extensions enable features such as setting gravity wells, updating particle systems, rendering particles, and managing system resources.

## API
* `public static void SetGravityWell`: Sets a gravity well for the particle system. This method does not take any parameters, but its effect is dependent on the current state of the particle system. It does not return any value and does not throw any exceptions.
* `public static void UpdateFixed`: Updates the particle system with a fixed time step. This method does not take any parameters and does not return any value. It does not throw any exceptions.
* `public static void Render`: Renders the particle system. This method does not take any parameters and does not return any value. It does not throw any exceptions.
* `public static bool SafeDispose`: Attempts to safely dispose of the particle system resources. This method does not take any parameters and returns a boolean indicating whether the disposal was successful. It does not throw any exceptions.
* `public static long GetMemoryUsage`: Retrieves the current memory usage of the particle system. This method does not take any parameters and returns the memory usage as a long value. It does not throw any exceptions.
* `public static int GetParticleCount`: Retrieves the current number of particles in the system. This method does not take any parameters and returns the particle count as an integer value. It does not throw any exceptions.

## Usage
```csharp
// Example 1: Basic particle system setup and rendering
ParticleSystem ps = new ParticleSystem();
ParticleSystemExtensions.SetGravityWell();
ParticleSystemExtensions.UpdateFixed();
ParticleSystemExtensions.Render();
```

```csharp
// Example 2: Resource management and particle count retrieval
if (ParticleSystemExtensions.SafeDispose()) {
    Console.WriteLine("Particle system resources disposed successfully.");
} else {
    Console.WriteLine("Failed to dispose particle system resources.");
}
int particleCount = ParticleSystemExtensions.GetParticleCount();
long memoryUsage = ParticleSystemExtensions.GetMemoryUsage();
Console.WriteLine($"Particle count: {particleCount}, Memory usage: {memoryUsage} bytes");
```

## Notes
When using the `ParticleSystemExtensions` class, be aware that the `SetGravityWell` method modifies the particle system's state, and its effects may persist across multiple updates and renders. The `UpdateFixed` method updates the particle system with a fixed time step, which may not be suitable for all scenarios. The `Render` method assumes that the particle system is in a valid state and that the necessary resources are available. The `SafeDispose` method attempts to release system resources, but its success is not guaranteed. The `GetMemoryUsage` and `GetParticleCount` methods provide information about the current state of the particle system, but their values may change rapidly due to updates and other operations. Additionally, the thread-safety of these methods is not guaranteed, and concurrent access to the particle system may lead to unpredictable behavior.
