// Bayer Matrix Dithering for TouchDesigner
// Input: Any texture (RGB or grayscale)
// Output: Dithered black/white

uniform float threshold;  // 0-1, default 0.5
uniform float scale;      // 1-4, default 1.0

out vec4 fragColor;

// 4x4 Bayer matrix
const mat4 bayerMatrix = mat4(
    0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
    12.0/16.0, 4.0/16.0, 14.0/16.0,  6.0/16.0,
    3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
    15.0/16.0, 7.0/16.0, 13.0/16.0,  5.0/16.0
);

void main()
{
    vec2 uv = vUV.st;
    vec4 color = texture(sTD2DInputs[0], uv);
    
    // Convert to grayscale
    float gray = dot(color.rgb, vec3(0.299, 0.587, 0.114));
    
    // Get Bayer threshold based on pixel position
    ivec2 pixelCoord = ivec2(gl_FragCoord.xy * scale);
    int x = pixelCoord.x % 4;
    int y = pixelCoord.y % 4;
    float bayerValue = bayerMatrix[y][x];
    
    // Apply dithering
    float dithered = step(bayerValue * threshold, gray);
    
    fragColor = vec4(vec3(dithered), 1.0);
}
