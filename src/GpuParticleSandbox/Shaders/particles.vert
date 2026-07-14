#version 430 core

// Pulls position straight out of the SSBO the compute stage just wrote.
struct Particle
{
    vec2 position;
    vec2 velocity;
    float life;
    float _pad0;
    float _pad1;
    float _pad2;
};

layout(std430, binding = 0) buffer Particles
{
    Particle particles[];
};

out float vLife;

void main()
{
    Particle p = particles[gl_VertexID];
    vLife = p.life;
    gl_Position = vec4(p.position, 0.0, 1.0);
    gl_PointSize = 2.0;
}
