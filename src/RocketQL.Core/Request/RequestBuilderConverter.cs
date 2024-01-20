namespace RocketQL.Core.Base;

public partial class RequestBuilder
{
    private RequestBuilderConverter? _converter = null;
    private RequestBuilderConverter Converter => _converter ??= new RequestBuilderConverter(this);

    private class RequestBuilderConverter(RequestBuilder request) : NodePathTracker, ISyntaxNodeVisitors
    {
        private readonly RequestBuilder _request = request;
        private ISchema _schema = Schema.Empty;

        public void Visit(ISchema schema)
        {
            _schema = schema;
            ((ISyntaxNodeVisitors)this).Visit(_request._nodes);
        }

        public void VisitOperationDefinition(SyntaxOperationDefinitionNode operation)
        {
            var operationName = string.IsNullOrEmpty(operation.Name) ? "(anon)" : operation.Name;
            PushPath($"{operation.Operation.ToString().ToLower()} {operationName}");

            if (_request._operations.ContainsKey(operation.Name))
            {
                if (string.IsNullOrEmpty(operation.Name))
                    _request.NonFatalException(ValidationException.RequestAnonymousAlreadyDefined(operation, CurrentPath));
                else
                    _request.NonFatalException(ValidationException.RequestOperationAlreadyDefined(operation, CurrentPath));
            }
            else
            {
                if ((string.IsNullOrEmpty(operation.Name) && (_request._operations.Count > 0)) ||
                    (!string.IsNullOrEmpty(operation.Name) && _request._operations.ContainsKey("")))
                {
                    _request.NonFatalException(ValidationException.RequestAnonymousAndNamed(operation, CurrentPath));
                }

                _request._operations.Add(operation.Name, new(operation.Operation,
                                                             operation.Name,
                                                             ConvertDirectives(operation.Directives),
                                                             ConvertVariableDefinitions(operation.VariableDefinitions),
                                                             ConvertSelectionSet(operation.SelectionSet),
                                                             operation.Location));
            }

            PopPath();
        }

        public void VisitFragmentDefinition(SyntaxFragmentDefinitionNode fragment)
        {
            PushPath(fragment);

            if (_request._fragments.ContainsKey(fragment.Name))
                _request.NonFatalException(ValidationException.TypeNameAlreadyDefined(fragment, fragment.OutputElement(), fragment.Name, CurrentPath));
            else
            {
                _request._fragments.Add(fragment.Name, new(fragment.Name,
                                                           fragment.TypeCondition,
                                                           ConvertDirectives(fragment.Directives),
                                                           ConvertSelectionSet(fragment.SelectionSet),
                                                           fragment.Location));
            }

            PopPath();
        }

        public void VisitSchemaDefinition(SyntaxSchemaDefinitionNode schema)
        {
            _request.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(schema, "Schema"));
        }

        public void VisitDirectiveDefinition(SyntaxDirectiveDefinitionNode directive)
        {
            _request.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(directive, "Directive"));
        }

        public void VisitScalarTypeDefinition(SyntaxScalarTypeDefinitionNode scalarType)
        {
            _request.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(scalarType, "Scalar"));
        }

        public void VisitObjectTypeDefinition(SyntaxObjectTypeDefinitionNode objectType)
        {
            _request.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(objectType, "Type"));
        }

        public void VisitInterfaceTypeDefinition(SyntaxInterfaceTypeDefinitionNode interfaceType)
        {
            _request.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(interfaceType, "Interface"));
        }

        public void VisitUnionTypeDefinition(SyntaxUnionTypeDefinitionNode unionType)
        {
            _request.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(unionType, "Union"));
        }

        public void VisitEnumTypeDefinition(SyntaxEnumTypeDefinitionNode enumType)
        {
            _request.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(enumType, "Enum"));
        }

        public void VisitInputObjectTypeDefinition(SyntaxInputObjectTypeDefinitionNode inputObjectType)
        {
            _request.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(inputObjectType, "Input object"));
        }

        public void VisitExtendSchemaDefinition(SyntaxExtendSchemaDefinitionNode extendSchema)
        {
            _request.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(extendSchema, "Extend schema"));
        }

        public void VisitExtendScalarTypeDefinition(SyntaxExtendScalarTypeDefinitionNode extendScalarType)
        {
            _request.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(extendScalarType, "Extend scalar"));
        }

        public void VisitExtendObjectTypeDefinition(SyntaxExtendObjectTypeDefinitionNode extendObjectType)
        {
            _request.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(extendObjectType, "Extend object"));
        }

        public void VisitExtendInterfaceTypeDefinition(SyntaxExtendInterfaceTypeDefinitionNode extendInterfaceType)
        {
            _request.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(extendInterfaceType, "Extend interface"));
        }

        public void VisitExtendUnionTypeDefinition(SyntaxExtendUnionTypeDefinitionNode extendUnionType)
        {
            _request.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(extendUnionType, "Extend union"));
        }

        public void VisitExtendEnumDefinition(SyntaxExtendEnumTypeDefinitionNode extendEnumType)
        {
            _request.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(extendEnumType, "Extend enum"));
        }

        public void VisitExtendInputObjectTypeDefinition(SyntaxExtendInputObjectTypeDefinitionNode extendInputObjectType)
        {
            _request.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(extendInputObjectType, "Extend input object"));
        }

        private VariableDefinitions ConvertVariableDefinitions(SyntaxVariableDefinitionNodeList variables)
        {
            var nodes = new VariableDefinitions();

            foreach (var variable in variables)
            {
                PushPath(variable);

                if (nodes.ContainsKey(variable.Name))
                    _request.NonFatalException(ValidationException.DuplicateName(variable, "variable", variable.Name, CurrentPath));
                else
                {
                    nodes.Add(variable.Name, new VariableDefinition(variable.Name,
                                                                    ConvertTypeNode(variable.Type),
                                                                    variable.DefaultValue,
                                                                    ConvertDirectives(variable.Directives),
                                                                    variable.Location));
                }

                PopPath();
            }

            return nodes;
        }

        private SelectionSet ConvertSelectionSet(SyntaxSelectionDefinitionNodeList selections)
        {
            var set = new SelectionSet();

            foreach (var selection in selections)
            {
                set.Add(selection switch
                {
                    SyntaxFieldSelectionNode fieldSelection => ConvertFieldSelection(fieldSelection),
                    SyntaxFragmentSpreadSelectionNode fragmentSpread => ConvertFragmentSpreadSelection(fragmentSpread),
                    SyntaxInlineFragmentSelectionNode inlineFragment => ConvertInlineFragmentSelection(inlineFragment),
                    _ => throw ValidationException.UnrecognizedType(selection, CurrentPath)
                });
            }

            return set;
        }

        private SelectionField ConvertFieldSelection(SyntaxFieldSelectionNode fieldSelection)
        {
            PushPath(fieldSelection);

            var ret = new SelectionField(fieldSelection.Alias,
                                         fieldSelection.Name,
                                         ConvertDirectives(fieldSelection.Directives),
                                         ConvertObjectFields(fieldSelection, fieldSelection.Arguments),
                                         ConvertSelectionSet(fieldSelection.SelectionSet),
                                         fieldSelection.Location);

            PopPath();
            return ret;
        }

        private SelectionFragmentSpread ConvertFragmentSpreadSelection(SyntaxFragmentSpreadSelectionNode fragmentSpread)
        {
            PushPath(fragmentSpread);

            var ret = new SelectionFragmentSpread(fragmentSpread.Name, ConvertDirectives(fragmentSpread.Directives), fragmentSpread.Location);

            PopPath();
            return ret;
        }

        private SelectionInlineFragment ConvertInlineFragmentSelection(SyntaxInlineFragmentSelectionNode inlineFragment)
        {
            PushPath(inlineFragment);

            var ret = new SelectionInlineFragment(inlineFragment.TypeCondition,
                                                  ConvertDirectives(inlineFragment.Directives),
                                                  ConvertSelectionSet(inlineFragment.SelectionSet),
                                                  inlineFragment.Location);

            PopPath();
            return ret;
        }

        private Directives ConvertDirectives(SyntaxDirectiveNodeList directives)
        {
            var nodes = new Directives();

            foreach (var directive in directives)
                nodes.Add(new(directive.Name, ConvertObjectFields(directive, directive.Arguments), directive.Location));

            return nodes;
        }

        private ObjectFields ConvertObjectFields(LocationNode parentNode, SyntaxObjectFieldNodeList fields)
        {
            var nodes = new ObjectFields();

            foreach (var field in fields)
            {
                PushPath(field);

                if (nodes.ContainsKey(field.Name))
                    throw ValidationException.DuplicateName(parentNode, "argument", field.Name, CurrentPath);
                else
                    nodes.Add(field.Name, field);

                PopPath();
            }

            return nodes;
        }

        private TypeNode ConvertTypeNode(SyntaxTypeNode node)
        {
            return node switch
            {
                SyntaxTypeNameNode nameNode => new TypeName(nameNode.Name, nameNode.Location),
                SyntaxTypeNonNullNode nonNullNode => new TypeNonNull(ConvertTypeNode(nonNullNode.Type), nonNullNode.Location),
                SyntaxTypeListNode listNode => new TypeList(ConvertTypeNode(listNode.Type), listNode.Location),
                _ => throw ValidationException.UnrecognizedType(node.Location, node.GetType().Name, CurrentPath)
            };
        }
    }
}
