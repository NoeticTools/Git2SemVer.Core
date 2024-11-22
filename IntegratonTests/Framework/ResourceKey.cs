using NUnit.Framework.Internal;


namespace NoeticTools.Git2SemVer.Core.IntegrationTests.Framework;

public class ResourceKey : IEquatable<ResourceKey>
{
    private static int _nextId = 1;

    public ResourceKey()
    {
        Id = _nextId++;
        Context = TestExecutionContext.CurrentContext;
    }

    public TestExecutionContext Context { get; }

    public int Id { get; }

    public bool Equals(ResourceKey? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ResourceKey)obj);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}