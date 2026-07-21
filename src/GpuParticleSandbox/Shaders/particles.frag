#version 430 core

in float vLife;
in float vVelocityMagnitude;
out vec4 fragColor;

uniform int uColorMode; // 0 = age-based, 1 = velocity-based, 2 = uniform

void main()
{
    vec3 col;

    if (uColorMode == 1) // velocity-based coloring
    {
        // Blue (0, 0, 1) at low velocity, Red (1, 0, 0) at high velocity
        float normalizedVelocity = clamp(vVelocityMagnitude, 0.0, 1.0);
        col = vec3(normalizedVelocity, 0.0, 1.0 - normalizedVelocity);
    }
    else if (uColorMode == 0) // age-based coloring (original)
    {
        // fade from hot white to cool blue as the particle ages out
        float t = clamp(vLife / 4.0, 0.0, 1.0);
        col = mix(vec3(0.15, 0.35, 0.9), vec3(1.0, 0.95, 0.8), t);
    }
    else // uniform white
    {
        col = vec3(1.0);
    }

    fragColor = vec4(col, 1.0);
}
