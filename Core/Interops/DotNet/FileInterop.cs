using Injectio.Attributes;


namespace NoeticTools.Common.Interops.DotNet;

[RegisterSingleton]
public sealed class FileInterop : IFiles
{
    public bool Exists(string filePath)
    {
        return File.Exists(filePath);
    }
}