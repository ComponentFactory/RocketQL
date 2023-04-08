﻿namespace RocketQL.Core;

public record class DocumentNode(OperationDefinitionNodeList Operations,
                                 FragmentDefinitionNodeList Fragments,
                                 SchemaDefinitionNodeList Schemas,
                                 ScalarTypeDefinitionNodeList ScalarTypes, 
                                 ObjectTypeDefinitionNodeList ObjectTypes, 
                                 InterfaceTypeDefinitionNodeList InterfaceTypes,
                                 UnionTypeDefinitionNodeList UnionTypes,
                                 EnumTypeDefinitionNodeList EnumTypes,
                                 InputObjectTypeDefinitionNodeList InputObjectTypes,
                                 DirectiveDefinitionNodeList Directives);

//------------------------------------------------------------------------------
// Nodes at the top level of a document
//------------------------------------------------------------------------------
public record class OperationDefinitionNode(OperationType Operation, string Name, VariableDefinitionNodeList VariableDefinitions, DirectiveNodeList Directives, SelectionDefinitionNodeList SelectionSet);
public record class FragmentDefinitionNode(string Name, string TypeCondition, DirectiveNodeList Directives, SelectionDefinitionNodeList SelectionSet);
public record class SchemaDefinitionNode(string Description, DirectiveNodeList Directives, OperationTypeDefinitionNodeList OperationTypes);
public record class ScalarTypeDefinitionNode(string Description, string Name, DirectiveNodeList Directives);
public record class ObjectTypeDefinitionNode(string Description, string Name, NameList ImplementsInterfaces, DirectiveNodeList Directives, FieldDefinitionNodeList Fields);
public record class InterfaceTypeDefinitionNode(string Description, string Name, NameList ImplementsInterfaces, DirectiveNodeList Directives, FieldDefinitionNodeList Fields);
public record class UnionTypeDefinitionNode(string Description, string Name, DirectiveNodeList Directives, NameList MemberTypes);
public record class EnumTypeDefinitionNode(string Description, string Name, DirectiveNodeList Directives, EnumValueDefinitionList EnumValues);
public record class InputObjectTypeDefinitionNode(string Description, string Name, DirectiveNodeList Directives, InputValueDefinitionNodeList InputFields);
public record class DirectiveDefinitionNode(string Description, string Name, InputValueDefinitionNodeList Arguments, bool Repeatable, DirectiveLocations DirectiveLocations);

//------------------------------------------------------------------------------
// Nodes contained inside other nodes
//------------------------------------------------------------------------------
public record class DirectiveNode(string Name, ObjectFieldNodeList Arguments);
public record class OperationTypeDefinitionNode(OperationType Operation, string NamedType);
public record class VariableDefinitionNode(string Name, TypeNode Type, ValueNode? DefaultValue, DirectiveNodeList Directives);
public record class InputValueDefinitionNode(string Description, string Name, TypeNode Type, ValueNode? DefaultValue, DirectiveNodeList Directives);
public record class FieldDefinitionNode(string Description, string Name, InputValueDefinitionNodeList Arguments, TypeNode Type, DirectiveNodeList Directives);
public record class EnumValueDefinition(string Description, string Name, DirectiveNodeList Directives);

//------------------------------------------------------------------------------
// SelectionSet can contains fields, fragment spreads and inline fragments
//------------------------------------------------------------------------------
public abstract record class SelectionNode();
public record class FieldSelectionNode(string Alias, string Name, ObjectFieldNodeList Arguments, DirectiveNodeList Directives, SelectionDefinitionNodeList SelectionSet) : SelectionNode();
public record class FragmentSpreadSelectionNode(string Name, DirectiveNodeList Directives) : SelectionNode();
public record class InlineFragmentSelectionNode(string TypeCondition, DirectiveNodeList Directives, SelectionDefinitionNodeList SelectionSet) : SelectionNode();

//------------------------------------------------------------------------------
// Types are specified by name or as a list containing a type
//------------------------------------------------------------------------------
public abstract record class TypeNode(bool NonNull);
public record class TypeNameNode(string Name, bool NonNull) : TypeNode(NonNull);
public record class TypeListNode(TypeNode Type, bool NonNull) : TypeNode(NonNull);

//------------------------------------------------------------------------------
// Constant Values
//------------------------------------------------------------------------------
public abstract record class ValueNode();
public record class NullValueNode() : ValueNode();
public record class BooleanValueNode(bool Value) : ValueNode();
public record class IntValueNode(string Value) : ValueNode();
public record class FloatValueNode(string Value) : ValueNode();
public record class StringValueNode(string Value) : ValueNode();
public record class EnumValueNode(string Value) : ValueNode();
public record class ListValueNode(ValueNodeList Values) : ValueNode();
public record class ObjectValueNode(ObjectFieldNodeList ObjectFields) : ValueNode();
public record class ObjectFieldNode(string Name, ValueNode Value) : ValueNode();
public record class VariableValueNode(string Value) : ValueNode();

