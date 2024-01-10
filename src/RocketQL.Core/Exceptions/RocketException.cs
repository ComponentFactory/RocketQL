using RocketQL.Core.Nodes;
using System.Collections.ObjectModel;

namespace RocketQL.Core.Exceptions;

public class RocketException : Exception
{
    public RocketException()
    {
        Locations = [];
    }

    public RocketException(Location location, string message)
        : base(message)
    {
        Locations = [location];
    }

    public Location[] Locations { get; init; }
}

public class RocketExceptions : Exception
{
    private readonly List<RocketException> _innerExceptions;
    private ReadOnlyCollection<RocketException>? _readOnlyExceptions;

    public RocketExceptions(RocketException innerException)
        : this([innerException])
    {
    }

    public RocketExceptions(IEnumerable<RocketException> innerExceptions)
        : base("Aggregate RocketExceptions")
    {
        _innerExceptions = new(innerExceptions);
    }

    public ReadOnlyCollection<RocketException> InnerExceptions => _readOnlyExceptions ??= new ReadOnlyCollection<RocketException>(_innerExceptions);
}
