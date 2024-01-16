namespace RocketQL.Core.Visitors;

public abstract class LinkerNodeVisitor
{
    public static void InterlinkDirectives(Directives directives,
                                           DocumentNode parentNode,
                                           DocumentNode? grandParentNode,
                                           IReadOnlyDictionary<string, DirectiveDefinition> directiveDefinitions)
    {
        foreach (var directive in directives)
        {
            directive.Parent = parentNode;

            if (!directiveDefinitions.TryGetValue(directive.Name, out DirectiveDefinition? directiveDefinition))
            {
                if (grandParentNode is not null)
                    throw ValidationException.UndefinedDirective(directive, parentNode.OutputElement, parentNode.OutputName, grandParentNode);
                else
                    throw ValidationException.UndefinedDirective(directive, parentNode);
            }
            else
            {
                directive.Definition = directiveDefinition;
                directiveDefinition.References.Add(directive!);
            }
        }
    }
    public static void InterlinkTypeNode(TypeNode typeLocation,
                                         DocumentNode parentNode,
                                         DocumentNode rootNode,
                                         DocumentNode typeParentNode,
                                         IReadOnlyDictionary<string, TypeDefinition> typeDefinitions)
    {
        typeLocation.Parent = typeParentNode;

        if (typeLocation is TypeList typeList)
            InterlinkTypeNode(typeList.Type, parentNode, rootNode, typeList, typeDefinitions);
        if (typeLocation is TypeNonNull typeNonNull)
            InterlinkTypeNode(typeNonNull.Type, parentNode, rootNode, typeNonNull, typeDefinitions);
        else if (typeLocation is TypeName typeName)
        {
            if (!typeDefinitions.TryGetValue(typeName.Name, out var type))
            {
                throw ValidationException.UndefinedTypeForListEntry(
                    typeName.Location, 
                    typeName.Name, 
                    parentNode.OutputElement, 
                    parentNode.OutputName, 
                    rootNode);
            }
            else
            {
                typeName.Definition = type;
                type.References.Add(typeName);
            }
        }
    }
}