#version 330 core

out vec4 FragColor;

in vec2 TexCoord;
in vec3 Normal;
in vec3 FragPos;

// Am redenumit 'texture0' in 'uTexture' pentru a evita conflicte de nume
uniform sampler2D uTexture;

uniform vec3 lightPos;   
uniform vec3 viewPos;    
uniform vec3 lightColor; 

void main()
{
    // 1. Ambient
    float ambientStrength = 0.2;
    vec3 ambient = ambientStrength * lightColor;

    // 2. Diffuse
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;

    // 3. Specular
    float specularStrength = 0.5;
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm); 
    
    // CORECTIE: 32 -> 32.0 (GLSL cere float strict)
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0); 
    vec3 specular = specularStrength * spec * lightColor;

    // --- Rezultatul Final ---
    // CORECTIE: Folosim noua denumire 'uTexture'
    vec4 texColor = texture(uTexture, TexCoord);
    
    vec3 result = (ambient + diffuse + specular) * texColor.rgb;
    
    FragColor = vec4(result, texColor.a);
}