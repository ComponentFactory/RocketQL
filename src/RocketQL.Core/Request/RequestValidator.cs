using RocketQL.Core.Nodes;

namespace RocketQL.Core.Base;

public partial class Request
{
    private RequestValidator? _requestValidator = null;
    private RequestValidator Validator => _requestValidator ??= new RequestValidator(this);

    private class RequestValidator(Request request) : NodeVisitor, IDocumentNodeVisitors
    {
        private readonly Request _request = request;

        public void Visit()
        {
            IDocumentNodeVisitors visitor = this;
            visitor.Visit(_request._operations.Values);
            visitor.Visit(_request._fragments.Values);
        }

        public void VisitOperationDefinition(OperationDefinition operation)
        {
        }

        public void VisitFragmentDefinition(FragmentDefinition fragment)
        {
        }

        public void VisitDirectiveDefinition(DirectiveDefinition directiveDefinition)
        {
        }

        public void VisitScalarTypeDefinition(ScalarTypeDefinition scalarType)
        {
        }

        public void VisitObjectTypeDefinition(ObjectTypeDefinition objectType)
        {
        }

        public void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType)
        {
        }

        public void VisitUnionTypeDefinition(UnionTypeDefinition unionType)
        {
        }

        public void VisitEnumTypeDefinition(EnumTypeDefinition enumType)
        {
        }

        public void VisitInputObjectTypeDefinition(InputObjectTypeDefinition inputObjectType)
        {
        }

        public void VisitSchemaDefinition(SchemaRoot schemaRoot)
        {
        }

        public void VisitSchemaDefinition(SchemaDefinition schemaDefinition)
        {
        }
    }
}
