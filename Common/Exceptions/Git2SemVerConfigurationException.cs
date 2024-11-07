namespace NoeticTools.Common.Exceptions;

public class Git2SemVerConfigurationException : Git2SemverExceptionBase
{
    public Git2SemVerConfigurationException(string message) : base(message)
    {
    }

    public Git2SemVerConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}