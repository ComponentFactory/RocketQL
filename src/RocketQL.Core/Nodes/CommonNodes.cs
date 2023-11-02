namespace RocketQL.Core.Nodes;

public record struct Location(int Position, int Line, int Column, string Source);

public abstract record class LocationNode(Location Location);
