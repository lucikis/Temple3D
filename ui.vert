#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal; // Ignoram, dar trebuie sa existe pentru Mesh-ul tau
layout(location = 2) in vec2 aTexCoord;

out vec2 TexCoord;

uniform mat4 model;
uniform mat4 projection;

void main()
{
    // Randam fara View Matrix, doar Model si Proiectie Ortografica
    gl_Position = vec4(aPosition, 1.0) * model * projection;
    TexCoord = aTexCoord;
}