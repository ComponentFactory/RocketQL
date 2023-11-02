namespace RocketQL.Core.Nodes;

public class DirectiveDefinitions : Dictionary<string, DirectiveDefinition> { };
public class ScalarTypeDefinitions : Dictionary<string, ScalarTypeDefinition> { };
public class ObjectFieldNodes : Dictionary<string, ObjectFieldNode> { };

public class SchemaLocation
{ 
    public required Location Location { get; init; } 
}

public class DirectiveDefinition : SchemaLocation
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required InputValueDefinitionNodes Arguments { get; init; }
    public required bool Repeatable { get; init; }
    public required DirectiveLocations DirectiveLocations { get; init; }
}

public class DirectiveNodes : Dictionary<string, DirectiveNode> { };

public class DirectiveNode : SchemaLocation
{
    public required string Name { get; init; }
    public required DirectiveDefinition? Definition { get; set; }
    public required ObjectFieldNodes Arguments { get; init; }
}

public class TypeDefinition : SchemaLocation
{ 
}

public class ScalarTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required DirectiveNodes Directives { get; init; }
}

public class InputValueDefinitionNodes : Dictionary<string, InputValueDefinitionNode> { };

public class InputValueDefinitionNode : SchemaLocation
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required TypeNode Type { get; init; }
    public required ValueNode? DefaultValue { get; init; }
    public required DirectiveNodes Directives { get; init; }
}

public abstract class TypeNode : SchemaLocation
{
    public required bool NonNull { get; init; }
}

public class TypeNameNode : TypeNode
{
    public required string Name { get; init; }
}

public class TypeListNode : TypeNode
{
    public required TypeNode Type { get; init; }
    public required TypeDefinition? Definition { get; set; }
}