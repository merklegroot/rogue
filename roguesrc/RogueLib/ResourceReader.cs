using System.Reflection;

namespace Rogue;

public interface IResourceReader
{
    byte[] ReadResourceBytes(string resourceName, Assembly assembly);
}

public class ResourceReader : IResourceReader
{
    public byte[] ReadResourceBytes(string resourceName, Assembly assembly)
    {
        var matchingResourceName = FindResource(resourceName, assembly);
        if (string.IsNullOrWhiteSpace(matchingResourceName))
            throw new ApplicationException($"Embedded resource not found: {resourceName}");
        
        // Open a stream to the embedded resource
        using var stream = assembly.GetManifestResourceStream(matchingResourceName);
        if (stream == null)
            throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");
        
        // Read the font data into a byte array
        var data = new byte[stream.Length];
        _ = stream.Read(data, 0, data.Length);
        
        return data;
    }
    
    private string? FindResource(string partialName, Assembly assembly)
    {
        var names = assembly.GetManifestResourceNames();
        
        var prioritizedQueries = new List<Func<string?>>
        {
            () => names.FirstOrDefault(resourceName => string.Equals(partialName, resourceName, StringComparison.Ordinal)),
            () => names.FirstOrDefault(resourceName => string.Equals(partialName, resourceName, StringComparison.OrdinalIgnoreCase)),
            () => names.FirstOrDefault(resourceName => resourceName.EndsWith(partialName, StringComparison.Ordinal)),
            () => names.FirstOrDefault(resourceName => resourceName.EndsWith(partialName, StringComparison.OrdinalIgnoreCase)),
            () => names.FirstOrDefault(resourceName => resourceName.StartsWith(partialName, StringComparison.Ordinal)),
            () => names.FirstOrDefault(resourceName => resourceName.StartsWith(partialName, StringComparison.OrdinalIgnoreCase)),
            () => names.FirstOrDefault(resourceName => resourceName.Contains(partialName, StringComparison.Ordinal)),
            () => names.FirstOrDefault(resourceName => resourceName.Contains(partialName, StringComparison.OrdinalIgnoreCase)),
        };

        return prioritizedQueries.Select(query => query()).FirstOrDefault(result => !string.IsNullOrWhiteSpace(result));
    }
}