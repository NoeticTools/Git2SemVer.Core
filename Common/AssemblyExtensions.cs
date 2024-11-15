using System;
using System.Reflection;


namespace NoeticTools.Common;

public static class AssemblyExtensions
{
    public static string GetResourceFileContent(this Assembly assembly, string filename)
    {
        var resourcePath = assembly.GetManifestResourceNames()
                                   .SingleOrDefault(str => str.EndsWith(filename));
        if (resourcePath == null)
        {
            throw new Exception($"Resource file {filename} not found.");
        }

        try
        {
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