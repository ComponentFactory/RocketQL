using RocketQL.Core.Nodes;

namespace RocketQL.Core.Base;

public partial class RequestBuilder
{
    private RequestBuilderValidator? _validator = null;
    private RequestBuilderValidator Validator => _validator ??= new RequestBuilderValidator(this);

    private class RequestBuilderValidator(RequestBuilder request) : NodePathTracker, IVisitDocumentNode
    {
        private readonly RequestBuilder _request = request;
        private ISchema _schema = Schema.Empty;

        public void Visit(ISchema schema)
        {
            _schema = schema;
            IVisitDocumentNode visitor = this;
            visitor.Visit(_request._operations.Values);
            visitor.Visit(_request._fragments.Values);
        }

        public void VisitOperationDefinition(OperationDefinition operation)
        {
            PushPath(operation);

            var operationDefinition = operation.Operation switch
            {
                OperationType.QUERY => _schema.Root.Query,
                OperationType.MUTATION => _schema.Root.Mutation,
                OperationType.SUBSCRIPTION => _schema.Root.Subscription,
                _ => null
            }; ;

            if (operationDefinition is null)
                _request.NonFatalException(ValidationException.OperationTypeNodeDefinedInSchema(operation, CurrentPath));
            else if (operationDefinition.Definition is null)
                _request.NonFatalException(ValidationException.TypeDefinitionMissing(operation, CurrentPath));
            else
                ValidateSelectionSet(operation.SelectionSet, operationDefinition.Definition!);

            PopPath();
        }

        private void ValidateSelectionSet(SelectionSet selectionSet, ObjectTypeDefinition typeDefinition)
        {
            foreach(var selection in selectionSet)
            {
                switch (selection)
                {
                    case SelectionField field:
                        {
                            PushPath(field);

                            if (!typeDefinition.Fields.TryGetValue(field.Name, out var fieldDefinition))
                                _request.NonFatalException(ValidationException.FieldNotDefinedOnObject(field, typeDefinition, CurrentPath));
                            else
                            {
                                var checkedArguments = field.Arguments.ToDictionary();
                                foreach (var argumentDefinition in fieldDefinition.Arguments.Values)
                                {
                                    PushPath(argumentDefinition);

                                    if (checkedArguments.TryGetValue(argumentDefinition.Name, out var checkedArgument))
                                    {
                                        // Handle variables

                                        if ((checkedArgument.Value is not null) && !TypeHelper.IsInputTypeCompatibleWithValue(_schema.Types, argumentDefinition.Type, checkedArgument.Value))
                                            _request.NonFatalException(ValidationException.ValueNotCompatibleInArgument(argumentDefinition, CurrentPath));
                                    }
                                    else if ((argumentDefinition.DefaultValue is null) && (argumentDefinition.Type is TypeNonNull))
                                        _request.NonFatalException(ValidationException.NodeMandatoryArgumentMissing(field, argumentDefinition.Name, CurrentPath));

                                    checkedArguments.Remove(argumentDefinition.Name);
                                    PopPath();
                                }

                                if (checkedArguments.Count > 0)
                                    foreach (var checkArgument in checkedArguments)
                                        _request.NonFatalException(ValidationException.NodeArgumentNotDefined(field, checkArgument.Key, CurrentPath));
                            }

                            PopPath();
                        }
                        break;
                }
            }
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
