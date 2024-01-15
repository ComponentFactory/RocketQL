namespace RocketQL.Core.Visitors;

public abstract class ConverterNodeVisitor
{
    public static void CheckDoubleUnderscore(TypeDefinition node)
    {
        if (node.Name.StartsWith("__"))
            throw ValidationException.NameDoubleUnderscore(node);
    }

    public static Directives ConvertDirectives(SyntaxDirectiveNodeList directives)
    {
        var nodes = new Directives();

        foreach (var directive in directives)
        {
            nodes.Add(new()
            {
                Name = directive.Name,
                Definition = null,
                Arguments = ConvertObjectFields(directive.Arguments, directive.Location, "Directive", directive.Name, "argument"),
                Location = directive.Location
            });
        }

        return nodes;
    }

    public static ObjectFields ConvertObjectFields(SyntaxObjectFieldNodeList fields, Location location, string parentType, string parentName, string listType)
    {
        var nodes = new ObjectFields();

        foreach (var field in fields)
        {
            if (nodes.ContainsKey(field.Name))
                throw ValidationException.ListEntryDuplicateName(location, parentType, parentName, listType, field.Name);
            else
                nodes.Add(field.Name, field);
        }

        return nodes;
    }

    public static TypeNode ConvertTypeNode(SyntaxTypeNode node)
    {
        return node switch
        {
            SyntaxTypeNameNode nameNode => new TypeName()
            {
                Name = nameNode.Name,
                Definition = null,
                Location = nameNode.Location,
            },
            SyntaxTypeNonNullNode nonNullNode => new TypeNonNull()
            {
                Type = ConvertTypeNode(nonNullNode.Type),
                Location = nonNullNode.Location,
            },
            SyntaxTypeListNode listNode => new TypeList()
            {
                Type = ConvertTypeNode(listNode.Type),
                Location = listNode.Location,
            },
            _ => throw ValidationException.UnrecognizedType(node.Location, node.GetType().Name)
        };
    }
}