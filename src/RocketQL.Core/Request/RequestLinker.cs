using RocketQL.Core.Nodes;

namespace RocketQL.Core.Base;

public partial class Request
{
    private RequestLinker? _requestLinker = null;
    private RequestLinker Linker => _requestLinker ??= new RequestLinker(this);

    private class RequestLinker(Request request) : LinkerNodeVisitor, IDocumentNodeVisitors
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
            InterlinkDirectives(operation.Directives,operation, null, _request._schema.Directives);
            InterlinkVariables(operation.Variables, operation, operation);
            // TODO selection set
        }

        private void InterlinkVariables(VariableDefinitions variables, DocumentNode parentNode, DocumentNode rootNode)
        {
            foreach (var variable in variables.Values)
            {
                variable.Parent = parentNode;

                InterlinkDirectives(variable.Directives, variable, rootNode, _request._schema.Directives);
                InterlinkTypeNode(variable.Type, variable, rootNode, variable, _request._schema.Types);
            }
        }

        public void VisitFragmentDefinition(FragmentDefinition fragment)
        {
            InterlinkDirectives(fragment.Directives, fragment, null, _request._schema.Directives);

            if (!_request._schema.Types.TryGetValue(fragment.TypeCondition, out var type))
                FatalException(ValidationException.UndefinedTypeForFragment(fragment));
            else if ((type is not ObjectTypeDefinition) && (type is not InterfaceTypeDefinition) && (type is not UnionTypeDefinition))
                FatalException(ValidationException.FragmentTypeInvalid(fragment, type));

            fragment.Definition = type;
            // TODO selection set
        }

        public void VisitDirectiveDefinition(DirectiveDefinition directive)
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