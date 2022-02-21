#version 450 core
in vec4 vs_color;
out vec4 out_color;

in vec2 TexCoords;

uniform sampler2D Texture0;

void main(void)
{
	out_color = texture(Texture0, TexCoords);
	//color = vs_color;
}