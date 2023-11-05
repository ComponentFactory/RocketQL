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
    public static ValidationException ScalarAlreadyDefined(Location location, string name) => new(location, $"Scalar '{name}' is already defined.");
    public static ValidationException InterfaceAlreadyDefined(Location location, string name) => new(location, $"Interface '{name}' is already defined.");
    public static ValidationException EnumAlreadyDefined(Location location, string name) => new(location, $"Enum '{name}' is already defined.");
    public static ValidationException UnrecognizedType(Location location, string name) => new(location, $"Unrecognized type '{name}' encountered.");
}
