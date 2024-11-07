using System.Reflection;


namespace NoeticTools.Common;

public static class AssemblyExtensions
{
    public static string GetResourceFileContent(this Assembly assembly, string filename)
    {
        try
        {
            var resourcePath = assembly.GetManifestResourceNames()
                                       .Single(str => str.EndsWith(filename));
            using var stream = assembly.GetManifestResourceStream(resourcePath);
            using var reader = new StreamReader(stream!);
            return reader.ReadToEnd();
        }
        catch (Exception exception)
        {
            throw new Exception($"Unable to get resource file {filename}.", exception);
        }
    }

    public static void WriteResourceFile(this Assembly assembly, string resourceFilename, string destinationPath)
    {
        var content = assembly.GetResourceFileContent(resourceFilename);
        File.WriteAllText(destinationPath, content);
    }
}