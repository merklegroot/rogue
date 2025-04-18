#version 120

varying vec2 fragTexCoord;
varying vec4 fragColor;
uniform sampler2D texture0;

void main()
{
    gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0); // Force everything bright red
}