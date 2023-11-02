﻿namespace RocketQL.Core.Nodes;

public record class SyntaxRequestNode(SyntaxOperationDefinitionNodeList Operations, SyntaxFragmentDefinitionNodeList Fragments);

public record class SyntaxSchemaNode(SyntaxDirectiveDefinitionNodeList Directives,
                                     SyntaxSchemaDefinitionNodeList Schemas, SyntaxExtendSchemaDefinitionNodeList ExtendSchemas,
                                     SyntaxScalarTypeDefinitionNodeList ScalarTypes, SyntaxExtendScalarTypeDefinitionNodeList ExtendScalarTypes,
                                     SyntaxObjectTypeDefinitionNodeList ObjectTypes, SyntaxExtendObjectTypeDefinitionNodeList ExtendObjectTypes,
                                     SyntaxInterfaceTypeDefinitionNodeList InterfaceTypes, SyntaxExtendInterfaceTypeDefinitionNodeList ExtendInterfaceTypes,
                                     SyntaxUnionTypeDefinitionNodeList UnionTypes, SyntaxExtendUnionTypeDefinitionNodeList ExtendUnionTypes,
                                     SyntaxEnumTypeDefinitionNodeList EnumTypes, SyntaxExtendEnumTypeDefinitionNodeList ExtendEnumTypes,
                                     SyntaxInputObjectTypeDefinitionNodeList InputObjectTypes, SyntaxExtendInputObjectTypeDefinitionNodeList ExtendInputObjectTypes);

public class SyntaxOperationDefinitionNodeList : List<SyntaxOperationDefinitionNode> { };
public class SyntaxFragmentDefinitionNodeList : List<SyntaxFragmentDefinitionNode> { };
public class SyntaxDirectiveDefinitionNodeList : List<SyntaxDirectiveDefinitionNode> { };
public class SyntaxSchemaDefinitionNodeList : List<SyntaxSchemaDefinitionNode> { };
public class SyntaxExtendSchemaDefinitionNodeList : List<SyntaxExtendSchemaDefinitionNode> { };
public class SyntaxScalarTypeDefinitionNodeList : List<SyntaxScalarTypeDefinitionNode> { };
public class SyntaxExtendScalarTypeDefinitionNodeList : List<SyntaxExtendScalarTypeDefinitionNode> { };
public class SyntaxObjectTypeDefinitionNodeList : List<SyntaxObjectTypeDefinitionNode> { };
public class SyntaxExtendObjectTypeDefinitionNodeList : List<SyntaxExtendObjectTypeDefinitionNode> { };
public class SyntaxInterfaceTypeDefinitionNodeList : List<SyntaxInterfaceTypeDefinitionNode> { };
public class SyntaxExtendInterfaceTypeDefinitionNodeList : List<SyntaxExtendInterfaceTypeDefinitionNode> { };
public class SyntaxUnionTypeDefinitionNodeList : List<SyntaxUnionTypeDefinitionNode> { };
public class SyntaxExtendUnionTypeDefinitionNodeList : List<SyntaxExtendUnionTypeDefinitionNode> { };
public class SyntaxEnumTypeDefinitionNodeList : List<SyntaxEnumTypeDefinitionNode> { };
public class SyntaxExtendEnumTypeDefinitionNodeList : List<SyntaxExtendEnumTypeDefinitionNode> { };
public class SyntaxInputObjectTypeDefinitionNodeList : List<SyntaxInputObjectTypeDefinitionNode> { };
public class SyntaxExtendInputObjectTypeDefinitionNodeList : List<SyntaxExtendInputObjectTypeDefinitionNode> { };
public class SyntaxDirectiveNodeList : List<SyntaxDirectiveNode> { };
public class SyntaxEnumValueDefinitionList : List<SyntaxEnumValueDefinition> { };
public class SyntaxFieldDefinitionNodeList : List<SyntaxFieldDefinitionNode> { };
public class SyntaxInputValueDefinitionNodeList : List<SyntaxInputValueDefinitionNode> { };
public class SyntaxObjectFieldNodeList : List<ObjectFieldNode> { };
public class SyntaxOperationTypeDefinitionNodeList : List<SyntaxOperationTypeDefinitionNode> { };
public class SyntaxSelectionDefinitionNodeList : List<SyntaxSelectionNode> { };
public class SyntaxNameList : List<string> { };
public class SyntaxVariableDefinitionNodeList : List<SyntaxVariableDefinitionNode> { };

public record class SyntaxOperationDefinitionNode(OperationType Operation, string Name, SyntaxVariableDefinitionNodeList VariableDefinitions, SyntaxDirectiveNodeList Directives, SyntaxSelectionDefinitionNodeList SelectionSet, Location Location) : LocationNode(Location);
public record class SyntaxFragmentDefinitionNode(string Name, string TypeCondition, SyntaxDirectiveNodeList Directives, SyntaxSelectionDefinitionNodeList SelectionSet, Location Location) : LocationNode(Location);
public record class SyntaxExtendSchemaDefinitionNode(SyntaxDirectiveNodeList Directives, SyntaxOperationTypeDefinitionNodeList OperationTypes, Location Location) : LocationNode(Location);
public record class SyntaxSchemaDefinitionNode(string Description, SyntaxDirectiveNodeList Directives, SyntaxOperationTypeDefinitionNodeList OperationTypes, Location Location) : SyntaxExtendSchemaDefinitionNode(Directives, OperationTypes, Location);
public record class SyntaxExtendScalarTypeDefinitionNode(string Name, SyntaxDirectiveNodeList Directives, Location Location) : LocationNode(Location);
public record class SyntaxScalarTypeDefinitionNode(string Description, string Name, SyntaxDirectiveNodeList Directives, Location Location) : SyntaxExtendScalarTypeDefinitionNode(Name, Directives, Location);
public record class SyntaxExtendObjectTypeDefinitionNode(string Name, SyntaxNameList ImplementsInterfaces, SyntaxDirectiveNodeList Directives, SyntaxFieldDefinitionNodeList Fields, Location Location) : LocationNode(Location);
public record class SyntaxObjectTypeDefinitionNode(string Description, string Name, SyntaxNameList ImplementsInterfaces, SyntaxDirectiveNodeList Directives, SyntaxFieldDefinitionNodeList Fields, Location Location) : SyntaxExtendObjectTypeDefinitionNode(Name, ImplementsInterfaces, Directives, Fields, Location);
public record class SyntaxExtendInterfaceTypeDefinitionNode(string Name, SyntaxNameList ImplementsInterfaces, SyntaxDirectiveNodeList Directives, SyntaxFieldDefinitionNodeList Fields, Location Location) : LocationNode(Location);
public record class SyntaxInterfaceTypeDefinitionNode(string Description, string Name, SyntaxNameList ImplementsInterfaces, SyntaxDirectiveNodeList Directives, SyntaxFieldDefinitionNodeList Fields, Location Location) : SyntaxExtendInterfaceTypeDefinitionNode(Name, ImplementsInterfaces, Directives, Fields, Location);
public record class SyntaxExtendUnionTypeDefinitionNode(string Name, SyntaxDirectiveNodeList Directives, SyntaxNameList MemberTypes, Location Location) : LocationNode(Location);
public record class SyntaxUnionTypeDefinitionNode(string Description, string Name, SyntaxDirectiveNodeList Directives, SyntaxNameList MemberTypes, Location Location) : SyntaxExtendUnionTypeDefinitionNode(Name, Directives, MemberTypes, Location);
public record class SyntaxExtendEnumTypeDefinitionNode(string Name, SyntaxDirectiveNodeList Directives, SyntaxEnumValueDefinitionList EnumValues, Location Location) : LocationNode(Location);
public record class SyntaxEnumTypeDefinitionNode(string Description, string Name, SyntaxDirectiveNodeList Directives, SyntaxEnumValueDefinitionList EnumValues, Location Location) : SyntaxExtendEnumTypeDefinitionNode(Name, Directives, EnumValues, Location);
public record class SyntaxExtendInputObjectTypeDefinitionNode(string Name, SyntaxDirectiveNodeList Directives, SyntaxInputValueDefinitionNodeList InputFields, Location Location) : LocationNode(Location);
public record class SyntaxInputObjectTypeDefinitionNode(string Description, string Name, SyntaxDirectiveNodeList Directives, SyntaxInputValueDefinitionNodeList InputFields, Location Location) : SyntaxExtendInputObjectTypeDefinitionNode(Name, Directives, InputFields, Location);
public record class SyntaxDirectiveDefinitionNode(string Description, string Name, SyntaxInputValueDefinitionNodeList Arguments, bool Repeatable, DirectiveLocations DirectiveLocations, Location Location) : LocationNode(Location);

public record class SyntaxDirectiveNode(string Name, SyntaxObjectFieldNodeList Arguments, Location Location) : LocationNode(Location);
public record class SyntaxOperationTypeDefinitionNode(OperationType Operation, string NamedType, Location Location) : LocationNode(Location);
public record class SyntaxVariableDefinitionNode(string Name, SyntaxTypeNode Type, ValueNode? DefaultValue, SyntaxDirectiveNodeList Directives, Location Location) : LocationNode(Location);
public record class SyntaxInputValueDefinitionNode(string Description, string Name, SyntaxTypeNode Type, ValueNode? DefaultValue, SyntaxDirectiveNodeList Directives, Location Location) : LocationNode(Location);
public record class SyntaxFieldDefinitionNode(string Description, string Name, SyntaxInputValueDefinitionNodeList Arguments, SyntaxTypeNode Type, SyntaxDirectiveNodeList Directives, Location Location) : LocationNode(Location);
public record class SyntaxEnumValueDefinition(string Description, string Name, SyntaxDirectiveNodeList Directives, Location Location) : LocationNode(Location);

public abstract record class SyntaxSelectionNode(Location Location) : LocationNode(Location);
public record class SyntaxFieldSelectionNode(string Alias, string Name, SyntaxObjectFieldNodeList Arguments, SyntaxDirectiveNodeList Directives, SyntaxSelectionDefinitionNodeList SelectionSet, Location Location) : SyntaxSelectionNode(Location);
public record class SyntaxFragmentSpreadSelectionNode(string Name, SyntaxDirectiveNodeList Directives, Location Location) : SyntaxSelectionNode(Location);
public record class SyntaxInlineFragmentSelectionNode(string TypeCondition, SyntaxDirectiveNodeList Directives, SyntaxSelectionDefinitionNodeList SelectionSet, Location Location) : SyntaxSelectionNode(Location);

public abstract record class SyntaxTypeNode(bool NonNull, Location Location) : LocationNode(Location);
public record class SyntaxTypeNameNode(string Name, bool NonNull, Location Location) : SyntaxTypeNode(NonNull, Location);
public record class SyntaxTypeListNode(SyntaxTypeNode Type, bool NonNull, Location Location) : SyntaxTypeNode(NonNull, Location);
