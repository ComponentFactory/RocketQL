using System.Data;

namespace RocketQL.Core.Base;

public static class TypeHelper
{
    public static bool IsInputTypeCompatibleWithValue(IReadOnlyDictionary<string, TypeDefinition> types, TypeNode typeNode, ValueNode valueNode)
    {
        return typeNode switch
        {
            TypeName typeNameNode => IsInputTypeCompatibleWithTypeName(types, typeNameNode, valueNode),
            TypeList typeListNode => IsInputTypeCompatibleWithTypeList(types, typeListNode, valueNode),
            TypeNonNull typeNonNullNode => IsInputValueCompatibleWithTypeNonNull(types, typeNonNullNode, valueNode),
            _ => false
        };
    }

    private static bool IsInputTypeCompatibleWithTypeName(IReadOnlyDictionary<string, TypeDefinition> types, TypeName typeNameNode, ValueNode valueNode)
    {
        if (types.TryGetValue(typeNameNode.Name, out var typeDefinition))
        {
            return typeDefinition switch
            {
                ScalarTypeDefinition scalarTypeDefinition => IsInputTypeCompatibleWithScalarType(valueNode, scalarTypeDefinition),
                EnumTypeDefinition enumTypeDefinition => IsInputTypeCompatibleWithEnumType(valueNode, enumTypeDefinition),
                InputObjectTypeDefinition inputObjectTypeDefinition => IsInputTypeCompatibleWithInputObjectType(types, valueNode, inputObjectTypeDefinition),
                _ => false
            };
        }

        return false;
    }

    private static bool IsInputTypeCompatibleWithScalarType(ValueNode valueNode, ScalarTypeDefinition scalarTypeDefinition)
    {
        // You can assign 'null' to a scalar
        if (valueNode is NullValueNode)
            return true;

        // Note that input coercion allows an integer to be assigned to float type
        return scalarTypeDefinition.Name switch
        {
            "Int" => valueNode is IntValueNode,
            "Float" => (valueNode is FloatValueNode) || (valueNode is IntValueNode),
            "Boolean" => valueNode is BooleanValueNode,
            _ => valueNode is StringValueNode,
        };
    }

    private static bool IsInputTypeCompatibleWithEnumType(ValueNode valueNode, EnumTypeDefinition enumTypeDefinition)
    {
        // You can assign 'null' to an enum
        if (valueNode is NullValueNode)
            return true;

        return valueNode switch
        {
            StringValueNode stringValue => enumTypeDefinition.EnumValues.ContainsKey(stringValue.Value),
            EnumValueNode enumValue => enumTypeDefinition.EnumValues.ContainsKey(enumValue.Value),
            _ => false
        };
    }

    private static bool IsInputTypeCompatibleWithInputObjectType(IReadOnlyDictionary<string, TypeDefinition> types, ValueNode valueNode, InputObjectTypeDefinition inputObjectTypeDefinition)
    {
        // You can assign 'null' to an input object
        if (valueNode is NullValueNode)
            return true;

        // You must assign an object
        if (valueNode is not ObjectValueNode objectValueNode)
            return false;

        var objectNodes = objectValueNode.ObjectFields.ToDictionary(o => o.Name, o => o.Value);
        foreach (var inputFieldDefinition in inputObjectTypeDefinition.InputFields.Values)
        {
            if (!objectNodes.TryGetValue(inputFieldDefinition.Name, out var objectNode))
            {
                // Must provide a value for mandatory fields, all non-null types are mandatory
                if (inputFieldDefinition.Type is TypeNonNull)
                    return false;
            }
            else if (!IsInputTypeCompatibleWithValue(types, inputFieldDefinition.Type, objectNode))
                return false;

            objectNodes.Remove(inputFieldDefinition.Name);
        }

        // Cannot have values for fields not in the type definintion
        return (objectNodes.Count == 0);
    }

    private static bool IsInputTypeCompatibleWithTypeList(IReadOnlyDictionary<string, TypeDefinition> types, TypeList typeListNode, ValueNode valueNode)
    {
        // You can assign 'null' to a list
        if (valueNode is NullValueNode)
            return true;

        // You must assign a list
        if (valueNode is not ListValueNode listValueNode)
            return false;

        foreach (var entryValueNode in listValueNode.Values)
            if (!IsInputTypeCompatibleWithValue(types, typeListNode.Type, entryValueNode))
                return false;

        return true;
    }

    private static bool IsInputValueCompatibleWithTypeNonNull(IReadOnlyDictionary<string, TypeDefinition> types, TypeNonNull typeNonNullNode, ValueNode valueNode)
    {
        // You cannot assign 'null' to a non-null type
        if (valueNode is NullValueNode)
            return false;

        return IsInputTypeCompatibleWithValue(types, typeNonNullNode.Type, valueNode);
    }
}
