namespace RocketQL.Core.Base;

public partial class Schema
{
    public bool IsInputTypeCompatibleWithValue(TypeNode typeNode, ValueNode valueNode)
    {
        return typeNode switch
        {
            TypeName typeNameNode => IsInputTypeCompatibleWithTypeName(typeNameNode, valueNode),
            TypeList typeListNode => IsInputTypeCompatibleWithTypeList(typeListNode, valueNode),
            TypeNonNull typeNonNullNode => IsInputValueCompatibleWithTypeNonNull(typeNonNullNode, valueNode),
            _ => false
        };
    }

    private bool IsInputTypeCompatibleWithTypeName(TypeName typeNameNode, ValueNode valueNode)
    {
        if (Types.TryGetValue(typeNameNode.Name, out var typeDefinition))
        {
            return typeDefinition switch
            {
                ScalarTypeDefinition scalarTypeDefinition => IsInputTypeCompatibleWithScalarType(typeNameNode, valueNode, scalarTypeDefinition),
                EnumTypeDefinition enumTypeDefinition => IsInputTypeCompatibleWithEnumType(typeNameNode, valueNode, enumTypeDefinition),
                InputObjectTypeDefinition inputObjectTypeDefinition => IsInputTypeCompatibleWithInputObjectType(typeNameNode, valueNode, inputObjectTypeDefinition),
                _ => false
            };
        }

        return false;
    }

    private static bool IsInputTypeCompatibleWithScalarType(TypeName typeNameNode, ValueNode valueNode, ScalarTypeDefinition scalarTypeDefinition)
    {
        // You can assign 'null' to a scalar
        if (valueNode is NullValueNode)
            return true;

        return scalarTypeDefinition.Name switch
        {
            "Int" => valueNode is IntValueNode,
            "Float" => (valueNode is FloatValueNode) || (valueNode is IntValueNode),
            "Boolean" => valueNode is BooleanValueNode,
            _ => valueNode is StringValueNode,
        };
    }

    private static bool IsInputTypeCompatibleWithEnumType(TypeName typeNameNode, ValueNode valueNode, EnumTypeDefinition enumTypeDefinition)
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

    private bool IsInputTypeCompatibleWithInputObjectType(TypeName typeNameNode, ValueNode valueNode, InputObjectTypeDefinition inputObjectTypeDefinition)
    {
        // You can assign 'null' to an input object
        if (valueNode is NullValueNode)
            return true;

        // You must assign an object to an input type
        if (valueNode is not ObjectValueNode objectValueNode)
            return false;

        var objectNodes = objectValueNode.ObjectFields.ToDictionary(o => o.Name, o => o.Value);
        foreach (var inputFieldDefinition in inputObjectTypeDefinition.InputFields.Values)
        {
            if (!objectNodes.TryGetValue(inputFieldDefinition.Name, out var objectNode))
            {
                if (inputFieldDefinition.Type is TypeNonNull)
                    return false;
            }
            else if (!IsInputTypeCompatibleWithValue(inputFieldDefinition.Type, objectNode))
                return false;

            objectNodes.Remove(inputFieldDefinition.Name);
        }

        return (objectNodes.Count == 0);
    }

    private bool IsInputTypeCompatibleWithTypeList(TypeList typeListNode, ValueNode valueNode)
    {
        if (valueNode is NullValueNode)
            return true;

        if (valueNode is not ListValueNode listValueNode)
            return false;

        foreach (var entryValueNode in listValueNode.Values)
            if (!IsInputTypeCompatibleWithValue(typeListNode.Type, entryValueNode))
                return false;

        return true;
    }

    private bool IsInputValueCompatibleWithTypeNonNull(TypeNonNull typeNonNullNode, ValueNode valueNode)
    {
        if (valueNode is NullValueNode)
            return false;

        return IsInputTypeCompatibleWithValue(typeNonNullNode.Type, valueNode);
    }
}
