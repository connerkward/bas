// ASCII Art Effect for TouchDesigner
// Converts video to ASCII-like character patterns

uniform float cellSize;      // 8-16, default 12 (pixels per character)
uniform float contrast;       // 0-2, default 1.0
uniform float threshold;      // 0-1, default 0.5

out vec4 fragColor;

// ASCII brightness ramp (from dark to light characters)
// Using simplified pattern lookup instead of actual characters
float getAsciiPattern(vec2 cellCoord, float brightness) {
    // Normalize cell coordinates to 0-1
    vec2 uv = fract(cellCoord);
    
    // Simple patterns that approximate ASCII density
    float pattern = 0.0;
    
    if (brightness < 0.2) {
        // Very dark: sparse dots
        pattern = step(0.45, uv.x) * step(0.45, uv.y) * step(uv.x, 0.55) * step(uv.y, 0.55);
    }
    else if (brightness < 0.4) {
        // Dark: thin lines
        pattern = step(abs(uv.x - 0.5), 0.1) + step(abs(uv.y - 0.5), 0.1);
    }
    else if (brightness < 0.6) {
        // Medium: cross pattern
        pattern = step(abs(uv.x - 0.5), 0.15) + step(abs(uv.y - 0.5), 0.15);
    }
    else if (brightness < 0.8) {
        // Bright: thick cross
        pattern = step(abs(uv.x - 0.5), 0.3) + step(abs(uv.y - 0.5), 0.3);
    }
    else {
        // Very bright: almost solid
        pattern = 1.0 - (step(0.9, uv.x) * step(0.9, uv.y));
    }
    
    return clamp(pattern, 0.0, 1.0);
}

void main()
{
    vec2 uv = vUV.st;
    ivec2 res = textureSize(sTD2DInputs[0], 0);
    
    // Calculate cell position
    vec2 cellCoord = uv * vec2(res) / cellSize;
    vec2 cellIndex = floor(cellCoord);
    
    // Sample color at cell center
    vec2 cellCenter = (cellIndex + 0.5) * cellSize / vec2(res);
    vec4 color = texture(sTD2DInputs[0], cellCenter);
    
    // Convert to brightness
    float brightness = dot(color.rgb, vec3(0.299, 0.587, 0.114));
    brightness = pow(brightness, 1.0 / contrast);
    brightness = smoothstep(threshold - 0.2, threshold + 0.2, brightness);
    
    // Get ASCII pattern
    float pattern = getAsciiPattern(cellCoord, brightness);
    
    // Output with optional color tint
    vec3 asciiColor = color.rgb * pattern;
    fragColor = vec4(asciiColor, 1.0);
}
