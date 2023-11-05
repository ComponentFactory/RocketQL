namespace RocketQL.Core;

public class ValidationException : Exception
{
    public ValidationException(Location location, string message)
        : base(message)
    {
        Locations = new Location[] { location };
    }

    public Location[] Locations { get; init; }

    public static ValidationException UnrecognizedType(Location location, string name) => new(location, $"Unrecognized type '{name}' encountered.");
    public static ValidationException DirectiveAlreadyDefined(Location location, string name) => new(location, $"Directive '{name}' is already defined.");
    public static ValidationException ScalarAlreadyDefined(Location location, string name) => new(location, $"Scalar type '{name}' is already defined.");
    public static ValidationException ObjectAlreadyDefined(Location location, string name) => new(location, $"Object type '{name}' is already defined.");
    public static ValidationException InterfaceAlreadyDefined(Location location, string name) => new(location, $"Interface type '{name}' is already defined.");
    public static ValidationException UnionAlreadyDefined(Location location, string name) => new(location, $"Union type '{name}' is already defined.");
    public static ValidationException EnumAlreadyDefined(Location location, string name) => new(location, $"Enum type '{name}' is already defined.");
    public static ValidationException InputObjectAlreadyDefined(Location location, string name) => new(location, $"Input object type '{name}' is already defined.");
}
