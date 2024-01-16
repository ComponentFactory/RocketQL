namespace RocketQL.Core.Nodes;

public abstract class DocumentNode
{
    public required Location Location { get; init; }
    public DocumentNode? Parent { get; set; }
    public abstract string OutputElement { get; }
    public abstract string OutputName { get; }
}

public class OperationDefinition : DocumentNode
{    
    public required OperationType Operation { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; set; }
    public required VariableDefinitions Variables { get; set; }
    public required SelectionSet SelectionSet { get; set; }
    public override string OutputElement => "Operation";
    public override string OutputName => string.IsNullOrEmpty(Name) ? Operation.ToString() : Name;
}

public class FragmentDefinition : DocumentNode
{
    public required string Name { get; init; }
    public required string TypeCondition { get; init; }
    public required TypeDefinition? Definition { get; set; }
    public required Directives Directives { get; set; }
    public required SelectionSet SelectionSet { get; set; }
    public override string OutputElement => "Fragment";
    public override string OutputName => Name;
}

public class SchemaRoot : DocumentNode
{
    public static readonly SchemaRoot Empty = new()
    { 
        Description = "",
        Directives = [],
        Query = null,
        Mutation = null,
        Subscription = null,
        Location = new()
    };
    
    public required string Description { get; set; }
    public required Directives Directives { get; set; }
    public required OperationTypeDefinition? Query { get; set; }
    public required OperationTypeDefinition? Mutation { get; set; }
    public required OperationTypeDefinition? Subscription { get; set; }
    public override string OutputElement => "Schema";
    public override string OutputName => "";
}

public class SchemaDefinition : DocumentNode
{
    public required string Description { get; set; }
    public required Directives Directives { get; set; }
    public required OperationTypeDefinitions Operations { get; set; }
    public override string OutputElement => "Schema";
    public override string OutputName => "";
}

public class DirectiveDefinition : DocumentNode
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required InputValueDefinitions Arguments { get; init; }
    public required bool Repeatable { get; init; }
    public required DirectiveLocations DirectiveLocations { get; init; }
    public bool IsBuiltIn { get; init; }
    public bool IsRooted { get; set; }
    public Directives References { get; init; } = [];
    public override string OutputElement => "Directive";
    public override string OutputName => Name;
}

public abstract class TypeDefinition : DocumentNode
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }
    public bool IsBuiltIn { get; init; }
    public bool IsRooted { get; set; }
    public DocumentNodes References { get; init; } = [];
    public override string OutputName => Name;
    public abstract bool IsInputType { get; }
    public abstract bool IsOutputType { get; }
}

public class ScalarTypeDefinition : TypeDefinition
{
    public override string OutputElement => "Scalar";
    public override bool IsInputType => true;
    public override bool IsOutputType => true;
}

public class ObjectTypeDefinition : TypeDefinition
{
    public required Interfaces ImplementsInterfaces { get; init; }
    public required FieldDefinitions Fields { get; init; }
    public override string OutputElement => "Object";
    public override bool IsInputType => false;
    public override bool IsOutputType => true;
}

public class InterfaceTypeDefinition : TypeDefinition
{
    public required Interfaces ImplementsInterfaces { get; init; }
    public required FieldDefinitions Fields { get; init; }
    public override string OutputElement => "Interface";
    public override bool IsInputType => false;
    public override bool IsOutputType => true;
}

public class UnionTypeDefinition : TypeDefinition
{
    public required MemberTypes MemberTypes { get; init; }
    public override string OutputElement => "Union";
    public override bool IsInputType => false;
    public override bool IsOutputType => true;
}

public class EnumTypeDefinition : TypeDefinition
{
    public required EnumValueDefinitions EnumValues { get; init; }
    public override string OutputElement => "Enum";
    public override bool IsInputType => true;
    public override bool IsOutputType => true;
}

public class InputObjectTypeDefinition : TypeDefinition
{
    public required InputValueDefinitions InputFields { get; init; }
    public override string OutputElement => "Input object";
    public override bool IsInputType => true;
    public override bool IsOutputType => false;
}

public class VariableDefinition : DocumentNode
{
    public required string Name { get; init; }
    public required TypeNode Type { get; init; }
    public required ValueNode? DefaultValue { get; init; }
    public required Directives Directives { get; set; }
    public override string OutputElement => "Variable";
    public override string OutputName => Name;
}

public abstract class SelectionNode : DocumentNode
{
}

public class SelectionField : SelectionNode
{
    public required string Alias { get; init; }
    public required string Name { get; init; }
    public required ObjectFields Arguments { get; init; }
    public required Directives Directives { get; set; }
    public required SelectionSet SelectionSet { get; set; }
    public override string OutputElement => "Field";
    public override string OutputName => Alias ?? Name;
}

public class SelectionFragmentSpread : SelectionNode
{
    public required string Name { get; init; }
    public required FragmentDefinition? Definition { get; set; }
    public required Directives Directives { get; set; }
    public override string OutputElement => "Fragment spread";
    public override string OutputName => Name;
}

public class SelectionInlineFragment : SelectionNode
{
    public required string TypeCondition { get; init; }
    public required TypeDefinition? Definition { get; set; }
    public required Directives Directives { get; set; }
    public required SelectionSet SelectionSet { get; set; }
    public override string OutputElement => "Inline fragment";
    public override string OutputName => TypeCondition;
}

public class OperationTypeDefinition : DocumentNode
{
    public required OperationType Operation { get; init; }
    public required string NamedType { get; init; }
    public required ObjectTypeDefinition? Definition { get; set; }
    public override string OutputElement => "Operation";
    public override string OutputName => Operation.ToString();
}

public class FieldDefinition : DocumentNode
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required InputValueDefinitions Arguments { get; init; }
    public required TypeNode Type { get; init; }
    public required Directives Directives { get; init; }
    public override string OutputElement => "Field";
    public override string OutputName => Name;
}

public class EnumValueDefinition : DocumentNode
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }
    public override string OutputElement => "Enum Value";
    public override string OutputName => Name;
}

public class InputValueDefinition : DocumentNode
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required TypeNode Type { get; init; }
    public required ValueNode? DefaultValue { get; init; }
    public required Directives Directives { get; init; }
    public required string ElementUsage { get; init; }
    public override string OutputElement => ElementUsage;
    public override string OutputName => Name;
}

public class Directive : DocumentNode
{
    public required string Name { get; init; }
    public required DirectiveDefinition? Definition { get; set; }
    public required ObjectFields Arguments { get; init; }
    public override string OutputElement => "Directive";
    public override string OutputName => Name;
}

public class Interface : DocumentNode
{
    public required string Name { get; init; }
    public required InterfaceTypeDefinition? Definition { get; set; }
    public override string OutputElement => "Interface";
    public override string OutputName => Name;
}

public class MemberType : DocumentNode
{
    public required string Name { get; init; }
    public required ObjectTypeDefinition? Definition { get; set; }
    public override string OutputElement => "Member Type";
    public override string OutputName => Name;
}

public abstract class TypeNode : DocumentNode
{
    public abstract TypeDefinition? Definition { get; set; }
    public abstract bool IsInputType { get; }
    public abstract bool IsOutputType { get; }
}

public class TypeName : TypeNode
{
    public required string Name { get; init; }
    public required override TypeDefinition? Definition { get; set; }
    public override string OutputElement => "TypeName";
    public override string OutputName => Name;
    public override bool IsInputType => Definition!.IsInputType;
    public override bool IsOutputType => Definition!.IsOutputType;
}

public class TypeList : TypeNode
{
    public required TypeNode Type { get; init; }
    public override TypeDefinition? Definition { get => Type.Definition; set => Type.Definition = value; }
    public override string OutputElement => "TypeList";
    public override string OutputName => ""; 
    public override bool IsInputType => Type.IsInputType;
    public override bool IsOutputType => Type.IsOutputType;
}

public class TypeNonNull : TypeNode
{
    public required TypeNode Type { get; init; }
    public override TypeDefinition? Definition { get => Type.Definition; set => Type.Definition = value; }
    public override string OutputElement => "TypeNonNull";
    public override string OutputName => "";
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
