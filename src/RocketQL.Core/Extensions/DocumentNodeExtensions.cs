namespace RocketQL.Core.Extensions;

public static class DocumentNodeExtensions
{
    public static string OutputElement(this DocumentNode node)
    {
        return node switch
        {
            OperationDefinition => "Operation",
            OperationTypeDefinition => "Operation",
            VariableDefinition => "Variable",
            FragmentDefinition => "Fragment",
            SelectionField => "Field",
            SelectionFragmentSpread => "Fragment spread",
            SelectionInlineFragment => "Inline fragment",
            SchemaRoot => "Schema",
            SchemaDefinition => "Schema",
            DirectiveDefinition => "Directive",
            ScalarTypeDefinition => "Scalar",
            ObjectTypeDefinition => "Type",
            InterfaceTypeDefinition => "Interface",
            UnionTypeDefinition => "Union",
            EnumTypeDefinition => "Enum",
            EnumValueDefinition => "Enum Value",
            InputObjectTypeDefinition => "Input object",
            FieldDefinition => "Field",
            InputValueDefinition inputValue => inputValue.Usage switch
            {
                InputValueUsage.Argument => "Argument",
                InputValueUsage.InputField => "Input field",
                _ => ""
            },
            Directive => "Directive",
            Interface => "Interface",
            MemberType => "Member Type",
            _ => ""
        };
    }

    public static string OutputName(this DocumentNode node)
    {
        return node switch
        {
            OperationDefinition operation => string.IsNullOrEmpty(operation.Name) ? operation.Operation.ToString() : operation.Name,
            OperationTypeDefinition operationType => operationType.Operation.ToString(),
            VariableDefinition variable => variable.Name,
            FragmentDefinition fragment => fragment.Name,
            SelectionField field => string.IsNullOrEmpty(field.Alias) ? field.Name : field.Alias,
            SelectionFragmentSpread fragmentSpread => fragmentSpread.Name,
            SelectionInlineFragment inlineFragment => inlineFragment.TypeCondition,
            DirectiveDefinition directive => directive.Name,
            TypeDefinition type => type.Name,
            FieldDefinition field => field.Name,
            EnumValueDefinition enumValue => enumValue.Name,
            InputValueDefinition inputValue => inputValue.Name,
            Directive directive => directive.Name,
            Interface interfaceType => interfaceType.Name,
            MemberType memberType => memberType.Name,
            _ => ""
        };
    }
}

