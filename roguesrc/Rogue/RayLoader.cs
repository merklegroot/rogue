using Raylib_cs;

namespace Rogue;

public interface IRayLoader
{
    Font LoadFontFromEmbeddedResource(string resourceName);
}

public class RayLoader : IRayLoader
{
    private readonly IResourceReader _resourceReader;
    
    public RayLoader(IResourceReader resourceReader) =>
        _resourceReader = resourceReader;
    
    public Font LoadFontFromEmbeddedResource(string resourceName)
    {
        // Get the current assembly
        var assembly = typeof(RayLoader).Assembly;
        
        var fontData = _resourceReader.ReadResourceBytes(resourceName, assembly);
            
        // Create a temporary file to load the font
        var tempFile = $"{Guid.NewGuid()}.ttf";
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