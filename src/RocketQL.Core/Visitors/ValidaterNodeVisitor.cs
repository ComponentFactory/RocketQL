namespace RocketQL.Core.Visitors;

public abstract class ValidaterNodeVisitor
{
    public static void CheckDoubleUnderscore(TypeDefinition node)
    {
        if (node.Name.StartsWith("__"))
            throw ValidationException.NameDoubleUnderscore(node);
    }
}