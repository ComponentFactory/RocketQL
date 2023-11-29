namespace RocketQL.Core;

public class RocketException(Location location, string message) : Exception(message)
{
    public Location[] Locations { get; init; } = [location];
}


