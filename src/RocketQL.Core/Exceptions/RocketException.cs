using RocketQL.Core.Nodes;
using System.Collections.ObjectModel;

namespace RocketQL.Core.Exceptions;

public class RocketException : Exception
{
    public RocketException()
    {
        Location = Location.Empty;
    }

    public RocketException(Location location, string message)
        : base(message)
    {
        Location = location;
    }

    public Location Location { get; init; }
}

public class RocketExceptions(IEnumerable<RocketException> innerExceptions) : Exception("Aggregate RocketExceptions")
{
    private readonly List<RocketException> _innerExceptions = new(innerExceptions);
    private ReadOnlyCollection<RocketException>? _readOnlyExceptions;

    public RocketExceptions(RocketException innerException)
        : this([innerException])
    {
    }

    public ReadOnlyCollection<RocketException> InnerExceptions => _readOnlyExceptions ??= new ReadOnlyCollection<RocketException>(_innerExceptions);
}
