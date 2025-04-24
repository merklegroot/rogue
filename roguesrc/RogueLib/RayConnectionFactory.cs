using Raylib_cs;
using RogueLib.Constants;

namespace RogueLib;

public interface IRayConnection : IDisposable
{
    Font MenuFont { get; init; }
    Texture2D CharsetTexture { get; init; }
    Shader CrtShader { get; init; }
    RenderTexture2D GameTexture { get; init; }
    int TimeLoc { get; init; }
}

public interface IRayConnectionFactory
{
    IRayConnection Connect();
}

public class RayConnectionFactory(IRayLoader rayLoader) : IRayConnectionFactory
{
    private class RayConnection : IRayConnection
    {
        public Font MenuFont { get; init; }
        public Texture2D CharsetTexture { get; init; }
        public Texture2D SkullTexture { get; init; }
        public Shader CrtShader { get; init; }
        public RenderTexture2D GameTexture { get; init; }
        public int ResolutionLoc { get; init; }
        public int CurvatureLoc { get; init; }
        public int ScanlineLoc { get; init; }
        public int VignetteLoc { get; init; }
        public int BrightnessLoc { get; init; }
        public int DistortionLoc { get; init; }
        public int FlickerLoc { get; init; }
        public int TimeLoc { get; init; }
        public int ScanlineCountLoc { get; init; }

        public RayConnection(IRayLoader rayLoader)
        {
            MenuFont = rayLoader.LoadRobotoFont();
            CharsetTexture = rayLoader.LoadCharsetTexture();
            SkullTexture = rayLoader.LoadSkullImage();
            
            // Create a render texture the size of the window
            int width = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
            int height = ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale;
            GameTexture = Raylib.LoadRenderTexture(width, height);
            
            // Load the CRT shader
            CrtShader = rayLoader.LoadCrtShader();
            
            // Get shader uniform locations
            ResolutionLoc = Raylib.GetShaderLocation(CrtShader, "resolution");
            CurvatureLoc = Raylib.GetShaderLocation(CrtShader, "curvature");
            ScanlineLoc = Raylib.GetShaderLocation(CrtShader, "scanlineIntensity");
            ScanlineCountLoc = Raylib.GetShaderLocation(CrtShader, "scanlineCount");
            VignetteLoc = Raylib.GetShaderLocation(CrtShader, "vignetteIntensity");
            BrightnessLoc = Raylib.GetShaderLocation(CrtShader, "brightness");
            DistortionLoc = Raylib.GetShaderLocation(CrtShader, "distortion");
            FlickerLoc = Raylib.GetShaderLocation(CrtShader, "flickerIntensity");
            TimeLoc = Raylib.GetShaderLocation(CrtShader, "time");
            
            // Set initial uniform values
            float[] resolution = { width, height };
            Raylib.SetShaderValue(CrtShader, ResolutionLoc, resolution, ShaderUniformDataType.Vec2);
            
            // Fix the extreme warping by drastically reducing curvature
            Raylib.SetShaderValue(CrtShader, CurvatureLoc, 0.05f, ShaderUniformDataType.Float);
            Raylib.SetShaderValue(CrtShader, ScanlineLoc, 0.1f, ShaderUniformDataType.Float);
            Raylib.SetShaderValue(CrtShader, ScanlineCountLoc, 240.0f, ShaderUniformDataType.Float);
            Raylib.SetShaderValue(CrtShader, VignetteLoc, 0.1f, ShaderUniformDataType.Float);
            Raylib.SetShaderValue(CrtShader, BrightnessLoc, 1.05f, ShaderUniformDataType.Float);
            Raylib.SetShaderValue(CrtShader, DistortionLoc, 0.002f, ShaderUniformDataType.Float);
            Raylib.SetShaderValue(CrtShader, FlickerLoc, 0.01f, ShaderUniformDataType.Float);
        }

        public void Dispose()
        {
            Raylib.UnloadShader(CrtShader);
            Raylib.UnloadRenderTexture(GameTexture);
            
            Raylib.UnloadTexture(CharsetTexture);
            Raylib.UnloadFont(MenuFont);
            Raylib.CloseWindow();
        }
    }
    
    public IRayConnection Connect()
    {
        // Disable the default ESC key behavior that closes the window
        Raylib.SetExitKey(KeyboardKey.Null);
        
        var windowWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        var windowHeight = ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale;

        Raylib.InitWindow(windowWidth, windowHeight, ScreenConstants.Title);
        
        Raylib.SetTargetFPS(ScreenConstants.TargetFps);
        
        return new RayConnection(rayLoader);
    }
}