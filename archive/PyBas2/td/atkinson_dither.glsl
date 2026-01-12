// Atkinson Dithering for TouchDesigner
// Note: Error diffusion dithering is hard in shaders due to sequential processing
// This is a simplified multi-pass approximation

uniform float threshold;  // 0-1, default 0.5

out vec4 fragColor;

void main()
{
    vec2 uv = vUV.st;
    vec2 res = textureSize(sTD2DInputs[0], 0);
    vec4 color = texture(sTD2DInputs[0], uv);
    
    // Convert to grayscale
    float gray = dot(color.rgb, vec3(0.299, 0.587, 0.114));
    
    // Simplified error diffusion - accumulate from neighbors
    vec2 pixelSize = 1.0 / res;
    
    // Sample neighbors with Atkinson weights
    float error = 0.0;
    error += texture(sTD2DInputs[0], uv + vec2(-pixelSize.x, 0)).r * 0.125;
    error += texture(sTD2DInputs[0], uv + vec2(-2.0*pixelSize.x, 0)).r * 0.125;
    error += texture(sTD2DInputs[0], uv + vec2(pixelSize.x, -pixelSize.y)).r * 0.125;
    error += texture(sTD2DInputs[0], uv + vec2(0, -pixelSize.y)).r * 0.125;
    error += texture(sTD2DInputs[0], uv + vec2(-pixelSize.x, -pixelSize.y)).r * 0.125;
    error += texture(sTD2DInputs[0], uv + vec2(0, -2.0*pixelSize.y)).r * 0.125;
    
    // Apply accumulated error
    float adjusted = gray + error * 0.5;
    float dithered = step(threshold, adjusted);
    
    fragColor = vec4(vec3(dithered), 1.0);
}
