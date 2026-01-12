// Depth Banding Effect for TouchDesigner
// Creates wavy horizontal lines based on depth values

uniform float lineSpacing;   // 2-20, default 10
uniform float waveAmplitude;  // 0-10, default 3
uniform float waveFrequency;  // 0-1, default 0.1
uniform float noiseAmount;    // 0-1, default 0.1
uniform float depthCutoff;    // 0-1, default 0.04 (filters very dark/close areas)

out vec4 fragColor;

// Simple noise function
float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

void main()
{
    vec2 uv = vUV.st;
    ivec2 res = textureSize(sTD2DInputs[0], 0);
    
    // Sample depth map (expects grayscale input)
    vec4 depthColor = texture(sTD2DInputs[0], uv);
    float depth = dot(depthColor.rgb, vec3(0.299, 0.587, 0.114));
    
    // Filter out very dark areas (close to camera)
    if (depth < depthCutoff) {
        fragColor = vec4(0.0, 0.0, 0.0, 1.0);
        return;
    }
    
    // Calculate pixel Y coordinate
    float y = gl_FragCoord.y;
    
    // Dynamic line spacing based on depth (closer = wider spacing)
    float spacing = mix(lineSpacing, 2.0, 1.0 - depth);
    
    // Wave offset based on depth
    float wave = sin(y * waveFrequency + depth * 0.05 * 255.0) * waveAmplitude;
    
    // Create lines
    float line = mod(y + wave, spacing);
    float isLine = step(line, 2.0);
    
    // Add depth-based noise
    float noise = hash(uv * res) * noiseAmount * depth;
    float result = max(isLine, step(0.9, noise));
    
    fragColor = vec4(vec3(result), 1.0);
}
