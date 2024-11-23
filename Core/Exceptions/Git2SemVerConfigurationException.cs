namespace NoeticTools.Git2SemVer.Core.Exceptions;

public class Git2SemVerConfigurationException : Git2SemverExceptionBase
{
    public Git2SemVerConfigurationException(string message) : base(message)
    {
    }

    public Git2SemVerConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}