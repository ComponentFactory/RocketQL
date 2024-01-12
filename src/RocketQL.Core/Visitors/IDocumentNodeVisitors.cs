namespace RocketQL.Core.Visitors;

public interface IDocumentNodeVisitors
{
    void Visit(IEnumerable<DocumentNode> nodes)
    {
        foreach (var node in nodes)
            Visit(node);
    }

    void Visit(DocumentNode node)
    {
        switch (node)
        {
            case SchemaRoot schemaRoot:
                VisitSchemaDefinition(schemaRoot);
                break;
            case SchemaDefinition schema:
                VisitSchemaDefinition(schema);
                break;
            case DirectiveDefinition directive:
                VisitDirectiveDefinition(directive);
                break;
            case ScalarTypeDefinition scalarType:
                VisitScalarTypeDefinition(scalarType);
                break;
            case ObjectTypeDefinition objectType:
                VisitObjectTypeDefinition(objectType);
                break;
            case InterfaceTypeDefinition interfaceType:
                VisitInterfaceTypeDefinition(interfaceType);
                break;
            case UnionTypeDefinition unionType:
                VisitUnionTypeDefinition(unionType);
                break;
            case EnumTypeDefinition enumType:
                VisitEnumTypeDefinition(enumType);
                break;
            case InputObjectTypeDefinition inputObjectType:
                VisitInputObjectTypeDefinition(inputObjectType);
                break;
            default:
                throw ValidationException.UnrecognizedType(node);
        }
    }

    void VisitSchemaDefinition(SchemaRoot schemaRoot);
    void VisitSchemaDefinition(SchemaDefinition schema);
    void VisitDirectiveDefinition(DirectiveDefinition directive);
    void VisitScalarTypeDefinition(ScalarTypeDefinition scalarType);
    void VisitObjectTypeDefinition(ObjectTypeDefinition objectType);
    void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType);
    void VisitUnionTypeDefinition(UnionTypeDefinition unionType);
    void VisitEnumTypeDefinition(EnumTypeDefinition enumType);
    void VisitInputObjectTypeDefinition(InputObjectTypeDefinition inputObjectType);
}
