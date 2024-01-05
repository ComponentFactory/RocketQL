using RocketQL.Core.Base;

namespace RocketQL.Core.Visitors;

public interface ISyntaxNodeVisitors
{
    void Visit(IEnumerable<SyntaxNode> nodes)
    {
        foreach (var node in nodes)
            Visit(node);
    }

    void Visit(SyntaxNode node)
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
            case SyntaxExtendSchemaDefinitionNode extendSchema:
                VisitExtendSchemaDefinition(extendSchema);
                break;
            case SyntaxExtendScalarTypeDefinitionNode extendScalarType:
                VisitExtendScalarTypeDefinition(extendScalarType);
                break;
            case SyntaxExtendObjectTypeDefinitionNode extendObjectType:
                VisitExtendObjectTypeDefinition(extendObjectType);
                break;
            case SyntaxExtendInterfaceTypeDefinitionNode extendInterfaceType:
                VisitExtendInterfaceTypeDefinition(extendInterfaceType);
                break;
            case SyntaxExtendUnionTypeDefinitionNode extendUnionType:
                VisitExtendUnionTypeDefinition(extendUnionType);
                break;
            case SyntaxExtendEnumTypeDefinitionNode extendEnumType:
                VisitExtendEnumDefinition(extendEnumType);
                break;
            case SyntaxExtendInputObjectTypeDefinitionNode extendInputObjectType:
                VisitExtendInputObjectTypeDefinition(extendInputObjectType);
                break;
            default:
                throw ValidationException.UnrecognizedType(node.Location, node.GetType().ToString());
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
    void VisitExtendSchemaDefinition(SyntaxExtendSchemaDefinitionNode extendSchema);
    void VisitExtendScalarTypeDefinition(SyntaxExtendScalarTypeDefinitionNode extendScalarType);
    void VisitExtendObjectTypeDefinition(SyntaxExtendObjectTypeDefinitionNode extendObjectType);
    void VisitExtendInterfaceTypeDefinition(SyntaxExtendInterfaceTypeDefinitionNode extendInterfaceType);
    void VisitExtendUnionTypeDefinition(SyntaxExtendUnionTypeDefinitionNode extendUnionType);
    void VisitExtendEnumDefinition(SyntaxExtendEnumTypeDefinitionNode extendEnumType);
    void VisitExtendInputObjectTypeDefinition(SyntaxExtendInputObjectTypeDefinitionNode extendInputObjectType);
}
 