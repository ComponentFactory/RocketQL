namespace RocketQL.Core.Visitors;

public interface ISyntaxNodeVisitors
{
    void Visit(IEnumerable<SyntaxNode> nodes)
    {
        foreach (var node in nodes)
        {
            switch (node)
            {
                case SyntaxSchemaDefinitionNode schema:
                    VisitSchemaDefinition(schema);
                    break;
                case SyntaxDirectiveDefinitionNode directive:
                    VisitDirectiveDefinition(directive);
                    break;
                case SyntaxScalarTypeDefinitionNode scalarType:
                    VisitScalarTypeDefinition(scalarType);
                    break;
                case SyntaxObjectTypeDefinitionNode objectType:
                    VisitObjectTypeDefinition(objectType);
                    break;
                case SyntaxInterfaceTypeDefinitionNode interfaceType:
                    VisitInterfaceTypeDefinition(interfaceType);
                    break;
                case SyntaxUnionTypeDefinitionNode unionType:
                    VisitUnionTypeDefinition(unionType);
                    break;
                case SyntaxEnumTypeDefinitionNode enumType:
                    VisitEnumTypeDefinition(enumType);
                    break;
                case SyntaxInputObjectTypeDefinitionNode inputObjectType:
                    VisitInputObjectTypeDefinition(inputObjectType);
                    break;
                default:
                    throw ValidationException.UnrecognizedType(node);
            }
        }
    }
    void VisitSchemaDefinition(SyntaxSchemaDefinitionNode schema);
    void VisitDirectiveDefinition(SyntaxDirectiveDefinitionNode directive);
    void VisitScalarTypeDefinition(SyntaxScalarTypeDefinitionNode scalarType);
    void VisitObjectTypeDefinition(SyntaxObjectTypeDefinitionNode objectType);
    void VisitInterfaceTypeDefinition(SyntaxInterfaceTypeDefinitionNode interfaceType);
    void VisitUnionTypeDefinition(SyntaxUnionTypeDefinitionNode unionType);
    void VisitEnumTypeDefinition(SyntaxEnumTypeDefinitionNode enumType);
    void VisitInputObjectTypeDefinition(SyntaxInputObjectTypeDefinitionNode inputObjectType);
}
