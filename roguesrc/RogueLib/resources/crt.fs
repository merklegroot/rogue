#version 120

// Input vertex attributes (from vertex shader)
varying vec2 fragTexCoord;
varying vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
uniform vec2 resolution;
uniform float time;

// CRT effect parameters - you can adjust these
uniform float curvature;           // Screen curvature amount (0.0 to 0.5)
uniform float scanlineIntensity;   // Scanline darkness (0.0 to 1.0)
uniform float scanlineCount;       // Number of scanlines
uniform float vignetteIntensity;   // Vignette darkness (0.0 to 1.0)
uniform float brightness;          // Overall brightness adjustment
uniform float distortion;          // RGB distortion amount
uniform float flickerIntensity;    // Screen flicker intensity

// Apply screen curvature
vec2 curveRemapUV(vec2 uv)
{
    // Scale up the UV coordinates to fill the blank spaces
    // This makes the content slightly larger to compensate for curvature
    float scale = 1.08;  // Decreased from 1.15 to 1.08 (8% larger)
    uv = (uv - 0.5) / scale + 0.5;
    
    // Convert UV from 0..1 to -1..1
    vec2 cuv = uv * 2.0 - 1.0;
    
    // Apply curvature - modified to be much more subtle
    float dist = length(cuv);
    cuv = cuv * (1.0 + dist * dist * curvature);
    
    // Convert back to 0..1
    return cuv * 0.5 + 0.5;
}

// Apply RGB distortion (chromatic aberration)
vec3 applyRGBDistortion(sampler2D tex, vec2 uv)
{
    float r = texture2D(tex, uv + vec2(distortion, 0.0)).r;
    float g = texture2D(tex, uv).g;
    float b = texture2D(tex, uv - vec2(distortion, 0.0)).b;
    return vec3(r, g, b);
}

// Apply vignette effect
float vignette(vec2 uv)
{
    uv = (uv - 0.5) * 2.0;
    return clamp(1.0 - dot(uv, uv) * vignetteIntensity, 0.0, 1.0);
}

// Apply scanlines
float scanlines(vec2 uv)
{
    return 1.0 - scanlineIntensity * sin(uv.y * scanlineCount) * sin(uv.y * scanlineCount);
}

// Apply screen flicker
float flicker()
{
    return 1.0 - flickerIntensity * sin(time * 10.0);
}

void main()
{
    // Apply screen curvature
    vec2 uv = curveRemapUV(fragTexCoord);
    
    // Discard pixels outside the curved screen
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) {
        gl_FragColor = vec4(0.0, 0.0, 0.0, 1.0);
        return;
    }
    
    // Apply RGB distortion
    vec3 color = applyRGBDistortion(texture0, uv);
    
    // Apply scanlines
    color *= scanlines(uv);
    
    // Apply vignette
    color *= vignette(uv);
    
    // Apply flicker
    color *= flicker();
    
    // Apply brightness adjustment
    color *= brightness;
    
    // Output final color
    gl_FragColor = vec4(color, 1.0);
}