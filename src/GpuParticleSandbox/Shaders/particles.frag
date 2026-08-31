#version 430 core

in float vLife;
in float vVelocityMagnitude;
out vec4 fragColor;

uniform int uColorMode; // 0 = age-based, 1 = velocity/heat-based, 2 = single-hue

void main()
{
    vec3 col;

    if (uColorMode == 1)
    {
        float heat = clamp(vVelocityMagnitude, 0.0, 1.0);
        col = mix(vec3(0.05, 0.1, 0.8), vec3(1.0, 0.15, 0.02), heat);
    }
    else if (uColorMode == 2)
    {
        col = vec3(0.15, 0.9, 0.65);
    }
    else
    {
        // fade from hot white to cool blue as the particle ages out
        float t = clamp(vLife / 4.0, 0.0, 1.0);
        col = mix(vec3(0.15, 0.35, 0.9), vec3(1.0, 0.95, 0.8), t);
    }

    fragColor = vec4(col, 1.0);
}
