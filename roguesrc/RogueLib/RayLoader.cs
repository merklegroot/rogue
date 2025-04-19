using Raylib_cs;

namespace RogueLib;

public interface IRayLoader
{
    Font LoadRobotoFont(IRayConnection connection);
    Texture2D LoadCharsetTexture(IRayConnection connection);
    Shader LoadCrtShader(IRayConnection connection);
}

public class RayLoader : IRayLoader
{
    private readonly IResourceReader _resourceReader;
    
    public RayLoader(IResourceReader resourceReader) =>
        _resourceReader = resourceReader;

    public Font LoadRobotoFont(IRayConnection connection) =>
        LoadFontFromEmbeddedResource("Roboto-Regular.ttf");

    public Texture2D LoadCharsetTexture(IRayConnection connection) =>
        LoadTextureFromEmbeddedResource("Codepage-437-transparent.png");

    public Shader LoadCrtShader(IRayConnection connection) =>
        LoadShaderFromEmbeddedResource("crt.fs");
    
    private Shader LoadShaderFromEmbeddedResource(string resourceName) =>
        LoadFromEmbeddedResource(resourceName, (fileName) => Raylib.LoadShader(null, fileName));
    
    private T LoadFromEmbeddedResource<T>(string resourceName, Func<string, T> load)
    {
        var extension = Path.GetExtension(resourceName);
        
        var assembly = typeof(RayLoader).Assembly;
        var fontData = _resourceReader.ReadResourceBytes(resourceName, assembly);

        // Create a temporary file to load the resource
        var tempFile = $"{Guid.NewGuid()}{extension}";
        try
        {
            File.WriteAllBytes(tempFile, fontData);

            // Load the font using Raylib
            return load(tempFile);
        }
        finally
        {
            // Delete the temporary file
            try { File.Delete(tempFile); }
            catch { /* Ignore errors */ }
        }
    }
    
    private Texture2D LoadTextureFromEmbeddedResource(string resourceName) =>
        LoadFromEmbeddedResource(resourceName, Raylib.LoadTexture);

    private Font LoadFontFromEmbeddedResource(string resourceName) =>
        LoadFromEmbeddedResource(resourceName, Raylib.LoadFont);
}