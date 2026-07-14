#version 430 core

in float vLife;
out vec4 fragColor;

void main()
{
    // fade from hot white to cool blue as the particle ages out
    float t = clamp(vLife / 4.0, 0.0, 1.0);
    vec3 col = mix(vec3(0.15, 0.35, 0.9), vec3(1.0, 0.95, 0.8), t);
    fragColor = vec4(col, 1.0);
}
