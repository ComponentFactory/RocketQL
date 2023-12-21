namespace RocketQL.Core.Nodes;

public class SchemaLocation
{
    public required Location Location { get; init; }
}

public class SchemaDefinition : SchemaLocation
{
    [SetsRequiredMembers]
    public SchemaDefinition()
    {
        Description = string.Empty;
        Directives = [];
        Query = null;
        Mutation = null;
        Subscription = null;
        Location = new Location();
    }

    public string Description { get; set; }
    public Directives Directives { get; set; }
    public OperationTypeDefinition? Query { get; set; }
    public OperationTypeDefinition? Mutation { get; set; }
    public OperationTypeDefinition? Subscription { get; set; }

    public bool IsDefault
    {
        get
        {
            return (Description == string.Empty) &&
                   (Directives.Count == 0) &&
                   (Query is null) &&
                   (Mutation is null) &&
                   (Subscription is null) &&
                   (Location == new Location());
        }
    }
}

public class OperationTypeDefinition : SchemaLocation
{
    public required OperationType Operation { get; init; }
    public required string NamedType { get; init; }
    public required TypeDefinition? Definition { get; set; }
}

public class OperationTypeDefinitions : Dictionary<OperationType, OperationTypeDefinition> { };

public class DirectiveDefinition : SchemaLocation
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required InputValueDefinitions Arguments { get; init; }
    public required bool Repeatable { get; init; }
    public required DirectiveLocations DirectiveLocations { get; init; }
}

public class DirectiveDefinitions : Dictionary<string, DirectiveDefinition> { };

public class Directives : Dictionary<string, Directive> { };

public class Directive : SchemaLocation
{
    public required string Name { get; init; }
    public required DirectiveDefinition? Definition { get; set; }
    public required ObjectFields Arguments { get; init; }
}

public abstract class TypeDefinition : SchemaLocation
{
    public abstract bool IsInputType { get; }
    public abstract bool IsOutputType { get; }
}

public class TypeDefinitions : Dictionary<string, TypeDefinition> { };

public class ScalarTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }

    public override bool IsInputType => true;
    public override bool IsOutputType => true;
}

public class ObjectTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Interfaces ImplementsInterfaces { get; init; }
    public required Directives Directives { get; init; }
    public required FieldDefinitions Fields { get; init; }

    public override bool IsInputType => false;
    public override bool IsOutputType => true;
}

public class InterfaceTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Interfaces ImplementsInterfaces { get; init; }
    public required Directives Directives { get; init; }
    public required FieldDefinitions Fields { get; init; }

    public override bool IsInputType => false;
    public override bool IsOutputType => true;
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

public class UnionTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }
    public required MemberTypes MemberTypes { get; init; }

    public override bool IsInputType => false;
    public override bool IsOutputType => true;
}

public class MemberTypes : Dictionary<string, MemberType> { };

public class MemberType
{
    public required string Name { get; init; }
    public required ObjectTypeDefinition? Definition { get; set; }
}

public class EnumTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }
    public required EnumValueDefinitions EnumValues { get; init; }

    public override bool IsInputType => true;
    public override bool IsOutputType => true;
}

public class EnumValueDefinitions : Dictionary<string, EnumValueDefinition> { };

public class EnumValueDefinition : SchemaLocation
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }
}

public class InputObjectTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }
    public required InputValueDefinitions InputFields { get; init; }

    public override bool IsInputType => true;
    public override bool IsOutputType => false;
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

    public abstract bool IsInputType { get; }
    public abstract bool IsOutputType { get; }
}

public class TypeName : TypeLocation
{
    public required string Name { get; init; }
    public required TypeDefinition? Definition { get; set; }

    public override bool IsInputType => Definition!.IsInputType;
    public override bool IsOutputType => Definition!.IsOutputType;
}

public class TypeList : TypeLocation
{
    public required TypeLocation Type { get; init; }

    public override bool IsInputType => Type.IsInputType;
    public override bool IsOutputType => Type.IsOutputType;
}