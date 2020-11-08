#version 330 core

in vec3 pos;
in vec3 norm;
in vec3 cam;
in float seconds;
out vec3 outColor;

void main() {
    outColor = norm;
}