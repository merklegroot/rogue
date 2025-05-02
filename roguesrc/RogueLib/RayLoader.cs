using Raylib_cs;
using RogueLib.Utils;

namespace RogueLib;

public interface IRayLoader
{
    Font LoadRobotoFont();
    Texture2D LoadCharsetTexture();
    Shader LoadCrtShader();
    Texture2D LoadSkullImage();
    Texture2D LoadSwordImage();
    Texture2D LoadSmileyImage();
    Texture2D LoadSmileyBorderImage();
    Texture2D LoadSmileyNeutralImage();
    Texture2D LoadSmileyDeterminedImage();
    List<string> LoadSarcasticRemarks();
    List<string> LoadMap();

    List<List<string>> LoadMinotaur();
}

public class RayLoader : IRayLoader
{
    private readonly IResourceReader _resourceReader;
    
    public RayLoader(IResourceReader resourceReader) =>
        _resourceReader = resourceReader;

    public List<string> LoadMap()
    {
        var mapText = _resourceReader.ReadResourceString("map.txt", typeof(RayLoader).Assembly);
        var lines = mapText.SplitLines()
            .Where(line => !string.IsNullOrEmpty(line))
            .ToList();

        return lines;
    }

    public Font LoadRobotoFont() =>
        LoadFontFromEmbeddedResource("Roboto-Regular.ttf");

    public Texture2D LoadCharsetTexture() =>
        LoadTextureFromEmbeddedResource("Codepage-437-transparent.png");

    public Texture2D LoadSkullImage() =>
        LoadTextureFromEmbeddedResource("skull.png");

    public Texture2D LoadSwordImage() =>
        LoadTextureFromEmbeddedResource("sword.png");

    public Texture2D LoadSmileyImage() =>
        LoadTextureFromEmbeddedResource("smiley.png");

    public Texture2D LoadSmileyBorderImage() =>
        LoadTextureFromEmbeddedResource("smiley-border.png");

    public Texture2D LoadSmileyNeutralImage() =>
        LoadTextureFromEmbeddedResource("smiley-neutral.png");

    public Texture2D LoadSmileyDeterminedImage() =>
        LoadTextureFromEmbeddedResource("smiley-determined.png");

    public Shader LoadCrtShader() =>
        LoadShaderFromEmbeddedResource("crt.fs");

    public List<string> LoadSarcasticRemarks() =>
        LoadStringFromEmbeddedResource("sarcastic-remarks.txt")
        .SplitLines()
        .Where(queryLine => !string.IsNullOrEmpty(queryLine))
        .Select(queryLine => queryLine.Trim())
        .ToList();
    
    public List<List<string>> LoadMinotaur()
    {
        var minotaurText = _resourceReader.ReadResourceString("minotaur.txt", typeof(RayLoader).Assembly);
        // Frames are separated by # Frame
        var frames = minotaurText.Split("# Frame");

        // The frame starts on the first non-null/whitespace line
        // and ends before the last null/whitespace line or the end of the group.


        var minotaur = new List<List<string>>();
        foreach (var frame in frames)
        {
            // Find the first non-empty line
            var allLines = frame.SplitLines();
            var startIndex = -1;
            var endIndex = -1;
            
            // Find the first non-empty line
            for (int i = 0; i < allLines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(allLines[i]))
                {
                    startIndex = i;
                    break;
                }
            }
            
            // Find the last non-empty line
            for (int i = allLines.Length - 1; i >= 0; i--)
            {
                if (!string.IsNullOrWhiteSpace(allLines[i]))
                {
                    endIndex = i;
                    break;
                }
            }
            
            // If we found valid content
            if (startIndex >= 0 && endIndex >= 0)
            {
                var lines = allLines
                    .Skip(startIndex)
                    .Take(endIndex - startIndex + 1)
                    .ToList();
                
                minotaur.Add(lines);
            }
        }

        return minotaur;
    }

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

    private string LoadStringFromEmbeddedResource(string resourceName) =>
        _resourceReader.ReadResourceString(resourceName, typeof(RayLoader).Assembly);
}
