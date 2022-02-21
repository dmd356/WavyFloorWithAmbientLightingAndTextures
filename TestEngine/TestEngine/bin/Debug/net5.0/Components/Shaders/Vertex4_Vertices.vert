#version 450 core

layout(location = 0) in vec4 _position;
layout(location = 1) in vec4 color;
layout(location = 2) in vec4 _Normal;
layout(location = 3) in vec3 _FragPosition;
layout(location = 4) in vec2 _TexCoords;

out vec3 LightPos;
out vec4 vs_color;
out vec3 FragPos;
out float AmbientStrength;
out vec3 Normal;
out vec2 TexCoords;

// we now define the uniform in the vertex shader and pass the 'view space' lightpos to the fragment shader. lightPos is currently in world space.
uniform vec3 lightPos;
uniform float ambientStrength;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;


void main(void)
{
	FragPos = vec3(_position * model);
	gl_Position = vec4(_position) * model *  view  * projection;
	vs_color = vec4(color);
	AmbientStrength =ambientStrength; 
	TexCoords = _TexCoords;
	LightPos = vec3(view * vec4(lightPos, 1.0)); // Transform world-space light position to view-space light position
}