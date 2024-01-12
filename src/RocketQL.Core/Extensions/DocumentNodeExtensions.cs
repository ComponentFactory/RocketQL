namespace RocketQL.Core.Extensions;

public static class DocumentNodeExtensions
{
    public static void CheckDoubleUnderscore(this TypeDefinition node)
    {
        if (node.Name.StartsWith("__"))
            throw ValidationException.NameDoubleUnderscore(node);
    }

    public static Directives ConvertDirectives(this SyntaxDirectiveNodeList directives)
    {
        var nodes = new Directives();

        foreach (var directive in directives)
        {
            nodes.Add(new()
            {
                Name = directive.Name,
                Definition = null,
                Arguments = directive.Arguments.ConvertObjectFields(),
                Location = directive.Location
            });
        }

        return nodes;
    }

    public static ObjectFields ConvertObjectFields(this SyntaxObjectFieldNodeList fields)
    {
        var nodes = new ObjectFields();

        foreach (var field in fields)
            nodes.Add(field.Name, field);

        return nodes;
    }
}

