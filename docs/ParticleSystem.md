# ParticleSystem

The `ParticleSystem` class manages the lifecycle, state, and rendering of GPU-accelerated particle systems within the `gpu-particle-sandbox` framework. It provides methods for updating particle behavior on both CPU and GPU, rendering particles via graphics APIs, and proper resource disposal. The class integrates with a `GravityWell` component to simulate gravitational effects on particles.

## API

### `GravityWell`
A public field or property representing the gravitational influence affecting particles in the system. The exact type and mutability depend on implementation details in `ParticleSystem.cs`.

### `ParticleSystem`
The default constructor initializes a new instance of the `ParticleSystem` class. It likely sets up internal buffers, shaders, or other GPU resources required for particle simulation.

### `void Update()`
Updates the particle system state on the GPU. This method is typically called once per frame to advance particle positions, velocities, and other dynamic properties based on the current simulation parameters.

### `void Update(float deltaTime)`
An overloaded version of `Update` that accepts a `deltaTime` parameter to control the time step for particle simulation. This allows for frame-rate-independent updates when precise timing control is required.

### `void UpdateCpu()`
Updates the particle system state on the CPU. This method may be used for debugging, hybrid simulations, or scenarios where GPU updates are not feasible. It likely synchronizes CPU-side particle data with GPU buffers.

### `void Render()`
Renders the particle system using the configured graphics pipeline. This method binds necessary shaders, sets uniforms, and issues draw calls to visualize particles on screen.

### `void Dispose()`
Releases all unmanaged resources held by the `ParticleSystem`, including GPU buffers, shader programs, and any other allocated memory. This method should be called explicitly to prevent resource leaks.

## Usage

```csharp
// Example 1: Basic particle system setup and update
var particleSystem = new ParticleSystem();
particleSystem.GravityWell = new Vector2(0.5f, 0.5f); // Apply gravitational force

while (window.IsOpen)
{
    particleSystem.Update(1.0f / 60.0f); // Update with fixed timestep
    particleSystem.Render();
    window.Update();
}

particleSystem.Dispose(); // Clean up resources
```

```csharp
// Example 2: CPU-side update for hybrid simulation
var particleSystem = new ParticleSystem();
particleSystem.GravityWell = new Vector2(0.0f, -0.1f); // Simulate downward gravity

// Update on CPU for custom logic
particleSystem.UpdateCpu();

// Render the result
particleSystem.Render();
```

## Notes

- **Thread Safety**: All methods (`Update`, `UpdateCpu`, `Render`, `Dispose`) are not thread-safe. They must be called from the main thread or a thread with active graphics context. Concurrent calls may result in undefined behavior or GPU driver errors.
- **Resource Management**: `Dispose()` must be called explicitly to release GPU resources. Failure to do so may cause memory leaks or resource exhaustion. Calling `Dispose()` multiple times has undefined behavior.
- **Update Order**: `UpdateCpu()` and `Update()` should not be called in the same frame unless explicitly required, as they may conflict in updating particle states. Prefer one update method per frame.
- **GravityWell Dependency**: The `GravityWell` field must be initialized before calling update methods. Uninitialized values may lead to unexpected particle behavior or runtime exceptions.
