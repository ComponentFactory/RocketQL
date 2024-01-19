namespace RocketQL.Core.Extensions;

public static class SyntaxNodeExtensions
{
    public static string OutputElement(this SyntaxNode node)
    {
        return node switch
        {
            SyntaxFieldSelectionNode => "Field",
            SyntaxVariableDefinitionNode => "Variable",
            SyntaxFragmentDefinitionNode => "Fragment",
            SyntaxFragmentSpreadSelectionNode => "Fragment spread",
            SyntaxInlineFragmentSelectionNode => "Inline fragment",
            SyntaxDirectiveDefinitionNode => "Directive",
            SyntaxDirectiveNode => "Directive",
            SyntaxScalarTypeDefinitionNode => "Scalar",
            SyntaxObjectTypeDefinitionNode => "Type",
            SyntaxInterfaceTypeDefinitionNode => "Interface",
            SyntaxUnionTypeDefinitionNode => "Union",
            SyntaxInputObjectTypeDefinitionNode => "Input object",
            SyntaxEnumTypeDefinitionNode => "Enum",
            SyntaxEnumValueDefinition => "Enum value",
            SyntaxFieldDefinitionNode => "Field",
            SyntaxOperationTypeDefinitionNode operationType => operationType.Operation.ToString(),
            SyntaxNameNode name => name.Usage switch
            {
                SyntaxNameUsage.Interface => "Interface",
                SyntaxNameUsage.MemberType => "Member type",
                _ => ""
            },
            SyntaxInputValueDefinitionNode inputValue => inputValue.Usage switch
            {
                SyntaxInputValueUsage.Argument => "Argument",
                SyntaxInputValueUsage.InputField => "Input field",
                _ => ""
            },
            SyntaxExtendScalarTypeDefinitionNode => "Extend scalar",
            SyntaxExtendObjectTypeDefinitionNode => "Extend type",
            SyntaxExtendInterfaceTypeDefinitionNode => "Extend interface",
            SyntaxExtendUnionTypeDefinitionNode => "Extend union",
            SyntaxExtendInputObjectTypeDefinitionNode => "Extend input object",
            SyntaxExtendEnumTypeDefinitionNode => "Extend enum",
            _ => ""
        };
    }

    public static string OutputName(this SyntaxNode node)
    {
        return node switch
        {
            SyntaxFieldSelectionNode fieldSelection => string.IsNullOrEmpty(fieldSelection.Alias) ? fieldSelection.Name : fieldSelection.Alias,
            SyntaxVariableDefinitionNode variable => variable.Name,
            SyntaxFragmentDefinitionNode fragment => fragment.Name,
            SyntaxFragmentSpreadSelectionNode fragmentSpread => fragmentSpread.Name,
            SyntaxInlineFragmentSelectionNode inlineFragment => inlineFragment.TypeCondition,
            SyntaxDirectiveDefinitionNode directive => directive.Name,
            SyntaxDirectiveNode directive => directive.Name,
            SyntaxScalarTypeDefinitionNode scalarType => scalarType.Name,
            SyntaxObjectTypeDefinitionNode objectType => objectType.Name,
            SyntaxInterfaceTypeDefinitionNode interfaceType => interfaceType.Name,
            SyntaxUnionTypeDefinitionNode unionType => unionType.Name,
            SyntaxInputObjectTypeDefinitionNode inputObjectType => inputObjectType.Name,
            SyntaxEnumTypeDefinitionNode enumType => enumType.Name,
            SyntaxEnumValueDefinition enumValue => enumValue.Name,
            SyntaxFieldDefinitionNode field => field.Name,
            SyntaxNameNode name => name.Name,
            SyntaxInputValueDefinitionNode inputValue => inputValue.Name,
            SyntaxOperationTypeDefinitionNode operationType => operationType.NamedType,
            SyntaxExtendScalarTypeDefinitionNode extendScalarType => extendScalarType.Name,
            SyntaxExtendObjectTypeDefinitionNode extendObjectType => extendObjectType.Name,
            SyntaxExtendInterfaceTypeDefinitionNode extendInterfaceType => extendInterfaceType.Name,
            SyntaxExtendUnionTypeDefinitionNode extendUnionType => extendUnionType.Name,
            SyntaxExtendInputObjectTypeDefinitionNode extendInputObjectType => extendInputObjectType.Name,
            SyntaxExtendEnumTypeDefinitionNode extendEnumType => extendEnumType.Name,
            _ => ""


        };
    }
}

