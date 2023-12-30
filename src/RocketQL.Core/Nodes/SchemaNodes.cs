using System.Xml.Linq;

namespace RocketQL.Core.Nodes;

public class SchemaDefinitions : List<SchemaDefinition> { };
public class DirectiveDefinitions : Dictionary<string, DirectiveDefinition> { };
public class TypeDefinitions : Dictionary<string, TypeDefinition> { };
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

public abstract class SchemaNode
{
    public required Location Location { get; init; }
    public virtual string OutputElement => string.Empty;
    public virtual string OutputName => string.Empty;
}

public class SchemaDefinition : SchemaNode
{
    public required string Description { get; set; }
    public required Directives Directives { get; set; }
    public required OperationTypeDefinitions Operations { get; set; }
}

public class DirectiveDefinition : SchemaNode
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required InputValueDefinitions Arguments { get; init; }
    public required bool Repeatable { get; init; }
    public required DirectiveLocations DirectiveLocations { get; init; }
    public override string OutputElement => "Directive";
    public override string OutputName => Name;
}

public abstract class TypeDefinition : SchemaNode
{
    public abstract bool IsInputType { get; }
    public abstract bool IsOutputType { get; }
}


public class ScalarTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }
    public override string OutputElement => "Scalar";
    public override string OutputName => Name;
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
    public override string OutputElement => "Object";
    public override string OutputName => Name;
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
    public override string OutputElement => "Interface";
    public override string OutputName => Name;
    public override bool IsInputType => false;
    public override bool IsOutputType => true;
}

public class UnionTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }
    public required MemberTypes MemberTypes { get; init; }
    public override string OutputElement => "Union";
    public override string OutputName => Name;
    public override bool IsInputType => false;
    public override bool IsOutputType => true;
}

public class EnumTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }
    public required EnumValueDefinitions EnumValues { get; init; }
    public override string OutputElement => "Enum";
    public override string OutputName => Name;
    public override bool IsInputType => true;
    public override bool IsOutputType => true;
}

public class Directive : SchemaNode
{
    public required string Name { get; init; }
    public required DirectiveDefinition? Definition { get; set; }
    public required ObjectFields Arguments { get; init; }
    public override string OutputElement => "Directive";
    public override string OutputName => Name;
}

public class Interface : SchemaNode
{
    public required string Name { get; init; }
    public required InterfaceTypeDefinition? Definition { get; set; }
    public override string OutputElement => "Interface";
    public override string OutputName => Name;
}

public class OperationTypeDefinition : SchemaNode
{
    public required OperationType Operation { get; init; }
    public required string NamedType { get; init; }
    public required TypeDefinition? Definition { get; set; }
}

public class FieldDefinition : SchemaNode
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required InputValueDefinitions Arguments { get; init; }
    public required TypeNode Type { get; init; }
    public required Directives Directives { get; init; }
    public override string OutputElement => "Field";
    public override string OutputName => Name;
}

public class MemberType : SchemaNode
{
    public required string Name { get; init; }
    public required ObjectTypeDefinition? Definition { get; set; }
    public override string OutputElement => "Member Type";
    public override string OutputName => Name;
}

public class EnumValueDefinition : SchemaNode
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }
    public override string OutputElement => "Enum Value";
    public override string OutputName => Name;
}

public class InputObjectTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }
    public required InputValueDefinitions InputFields { get; init; }
    public override string OutputElement => "Input object";
    public override string OutputName => Name;
    public override bool IsInputType => true;
    public override bool IsOutputType => false;
}

public class InputValueDefinition : SchemaNode
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

public abstract class TypeNode : SchemaNode
{
    public required bool NonNull { get; init; }
    public abstract TypeDefinition? Definition { get; set; }
    public abstract bool IsInputType { get; }
    public abstract bool IsOutputType { get; }
    public abstract TypeNode Clone(bool? nonNull = null);
}

public class TypeName : TypeNode
{
    public required string Name { get; init; }
    public required override TypeDefinition? Definition { get; set; }
    public override bool IsInputType => Definition!.IsInputType;
    public override bool IsOutputType => Definition!.IsOutputType;
    
    public override TypeNode Clone(bool? nonNull = null)
    {
        return new TypeName()
        {
            NonNull = nonNull ?? NonNull,
            Name = Name,
            Definition = Definition,
            Location = Location,
        };
    }
}

public class TypeList : TypeNode
{
    public required TypeNode Type { get; init; }
    public override TypeDefinition? Definition { get => Type.Definition; set => Type.Definition = value; }
    public override bool IsInputType => Type.IsInputType;
    public override bool IsOutputType => Type.IsOutputType;

    public override TypeNode Clone(bool? nonNull = null)
    {
        return new TypeList()
        {
            NonNull = nonNull ?? NonNull,
            Type = Type.Clone(),
            Definition = Definition,
            Location = Location,
        };
    }
}