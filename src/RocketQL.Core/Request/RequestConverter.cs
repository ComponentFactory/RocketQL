namespace RocketQL.Core.Base;

public partial class Request
{
    private RequestConverter? _requestConverter = null;
    private RequestConverter Converter => _requestConverter ??= new RequestConverter(this);

    private class RequestConverter(Request request) : ISyntaxNodeVisitors
    {
        private readonly Request _request = request;

        public void Visit()
        {
            ((ISyntaxNodeVisitors)this).Visit(_request._nodes);
        }

        public void VisitSchemaDefinition(SyntaxSchemaDefinitionNode schema) { }
        public void VisitDirectiveDefinition(SyntaxDirectiveDefinitionNode directive) { }
        public void VisitScalarTypeDefinition(SyntaxScalarTypeDefinitionNode scalarType) { }
        public void VisitObjectTypeDefinition(SyntaxObjectTypeDefinitionNode objectType) { }
        public void VisitInterfaceTypeDefinition(SyntaxInterfaceTypeDefinitionNode interfaceType) { }
        public void VisitUnionTypeDefinition(SyntaxUnionTypeDefinitionNode unionType) { }
        public void VisitEnumTypeDefinition(SyntaxEnumTypeDefinitionNode enumType) { }
        public void VisitInputObjectTypeDefinition(SyntaxInputObjectTypeDefinitionNode inputObjectType) { }
        public void VisitExtendSchemaDefinition(SyntaxExtendSchemaDefinitionNode extendSchema) { }
        public void VisitExtendScalarTypeDefinition(SyntaxExtendScalarTypeDefinitionNode extendScalarType) { }
        public void VisitExtendObjectTypeDefinition(SyntaxExtendObjectTypeDefinitionNode extendObjectType) { }
        public void VisitExtendInterfaceTypeDefinition(SyntaxExtendInterfaceTypeDefinitionNode extendInterfaceType) { }
        public void VisitExtendUnionTypeDefinition(SyntaxExtendUnionTypeDefinitionNode extendUnionType) { }
        public void VisitExtendEnumDefinition(SyntaxExtendEnumTypeDefinitionNode extendEnumType) { }
        public void VisitExtendInputObjectTypeDefinition(SyntaxExtendInputObjectTypeDefinitionNode extendInputObjectType) { }
    }
}