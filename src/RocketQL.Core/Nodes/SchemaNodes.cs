namespace RocketQL.Core.Nodes;

public class DirectiveDefinitions : Dictionary<string, DirectiveDefinition> { };
public class ScalarTypeDefinitions : Dictionary<string, ScalarTypeDefinition> { };
public class InterfaceTypeDefinitions : Dictionary<string, InterfaceTypeDefinition> { };
public class EnumTypeDefinitions : Dictionary<string, EnumTypeDefinition> { };

public class SchemaLocation
{ 
    public required Location Location { get; init; } 
}

public class DirectiveDefinition : SchemaLocation
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required InputValueDefinitions Arguments { get; init; }
    public required bool Repeatable { get; init; }
    public required DirectiveLocations DirectiveLocations { get; init; }
}

public class Directives : Dictionary<string, Directive> { };

public class Directive : SchemaLocation
{
    public required string Name { get; init; }
    public required DirectiveDefinition? Definition { get; set; }
    public required ObjectFields Arguments { get; init; }
}

public class TypeDefinition : SchemaLocation
{ 
}

public class ScalarTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }
}

public class InterfaceTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Interfaces ImplementsInterfaces { get; init; }
    public required Directives Directives { get; init; }
    public required FieldDefinitions Fields { get; init; }
}

public class Interfaces : Dictionary<string, Interface> { };

public class Interface
{
    public required string Name { get; init; }
    public required InterfaceTypeDefinition? Definition { get; set; }
}

public class FieldDefinitions : Dictionary<string, FieldDefinition> { };

public class FieldDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required InputValueDefinitions Arguments { get; init; }
    public required TypeLocation Type { get; init; }
    public required TypeDefinition? Definition { get; set; }
}

public class EnumTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }
    public required EnumValueDefinitions EnumValues { get; init; }
}

public class EnumValueDefinitions : Dictionary<string, EnumValueDefinition> { };

public class EnumValueDefinition : SchemaLocation
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }
}

public class ObjectFields : Dictionary<string, ObjectFieldNode> { };

public class InputValueDefinitions : Dictionary<string, InputValueDefinition> { };

public class InputValueDefinition : SchemaLocation
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required TypeLocation Type { get; init; }
    public required ValueNode? DefaultValue { get; init; }
    public required Directives Directives { get; init; }
}

public abstract class TypeLocation : SchemaLocation
{
    public required bool NonNull { get; init; }
}

public class TypeName : TypeLocation
{
    public required string Name { get; init; }
}

public class TypeList : TypeLocation
{
    public required TypeLocation Type { get; init; }
    public required TypeDefinition? Definition { get; set; }
}