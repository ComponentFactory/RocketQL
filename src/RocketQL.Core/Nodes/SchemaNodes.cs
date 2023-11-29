﻿using System.Diagnostics.CodeAnalysis;

namespace RocketQL.Core.Nodes;

public class SchemaLocation
{
    public required Location Location { get; init; }
}

public class DirectiveDefinitions : Dictionary<string, DirectiveDefinition> { };
public class ScalarTypeDefinitions : Dictionary<string, ScalarTypeDefinition> { };
public class ObjectTypeDefinitions : Dictionary<string, ObjectTypeDefinition> { };
public class InterfaceTypeDefinitions : Dictionary<string, InterfaceTypeDefinition> { };
public class UnionTypeDefinitions : Dictionary<string, UnionTypeDefinition> { };
public class EnumTypeDefinitions : Dictionary<string, EnumTypeDefinition> { };
public class InputObjectTypeDefinitions : Dictionary<string, InputObjectTypeDefinition> { };

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
    public required ObjectTypeDefinition ObjectTypeDefinition { get; set; }
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

public class Directives : Dictionary<string, Directive> { };

public class Directive : SchemaLocation
{
    public required DirectiveDefinition Definition { get; set; }
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

public class ObjectTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Interfaces ImplementsInterfaces { get; init; }
    public required Directives Directives { get; init; }
    public required FieldDefinitions Fields { get; init; }
    public required ObjectTypeDefinitions UsedByTypes { get; init; }
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
    public required InterfaceTypeDefinition Definition { get; set; }
}

public class FieldDefinitions : Dictionary<string, FieldDefinition> { };

public class FieldDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required InputValueDefinitions Arguments { get; init; }
    public required TypeDefinition Definition { get; set; }
}

public class UnionTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }
    public required MemberTypes MemberTypes { get; init; }
}

public class MemberTypes : Dictionary<string, MemberType> { };

public class MemberType
{
    public required ObjectTypeDefinition Definition { get; set; }
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

public class InputObjectTypeDefinition : TypeDefinition
{
    public required string Description { get; init; }
    public required string Name { get; init; }
    public required Directives Directives { get; init; }
    public required InputValueDefinitions InputFields { get; init; }
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
    public required TypeDefinition Definition { get; set; }
}