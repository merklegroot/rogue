namespace RogueLib.Utils;

public static class StringUtils
{
    public static string[] SplitLines(string source) =>
        source.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
}