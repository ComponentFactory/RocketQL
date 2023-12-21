namespace RocketQL.Core.Exceptions;

public class ValidationException(Location location, string message) : RocketException(location, message)
{
    public static ValidationException UnrecognizedType(Location location, string name) => new(location, $"Unrecognized type '{name}' encountered.");
    public static ValidationException UnrecognizedType(SyntaxNode node) => new(node.Location, $"Unrecognized type '{node.GetType()}' encountered.");
    public static ValidationException SchemaDefinitionAlreadyDefined(Location location) => new(location, $"Schema definition is already defined.");
    public static ValidationException SchemaDefinitionEmpty(Location location) => new(location, "Schema definition does not define any operations.");
    public static ValidationException SchemaOperationAlreadyDefined(Location location, OperationType operation) => new(location, $"Schema definition already defines the {operation} operation.");
    public static ValidationException SchemaDefinitionMissingQuery(Location location) => new(location, $"Schema definition missing mandatory Query operation.");
    public static ValidationException SchemaOperationsNotUnique(Location location, string op1, string op2, string type) => new(location, $"Schema {op1} and {op2} operations cannot have same '{type}' type.");
    public static ValidationException TypeNotDefinedForSchemaOperation(Location location, OperationType operation, string type) => new(location, $"Type '{type}' not defined for the schema operation {operation}.");
    public static ValidationException OperationTypeAlreadyDefined(Location location, OperationType operationType) => new(location, $"Type already defined for '{operationType}' operation.");
    public static ValidationException DirectiveNameAlreadyDefined(Location location, string name) => new(location, $"Directive name '{name}' is already defined.");
    public static ValidationException ScalarNameAlreadyDefined(Location location, string name) => new(location, $"Scalar type name '{name}' is already defined.");
    public static ValidationException ObjectNameAlreadyDefined(Location location, string name) => new(location, $"Object type name '{name}' is already defined.");
    public static ValidationException InterfaceNameAlreadyDefined(Location location, string name) => new(location, $"Interface type name '{name}' is already defined.");
    public static ValidationException UnionNameAlreadyDefined(Location location, string name) => new(location, $"Union type name '{name}' is already defined.");
    public static ValidationException EnumNameAlreadyDefined(Location location, string name) => new(location, $"Enum type name '{name}' is already defined.");
    public static ValidationException InputObjectNameAlreadyDefined(Location location, string name) => new(location, $"Input object type name '{name}' is already defined.");
}
