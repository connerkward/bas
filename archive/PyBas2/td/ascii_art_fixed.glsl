// ASCII Art Effect - Fixed
// Creates proper ASCII-like character patterns

uniform float cellSize;      // 8-20, default 12
uniform float contrast;       // 0.5-3, default 1.5

out vec4 fragColor;

// Character patterns using SDF (signed distance field) approach
float character(int n, vec2 p) {
    float result = 0.0;
    
    // Normalize to character cell space (0-1)
    p = p * 2.0 - 1.0;
    
    if (n == 0) { // Space (empty)
        result = 1.0;
    }
    else if (n == 1) { // Dot .
        result = length(p) - 0.15;
    }
    else if (n == 2) { // Colon :
        float d1 = length(p - vec2(0.0, 0.3)) - 0.15;
        float d2 = length(p - vec2(0.0, -0.3)) - 0.15;
        result = min(d1, d2);
    }
    else if (n == 3) { // Plus +
        float d1 = abs(p.x) - 0.1;
        float d2 = abs(p.y) - 0.1;
        result = min(max(d1, abs(p.y) - 0.6), max(d2, abs(p.x) - 0.6));
    }
    else if (n == 4) { // Hash #
        float d1 = min(abs(abs(p.x) - 0.3) - 0.1, abs(abs(p.y) - 0.3) - 0.1);
        result = d1;
    }
    else if (n == 5) { // Percent %
        float d1 = length(p - vec2(-0.4, 0.4)) - 0.2;
        float d2 = length(p - vec2(0.4, -0.4)) - 0.2;
        float d3 = length(p - vec2(-0.5, 0.5) * (1.0 - abs(p.x + p.y)));
        result = min(d1, d2);
    }
    else if (n == 6) { // At @
        float outer = length(p) - 0.6;
        float inner = length(p) - 0.3;
        result = max(outer, -inner);
    }
    else if (n == 7) { // Asterisk *
        float d1 = abs(p.x) - 0.05;
        float d2 = abs(p.y) - 0.05;
        float d3 = abs(p.x + p.y) - 0.05;
        float d4 = abs(p.x - p.y) - 0.05;
        result = min(min(max(d1, abs(p.y) - 0.5), max(d2, abs(p.x) - 0.5)), 
                    min(max(d3, length(p) - 0.5), max(d4, length(p) - 0.5)));
    }
    else { // Default to square
        result = max(abs(p.x), abs(p.y)) - 0.7;
    }
    
    return result;
}

void main()
{
    vec2 uv = vUV.st;
    ivec2 res = textureSize(sTD2DInputs[0], 0);
    
    // Calculate which cell we're in
    vec2 cellCoord = uv * vec2(res) / cellSize;
    vec2 cellIndex = floor(cellCoord);
    vec2 cellUV = fract(cellCoord);
    
    // Sample video at cell center for brightness
    vec2 sampleUV = (cellIndex + 0.5) * cellSize / vec2(res);
    vec4 color = texture(sTD2DInputs[0], sampleUV);
    
    // Get brightness
    float brightness = dot(color.rgb, vec3(0.299, 0.587, 0.114));
    brightness = pow(brightness, 1.0 / contrast);
    
    // Map brightness to character (0-7 range)
    int charIndex = int(brightness * 7.0);
    charIndex = clamp(charIndex, 0, 7);
    
    // Get character pattern
    float dist = character(charIndex, cellUV);
    
    // Convert distance to color (sharp edge)
    float pattern = 1.0 - smoothstep(-0.02, 0.02, dist);
    
    // Output with color tint from original
    vec3 outputColor = color.rgb * pattern;
    
    fragColor = vec4(outputColor, 1.0);
}
