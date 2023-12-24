namespace RocketQL.Core.Visitors;

public interface ISchemaNodeVisitors
{
    void Visit(IEnumerable<SchemaNode> nodes)
    {
        foreach (var node in nodes)
        {
            switch (node)
            {
                case SchemaDefinition schemaDefinition:
                    VisitSchemaDefinition(schemaDefinition);
                    break;
                case DirectiveDefinition directiveDefinition:
                    VisitDirectiveDefinition(directiveDefinition);
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
    }

    void VisitSchemaDefinition(SchemaDefinition schemaDefinition);
    void VisitDirectiveDefinition(DirectiveDefinition directiveDefinition);
    void VisitScalarTypeDefinition(ScalarTypeDefinition scalarType);
    void VisitObjectTypeDefinition(ObjectTypeDefinition objectType);
    void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType);
    void VisitUnionTypeDefinition(UnionTypeDefinition unionType);
    void VisitEnumTypeDefinition(EnumTypeDefinition enumType);
    void VisitInputObjectTypeDefinition(InputObjectTypeDefinition inputObjectType);
}
