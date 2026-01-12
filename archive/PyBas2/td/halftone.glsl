// Halftone/Dot Pattern Effect
// Similar aesthetic to ASCII but using dots

uniform float dotSize;    // 4-20, default 10
uniform float contrast;   // 0.5-2, default 1.2

out vec4 fragColor;

void main()
{
    vec2 uv = vUV.st;
    ivec2 res = textureSize(sTD2DInputs[0], 0);
    
    // Grid position
    vec2 gridUV = uv * vec2(res) / dotSize;
    vec2 gridIndex = floor(gridUV);
    vec2 gridFract = fract(gridUV);
    
    // Sample brightness at grid center
    vec2 sampleUV = (gridIndex + 0.5) * dotSize / vec2(res);
    vec4 color = texture(sTD2DInputs[0], sampleUV);
    float brightness = dot(color.rgb, vec3(0.299, 0.587, 0.114));
    brightness = pow(brightness, 1.0 / contrast);
    
    // Create circular dot scaled by brightness
    vec2 center = vec2(0.5);
    float dist = length(gridFract - center);
    float radius = brightness * 0.5;
    
    float pattern = smoothstep(radius + 0.02, radius - 0.02, dist);
    
    // Output
    vec3 outputColor = color.rgb * pattern;
    fragColor = vec4(outputColor, 1.0);
}
