namespace RocketQL.Core.Extensions;

public static class SchemaNodeExtensions
{
    public static void CheckDoubleUnderscore(this TypeDefinition node)
    {
        if (node.Name.StartsWith("__"))
            throw ValidationException.NameDoubleUnderscore(node);
    }
}

