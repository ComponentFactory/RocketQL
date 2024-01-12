using RocketQL.Core.Nodes;

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

        public void VisitOperationDefinition(SyntaxOperationDefinitionNode operation)
        {
            var operationName = operation.Name ?? "";

            if (_request._operations.ContainsKey(operationName))
            {
                if (string.IsNullOrEmpty(operation.Name))
                    FatalException(ValidationException.RequestDefaultAlreadyDefined(operation.Location));
                else
                    FatalException(ValidationException.RequestOperationAlreadyDefined(operation.Location, operation.Name));
            }
            else
            {
                if ((string.IsNullOrEmpty(operation.Name) && (_request._operations.Count > 0)) ||
                    (!string.IsNullOrEmpty(operation.Name) && _request._operations.ContainsKey("")))
                    FatalException(ValidationException.RequestDefaultAlreadyDefined(operation.Location));

                _request._operations.Add(operationName, new()
                {
                    Operation = operation.Operation,
                    Name = operationName,
                    Directives = operation.Directives.ConvertDirectives(),
                    Variables = [],
                    SelectionSet = [],
                    Location = operation.Location,
                });
            }
        }

        public void VisitFragmentDefinition(SyntaxFragmentDefinitionNode fragment)
        {
        }

        public void VisitSchemaDefinition(SyntaxSchemaDefinitionNode schema)
        {
            _request.NonFatalException(ValidationException.SchemaDefinitionIgnored(schema.Location, "Schema"));
        }

        public void VisitDirectiveDefinition(SyntaxDirectiveDefinitionNode directive)
        {
            _request.NonFatalException(ValidationException.SchemaDefinitionIgnored(directive.Location, "Directive"));
        }

        public void VisitScalarTypeDefinition(SyntaxScalarTypeDefinitionNode scalarType)
        {
            _request.NonFatalException(ValidationException.SchemaDefinitionIgnored(scalarType.Location, "Scalar"));
        }

        public void VisitObjectTypeDefinition(SyntaxObjectTypeDefinitionNode objectType)
        {
            _request.NonFatalException(ValidationException.SchemaDefinitionIgnored(objectType.Location, "Object"));
        }

        public void VisitInterfaceTypeDefinition(SyntaxInterfaceTypeDefinitionNode interfaceType)
        {
            _request.NonFatalException(ValidationException.SchemaDefinitionIgnored(interfaceType.Location, "Interface"));
        }

        public void VisitUnionTypeDefinition(SyntaxUnionTypeDefinitionNode unionType)
        {
            _request.NonFatalException(ValidationException.SchemaDefinitionIgnored(unionType.Location, "Union"));
        }

        public void VisitEnumTypeDefinition(SyntaxEnumTypeDefinitionNode enumType)
        {
            _request.NonFatalException(ValidationException.SchemaDefinitionIgnored(enumType.Location, "Enum"));
        }

        public void VisitInputObjectTypeDefinition(SyntaxInputObjectTypeDefinitionNode inputObjectType)
        {
            _request.NonFatalException(ValidationException.SchemaDefinitionIgnored(inputObjectType.Location, "Input object"));
        }

        public void VisitExtendSchemaDefinition(SyntaxExtendSchemaDefinitionNode extendSchema)
        {
            _request.NonFatalException(ValidationException.SchemaDefinitionIgnored(extendSchema.Location, "Extend schema"));
        }

        public void VisitExtendScalarTypeDefinition(SyntaxExtendScalarTypeDefinitionNode extendScalarType)
        {
            _request.NonFatalException(ValidationException.SchemaDefinitionIgnored(extendScalarType.Location, "Extend scalar"));
        }

        public void VisitExtendObjectTypeDefinition(SyntaxExtendObjectTypeDefinitionNode extendObjectType)
        {
            _request.NonFatalException(ValidationException.SchemaDefinitionIgnored(extendObjectType.Location, "Extend object"));
        }

        public void VisitExtendInterfaceTypeDefinition(SyntaxExtendInterfaceTypeDefinitionNode extendInterfaceType)
        {
            _request.NonFatalException(ValidationException.SchemaDefinitionIgnored(extendInterfaceType.Location, "Extend interface"));
        }

        public void VisitExtendUnionTypeDefinition(SyntaxExtendUnionTypeDefinitionNode extendUnionType)
        {
            _request.NonFatalException(ValidationException.SchemaDefinitionIgnored(extendUnionType.Location, "Extend union"));
        }

        public void VisitExtendEnumDefinition(SyntaxExtendEnumTypeDefinitionNode extendEnumType)
        {
            _request.NonFatalException(ValidationException.SchemaDefinitionIgnored(extendEnumType.Location, "Extend enum"));
        }

        public void VisitExtendInputObjectTypeDefinition(SyntaxExtendInputObjectTypeDefinitionNode extendInputObjectType)
        {
            _request.NonFatalException(ValidationException.SchemaDefinitionIgnored(extendInputObjectType.Location, "Extend input object"));
        }
    }
}