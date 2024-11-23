namespace NoeticTools.Git2SemVer.Core.Exceptions;

public class Git2SemVerArgumentException : Git2SemverExceptionBase
{
    public Git2SemVerArgumentException(string message) : base(message)
    {
    }

    // ReSharper disable once UnusedMember.Global
    public Git2SemVerArgumentException(string message, Exception innerException) : base(message, innerException)
    {
    }
}