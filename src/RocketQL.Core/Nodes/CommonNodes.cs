using System.Diagnostics.CodeAnalysis;

namespace RocketQL.Core.Nodes;

public class Location
{
    public static readonly Location Empty = new();

    [SetsRequiredMembers]
    protected Location()
    {
        Position = 0;
        Line = 0;
        Column = 0;
        Source = "";
    }

    [SetsRequiredMembers]
    public Location(int position, int line, int column, string source) 
    { 
        Position = position;
        Line = line;
        Column = column;
        Source = source;
    }

    public required int Position { get; init; }
    public required int Line { get; init; }
    public required int Column { get; init; }
    public required string Source { get; init; } = "";
}

public abstract record class LocationNode
{
    public LocationNode()
    {
        Location = Location.Empty;
    }

    public LocationNode(Location location)
    {
        Location = location;
    }

    public Location Location { get; init; } 
}
