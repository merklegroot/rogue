using Raylib_cs;

namespace Rogue;

public interface IRayLoader
{
    Font LoadRobotoFont();
    Texture2D LoadCharsetTexture();
}

public class RayLoader : IRayLoader
{
    private readonly IResourceReader _resourceReader;
    
    public RayLoader(IResourceReader resourceReader) =>
        _resourceReader = resourceReader;

    public Font LoadRobotoFont() =>
        LoadFontFromEmbeddedResource("Roboto-Regular.ttf");

    public Texture2D LoadCharsetTexture() =>
        // Raylib.LoadTexture("images/Codepage-437-transparent.png");
        LoadTextureFromEmbeddedResource("Codepage-437-transparent.png");

    private Texture2D LoadTextureFromEmbeddedResource(string resourceName)
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
            return Raylib.LoadTexture(tempFile);
        }
        finally
        {
            // Delete the temporary file
            try { File.Delete(tempFile); }
            catch { /* Ignore errors */ }
        }
    }
    
    private Font LoadFontFromEmbeddedResource(string resourceName)
    {
        var extension = Path.GetExtension(resourceName);
        
        var assembly = typeof(RayLoader).Assembly;
        var fontData = _resourceReader.ReadResourceBytes(resourceName, assembly);
            
        // Create a temporary file to load the font
        var tempFile = $"{Guid.NewGuid()}{extension}";
        try
        {
            File.WriteAllBytes(tempFile, fontData);

            // Load the font using Raylib
            return Raylib.LoadFont(tempFile);
        }
        finally
        {
            // Delete the temporary file
            try { File.Delete(tempFile); }
            catch { /* Ignore errors */ }
        }
    }
}