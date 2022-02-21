
#version 450 core

out vec4 FragColor;

uniform vec3 objectColor;
uniform vec3 lightColor;
uniform vec3 viewPos; 

in vec3 LightPos; 
in vec3 Normal;
in vec3 FragPos;
in float AmbientStrength;

void main(void)
{
    //Calculates the Ambient Lighting
    vec3 ambient = AmbientStrength * lightColor;

    //Calculate Diffuse Lighting
        vec3 norm = normalize(Normal);

        //Calculate distance (light direction) between light source and fragments/pixels on
        vec3 lightDir = normalize(LightPos - FragPos);

        //Calculate diffuse impact by generating dot product of normal and light
        float diff = max(dot(norm, lightDir), 0.0f);

        //Generate diffuse light color
        vec3 diffuse = diff * lightColor;

        //Calculate Specular lighting
        float specularStrength = 0.8f;
        float highlightSize = 6.0f;
    
        //Calculate view direction
        vec3 viewDir = normalize(viewPos - FragPos);

        //Calculate reflection vector
        vec3 reflectDir = reflect(-lightDir, norm);

        //Calculate reflection vector
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), highlightSize); 
        vec3 specular = specularStrength * spec * lightColor;
        vec3 result = (ambient + diffuse + specular) * objectColor;
        FragColor = vec4(result, 1.0f);
    
   
}