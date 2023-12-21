namespace RocketQL.Core.Exceptions;

public class RocketException(Location location, string message) : Exception(message)
{
    public Location[] Locations { get; init; } = [location];
}
