#version 330 core

uniform mat4 modelTransformation;
uniform mat4 viewProjectionTransformation;
uniform vec3 cameraPos;

layout(location = 0) in vec3 vertexPosModelspace;
layout(location = 2) in vec3 normalCoordinate;

out vec3 pos;
out vec3 norm;
out vec3 cam;

void main()
{
	pos = vertexPosModelspace;
	norm = normalCoordinate;
	cam = cameraPos;
    gl_Position = viewProjectionTransformation * modelTransformation * vec4(vertexPosModelspace, 1.0);
}