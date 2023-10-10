namespace RocketQL.Core;

public class ValidationException : Exception
{
    public ValidationException(Location location, string message)
        : base(message)
    {
        Locations = new Location[] { location };
    }

    public Location[] Locations { get; init; }

    public static ValidationException DirectiveAlreadyDefined(Location location, string name) => new(location, $"Directive '{name}' is already defined.");
}
