﻿namespace RocketQL.Core.Nodes;

public abstract record class DocumentNode : LocationNode
{
    public DocumentNode()
    {
    }

    public DocumentNode(Location location)
        : base(location)
    {
    }

    public DocumentNode? Parent { get; set; }
}

public record class OperationDefinition(OperationType Operation, string Name, Directives Directives, VariableDefinitions Variables, SelectionSet SelectionSet, Location Location) : DocumentNode(Location);

public record class FragmentDefinition(string Name, string TypeCondition, Directives Directives, SelectionSet SelectionSet, Location Location) : DocumentNode(Location)
{
    public TypeDefinition? Definition { get; set; }
}

public record class SchemaRoot(string Description, Directives Directives, OperationTypeDefinition? Query, OperationTypeDefinition? Mutation, OperationTypeDefinition? Subscription, Location Location) : DocumentNode(Location)
{
    public static readonly SchemaRoot Empty = new("", [], null, null, null, Location.Empty);
}

public record class SchemaDefinition(string Description, Directives Directives, OperationTypeDefinitions Operations, Location Location) : DocumentNode(Location);

public record class DirectiveDefinition(string Description, string Name, InputValueDefinitions Arguments, bool Repeatable, DirectiveLocations DirectiveLocations, Location Location) : DocumentNode(Location)
{
    public bool IsBuiltIn { get; set; }
    public bool IsRooted { get; set; }
    public Directives References { get; init; } = [];
}

public abstract record class TypeDefinition(string Description, string Name, Directives Directives, Location Location, bool IsInputType, bool IsOutputType) : DocumentNode(Location)
{
    public bool IsBuiltIn { get; set; }
    public bool IsRooted { get; set; }
    public DocumentNodes References { get; init; } = [];
}

public record class ScalarTypeDefinition(string Description, string Name, Directives Directives, Location Location) : TypeDefinition(Description, Name, Directives, Location, true, true);
public record class ObjectTypeDefinition(string Description, string Name, Directives Directives, Interfaces ImplementsInterfaces, FieldDefinitions Fields, Location Location) : TypeDefinition(Description, Name, Directives, Location, false, true);
public record class InterfaceTypeDefinition(string Description, string Name, Directives Directives, Interfaces ImplementsInterfaces, FieldDefinitions Fields, Location Location) : TypeDefinition(Description, Name, Directives, Location, false, true);
public record class UnionTypeDefinition(string Description, string Name, Directives Directives, MemberTypes MemberTypes, Location Location) : TypeDefinition(Description, Name, Directives, Location, false, true);
public record class EnumTypeDefinition(string Description, string Name, Directives Directives, EnumValueDefinitions EnumValues, Location Location) : TypeDefinition(Description, Name, Directives, Location, true, true);
public record class InputObjectTypeDefinition(string Description, string Name, Directives Directives, InputValueDefinitions InputFields, Location Location) : TypeDefinition(Description, Name, Directives, Location, true, false);
public record class VariableDefinition(string Name, TypeNode Type, ValueNode? DefaultValue, Directives Directives, Location Location) : DocumentNode(Location);

public abstract record class SelectionNode(Location Location) : DocumentNode(Location);

public record class SelectionField(string Alias, string Name, Directives Directives, ObjectFields Arguments, SelectionSet SelectionSet, Location Location) : SelectionNode(Location);
public record class SelectionFragmentSpread(string Name, Directives Directives, Location Location) : SelectionNode(Location)
{
    public FragmentDefinition? Definition { get; set; }
}

public record class SelectionInlineFragment(string TypeCondition, Directives Directives, SelectionSet SelectionSet, Location Location) : SelectionNode(Location)
{
    public TypeDefinition? Definition { get; set; }
}

public record class OperationTypeDefinition(OperationType Operation, string NamedType, Location Location) : DocumentNode(Location)
{
    public ObjectTypeDefinition? Definition { get; set; }
}

public record class FieldDefinition(string Description, string Name, InputValueDefinitions Arguments, TypeNode Type, Directives Directives, Location Location) : DocumentNode(Location);
public record class EnumValueDefinition(string Description, string Name, Directives Directives, Location Location) : DocumentNode(Location);
public record class InputValueDefinition(string Description, string Name, TypeNode Type, ValueNode? DefaultValue, Directives Directives, InputValueUsage Usage, Location Location) : DocumentNode(Location);

public record class Directive(string Name, ObjectFields Arguments, Location Location) : DocumentNode(Location)
{
    public DirectiveDefinition? Definition { get; set; }
}

public record class Interface(string Name, Location Location) : DocumentNode(Location)
{
    public InterfaceTypeDefinition? Definition { get; set; }
}

public record class MemberType(string Name, Location Location) : DocumentNode(Location)
{
    public ObjectTypeDefinition? Definition { get; set; }
}

public abstract record class TypeNode(Location Location) : DocumentNode(Location)
{
    public abstract TypeDefinition? Definition { get; set; }
    public abstract bool IsInputType { get; }
    public abstract bool IsOutputType { get; }
}

public record class TypeName(string Name, Location Location) : TypeNode(Location)
{
    public override TypeDefinition? Definition { get; set; }
    public override bool IsInputType => Definition!.IsInputType;
    public override bool IsOutputType => Definition!.IsOutputType;
}

public record class TypeList(TypeNode Type, Location Location) : TypeNode(Location)
{
    public override TypeDefinition? Definition { get => Type.Definition; set => Type.Definition = value; }
    public override bool IsInputType => Type.IsInputType;
    public override bool IsOutputType => Type.IsOutputType;
}

public record class TypeNonNull(TypeNode Type, Location Location) : TypeNode(Location)
{
    public override TypeDefinition? Definition { get => Type.Definition; set => Type.Definition = value; }
    public override bool IsInputType => Type.IsInputType;
    public override bool IsOutputType => Type.IsOutputType;
}

public class DocumentNodes : List<DocumentNode> { };

public class OperationDefinitions : Dictionary<string, OperationDefinition>
{
    public static readonly OperationDefinitions Empty = [];
};
public class FragmentDefinitions : Dictionary<string, FragmentDefinition>
{
    public static readonly FragmentDefinitions Empty = [];
};

public class DirectiveDefinitions : Dictionary<string, DirectiveDefinition>
{
    public static readonly DirectiveDefinitions Empty = [];
};

public class TypeDefinitions : Dictionary<string, TypeDefinition>
{
    public static readonly TypeDefinitions Empty = [];
};

public class VariableDefinitions : Dictionary<string, VariableDefinition> { };
public class SelectionSet : List<SelectionNode> { };
public class SchemaDefinitions : List<SchemaDefinition> { };
public class InterfaceTypeDefinitions : Dictionary<string, InterfaceTypeDefinition> { };
public class InputObjectTypeDefinitions : Dictionary<string, InputObjectTypeDefinition> { };
public class Directives : List<Directive> { };
public class Interfaces : Dictionary<string, Interface> { };
public class OperationTypeDefinitions : Dictionary<OperationType, OperationTypeDefinition> { };
public class FieldDefinitions : Dictionary<string, FieldDefinition> { };
public class MemberTypes : Dictionary<string, MemberType> { };
public class EnumValueDefinitions : Dictionary<string, EnumValueDefinition> { };
public class ObjectFields : Dictionary<string, ObjectFieldNode> { };
public class InputValueDefinitions : Dictionary<string, InputValueDefinition> { };

public interface IReadOnlyDirectives : IReadOnlyDictionary<string, DirectiveDefinition> { };
public interface IReadOnlyTypes : IReadOnlyDictionary<string, TypeDefinition> { };
