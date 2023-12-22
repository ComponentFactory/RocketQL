using RocketQL.Core.Nodes;
using System.Collections.Generic;

namespace RocketQL.Core.Exceptions;

public class ValidationException(Location location, string message) : RocketException(location, message)
{
    public static ValidationException UnrecognizedType(SyntaxNode node) => new(node.Location, $"Unrecognized type '{node.GetType()}' encountered.");
    public static ValidationException UnrecognizedType(TypeDefinition node) => new(node.Location, $"Unrecognized type '{node.GetType()}' encountered.");
    public static ValidationException UnrecognizedType(Location location, string name) => new(location, $"Unrecognized type '{name}' encountered.");

    public static ValidationException SchemaDefinitionAlreadyDefined(Location location) => new(location, $"Schema definition is already defined.");
    public static ValidationException SchemaDefinitionEmpty(Location location) => new(location, "Schema definition does not define any operations.");
    public static ValidationException SchemaOperationAlreadyDefined(Location location, OperationType operation) => new(location, $"Schema definition already defines the {operation} operation.");
    public static ValidationException SchemaDefinitionMissingQuery(Location location) => new(location, $"Schema definition missing mandatory Query operation.");
    public static ValidationException SchemaOperationsNotUnique(Location location, string op1, string op2, string type) => new(location, $"Schema {op1} and {op2} operations cannot have same '{type}' type.");
    public static ValidationException TypeNotDefinedForSchemaOperation(Location location, OperationType operation, string type) => new(location, $"Type '{type}' not defined for the schema operation {operation}.");
    public static ValidationException OperationTypeAlreadyDefined(Location location, OperationType operationType) => new(location, $"Type already defined for '{operationType}' operation.");

    public static ValidationException NameDoubleUnderscore(SchemaNode node) => new(node.Location, $"{node.OutputElement} name '{node.OutputName}' not allowed to start with two underscores.");
    public static ValidationException NameAlreadyDefined(Location location, string target, string name) => new(location, $"{target} name '{name}' is already defined.");
    public static ValidationException ListEntryDuplicateName(Location location, string list, string entryName, string entryType, string name) => new(location, $"{list} '{entryName}' has duplicate {entryType} name '{name}'.");
    public static ValidationException ListEntryDoubleUnderscore(Location location, string list, string entryName, string entryType, string name) => new(location, $"{list} '{entryName}' has {entryType} name '{name}' not allowed to start with two underscores.");
    public static ValidationException UndefinedTypeForListEntry(Location location, string type, SchemaNode listNode, SchemaNode parentNode) => new(location, $"Undefined type '{type}' for {listNode.OutputElement.ToLower()} '{listNode.OutputName}' of {parentNode.OutputElement.ToLower()} '{parentNode.OutputName}'.");
    public static ValidationException UndefinedDirective(SchemaNode node, SchemaNode parentNode) => new(node.Location, $"Undefined directive '{node.OutputName}' defined on {parentNode.OutputElement.ToLower()} '{parentNode.OutputName}'.");
    public static ValidationException UndefinedDirective(SchemaNode node, SchemaNode parentNode, SchemaNode grandParentNode) => new(node.Location, $"Undefined directive '{node.OutputName}' defined on {parentNode.OutputElement.ToLower()} '{parentNode.OutputName}' of {grandParentNode.OutputElement.ToLower()} '{grandParentNode.OutputName}'.");
}
