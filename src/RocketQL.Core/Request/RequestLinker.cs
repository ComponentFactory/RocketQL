using RocketQL.Core.Enumerations;
using RocketQL.Core.Nodes;

namespace RocketQL.Core.Base;

public partial class Request
{
    private RequestLinker? _requestLinker = null;
    private RequestLinker Linker => _requestLinker ??= new RequestLinker(this);

    private class RequestLinker(Request request) : NodeVisitor, IDocumentNodeVisitors
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
            PushPath(operation);
            InterlinkDirectives(operation.Directives, operation);
            InterlinkVariables(operation.Variables, operation);
            InterlinkSelectionSet(operation.SelectionSet, operation, operation);
            PopPath();
        }

        public void VisitFragmentDefinition(FragmentDefinition fragment)
        {
            PushPath(fragment);

            if (!_request._schema.Types.TryGetValue(fragment.TypeCondition, out var type))
                _request.NonFatalException(ValidationException.UndefinedTypeForFragment(fragment, CurrentPath));
            else if ((type is not ObjectTypeDefinition) && (type is not InterfaceTypeDefinition) && (type is not UnionTypeDefinition))
                _request.NonFatalException(ValidationException.FragmentTypeInvalid(fragment, type, CurrentPath));

            fragment.Definition = type;
            InterlinkDirectives(fragment.Directives, fragment);
            InterlinkSelectionSet(fragment.SelectionSet, fragment, fragment);
            PopPath();
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

        private void InterlinkVariables(VariableDefinitions variables, DocumentNode parentNode)
        {
            foreach (var variable in variables.Values)
            {
                PushPath(variable);
                variable.Parent = parentNode;
                InterlinkDirectives(variable.Directives, variable);
                InterlinkTypeNode(variable.Type, variable);
                PopPath();
            }
        }
        private void InterlinkSelectionSet(SelectionSet selectionSet, DocumentNode parentNode, DocumentNode rootNode)
        {
            foreach (var selection in selectionSet)
            {
                selection.Parent = parentNode;

                switch (selection)
                {
                    case SelectionField field:
                        {
                            PushPath(field);
                            InterlinkDirectives(field.Directives, field);
                            InterlinkSelectionSet(field.SelectionSet, field, rootNode);
                            PopPath();
                        }
                        break;
                    case SelectionFragmentSpread fragmentSpread:
                        {
                            PushPath(fragmentSpread);

                            if (!_request._fragments.TryGetValue(fragmentSpread.Name, out var fragmentType))
                                _request.NonFatalException(ValidationException.UndefinedTypeForFragmentSpread(fragmentSpread, rootNode, CurrentPath));

                            fragmentSpread.Definition = fragmentType;
                            InterlinkDirectives(fragmentSpread.Directives, fragmentSpread);
                            PopPath();
                        }
                        break;
                    case SelectionInlineFragment inlineFragment:
                        {
                            PushPath(inlineFragment);

                            if (!_request._schema.Types.TryGetValue(inlineFragment.TypeCondition, out var _))
                                _request.NonFatalException(ValidationException.UndefinedTypeForInlineFragment(inlineFragment, rootNode, CurrentPath));

                            InterlinkDirectives(inlineFragment.Directives, inlineFragment);
                            InterlinkSelectionSet(inlineFragment.SelectionSet, inlineFragment, rootNode);
                            PopPath();
                        }
                        break;
                }
            }
        }

        private void InterlinkDirectives(Directives directives, DocumentNode parentNode)
        {
            foreach (var directive in directives)
            {
                PushPath(directive);
                directive.Parent = parentNode;

                if (!_request._schema.Directives.TryGetValue(directive.Name, out var directiveDefinition))
                    _request.NonFatalException(ValidationException.UndefinedDirective(directive, parentNode, CurrentPath));
                else
                {
                    directive.Definition = directiveDefinition;
                    directiveDefinition.References.Add(directive!);
                }

                PopPath();
            }
        }

        private void InterlinkTypeNode(TypeNode typeLocation, DocumentNode typeParentNode)
        {
            typeLocation.Parent = typeParentNode;

            if (typeLocation is TypeList typeList)
                InterlinkTypeNode(typeList.Type, typeList);
            if (typeLocation is TypeNonNull typeNonNull)
                InterlinkTypeNode(typeNonNull.Type, typeNonNull);
            else if (typeLocation is TypeName typeName)
            {
                if (!_request._schema.Types.TryGetValue(typeName.Name, out var type))
                    _request.NonFatalException(ValidationException.UndefinedTypeForListEntry(typeName, typeName.Name, typeParentNode, CurrentPath));
                else
                {
                    typeName.Definition = type;
                    type.References.Add(typeName);
                }
            }
        }
    }
}
