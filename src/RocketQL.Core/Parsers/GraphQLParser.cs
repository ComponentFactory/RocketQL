namespace RocketQL.Core;

public ref struct GraphQLParser
{
    private GraphQLTokenizer _tokenizer;
    private string? _description = null;

    public GraphQLParser(ReadOnlySpan<char> text)
    {
        _tokenizer = new GraphQLTokenizer(text);
    }

    public DocumentNode Parse()
    {
        OperationDefinitionNodeList _operations = new();
        FragmentDefinitionNodeList _fragments = new();
        DirectiveDefinitionNodeList _directives = new();
        SchemaDefinitionNodeList _schemas = new();                      ExtendSchemaDefinitionNodeList _extendSchemas = new();
        ScalarTypeDefinitionNodeList _scalarTypes = new();              ExtendScalarTypeDefinitionNodeList _extendScalarTypes = new();
        ObjectTypeDefinitionNodeList _objectTypes = new();              ExtendObjectTypeDefinitionNodeList _extendObjectTypes = new();
        InterfaceTypeDefinitionNodeList _interfaceTypes = new();        ExtendInterfaceTypeDefinitionNodeList _extendInterfaceTypes = new();
        UnionTypeDefinitionNodeList _unionTypes = new();                ExtendUnionTypeDefinitionNodeList _extendUnionTypes = new();
        EnumTypeDefinitionNodeList _enumTypes = new();                  ExtendEnumTypeDefinitionNodeList _extendEnumTypes = new();
        InputObjectTypeDefinitionNodeList _inputObjectTypes = new();    ExtendInputObjectTypeDefinitionNodeList _extendInputObjectTypes = new();

        // Move to the first real token
        _tokenizer.Next();

        // Keep processing until we reach the end of the tokens or throw because of an exception
        while(_tokenizer.TokenKind != GraphQLTokenKind.EndOfText)
        {
            switch(_tokenizer.TokenKind) 
            {
                case GraphQLTokenKind.StringValue:
                    _description = _tokenizer.TokenString;
                    _tokenizer.Next();
                    break;
                case GraphQLTokenKind.Name:
                    switch (_tokenizer.TokenValue)
                    {
                        case "query":
                        case "mutation":
                        case "subscription":
                            _operations.Add(ParseOperationDefinition());
                            break;
                        case "fragment":
                            _fragments.Add(ParseFragmentDefinition());
                            break;
                        case "schema":
                            _schemas.Add(ParseSchemaDefinition());
                            break;
                        case "scalar":
                            _scalarTypes.Add(ParseScalarTypeDefinition());
                            break;
                        case "type":
                            _objectTypes.Add(ParseObjectTypeDefinition());
                            break;
                        case "interface":
                            _interfaceTypes.Add(ParseInterfaceTypeDefinition());
                            break;
                        case "union":
                            _unionTypes.Add(ParseUnionTypeDefinition());
                            break;
                        case "enum":
                            _enumTypes.Add(ParseEnumTypeDefinition());
                            break;
                        case "input":
                            _inputObjectTypes.Add(ParseInputObjectTypeDefinition());
                            break;
                        case "directive":
                            _directives.Add(ParseDirectiveDefinition());
                            break;
                        case "extend":
                            {
                                MandatoryNextToken(GraphQLTokenKind.Name);
                                switch (_tokenizer.TokenValue)
                                {
                                    case "schema":
                                        _extendSchemas.Add(ParseExtendSchemaDefinition());
                                        break;
                                    case "scalar":
                                        _extendScalarTypes.Add(ParseExtendScalarTypeDefinition());
                                        break;
                                    case "type":
                                        _extendObjectTypes.Add(ParseExtendObjectTypeDefinition());
                                        break;
                                    case "interface":
                                        _extendInterfaceTypes.Add(ParseExtendInterfaceTypeDefinition());
                                        break;
                                    case "union":
                                        _extendUnionTypes.Add(ParseExtendUnionTypeDefinition());
                                        break;
                                    case "enum":
                                        _extendEnumTypes.Add(ParseExtendEnumTypeDefinition());
                                        break;
                                    case "input":
                                        _extendInputObjectTypes.Add(ParseExtendInputObjectTypeDefinition());
                                        break;
                                    default:
                                        throw SyntaxException.UnrecognizedKeyword(_tokenizer.Location, _tokenizer.TokenValue);
                                }
                            }
                            break;
                        default:
                            throw SyntaxException.UnrecognizedKeyword(_tokenizer.Location, _tokenizer.TokenValue);
                    }
                    break;
                case GraphQLTokenKind.LeftCurlyBracket:
                    _operations.Add(new OperationDefinitionNode(OperationType.QUERY, string.Empty, new(), new(), ParseSelectionSet()));
                    break;
                default:
                    throw SyntaxException.UnrecognizedToken(_tokenizer.Location, _tokenizer.TokenKind);
            }
        }

        return new DocumentNode(_operations, _fragments, _directives,
                                _schemas, _extendSchemas,
                                _scalarTypes, _extendScalarTypes,
                                _objectTypes, _extendObjectTypes,
                                _interfaceTypes, _extendInterfaceTypes,
                                _unionTypes, _extendUnionTypes,
                                _enumTypes, _extendEnumTypes,
                                _inputObjectTypes, _extendInputObjectTypes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private OperationDefinitionNode ParseOperationDefinition()
    {
        OperationType operationType = OperationTypeFromTokenValue();
        MandatoryNext();

        string name = string.Empty;
        if (_tokenizer.TokenKind == GraphQLTokenKind.Name)
        {
            name = _tokenizer.TokenValue;
            MandatoryNext();
        }

        return new OperationDefinitionNode(operationType, 
                                           name, 
                                           ParseVariablesOptionalDefinition(), 
                                           ParseDirectivesOptional(), 
                                           ParseSelectionSet());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private FragmentDefinitionNode ParseFragmentDefinition()
    {
        MandatoryNextToken(GraphQLTokenKind.Name);
        string name = _tokenizer.TokenValue;
        if (name == "on")
            throw SyntaxException.FragmentNameCannotBeOn(_tokenizer.Location);

        MandatoryNext();
        MandatoryKeyword("on");
        MandatoryNextToken(GraphQLTokenKind.Name);
        string typeCondition = _tokenizer.TokenValue;
        MandatoryNext();

        return new FragmentDefinitionNode(name, typeCondition, ParseDirectivesOptional(), ParseSelectionSet());
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SchemaDefinitionNode ParseSchemaDefinition()
    {
        MandatoryNext();
        var directives = ParseDirectivesOptional();
        MandatoryTokenNext(GraphQLTokenKind.LeftCurlyBracket);

        OperationTypeDefinitionNodeList list = new();

        do
        {
            MandatoryToken(GraphQLTokenKind.Name);
            OperationType operationType = OperationTypeFromTokenValue();
            MandatoryNextToken(GraphQLTokenKind.Colon);
            MandatoryNextToken(GraphQLTokenKind.Name);
            string namedType = _tokenizer.TokenValue;
            MandatoryNext();

            list.Add(new OperationTypeDefinitionNode(operationType, namedType));

        } while (_tokenizer.TokenKind != GraphQLTokenKind.RightCurlyBracket);
        
        _tokenizer.Next();
        return new SchemaDefinitionNode(UseTopLevelDescription(), directives, list);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ExtendSchemaDefinitionNode ParseExtendSchemaDefinition()
    {
        MandatoryNext();
        var directives = ParseDirectivesOptional();
        
        OperationTypeDefinitionNodeList list = new();

        if (_tokenizer.TokenKind == GraphQLTokenKind.LeftCurlyBracket)
        {
            MandatoryNext();

            do
            {
                MandatoryToken(GraphQLTokenKind.Name);
                OperationType operationType = OperationTypeFromTokenValue();
                MandatoryNextToken(GraphQLTokenKind.Colon);
                MandatoryNextToken(GraphQLTokenKind.Name);
                string namedType = _tokenizer.TokenValue;
                MandatoryNext();

                list.Add(new OperationTypeDefinitionNode(operationType, namedType));

            } while (_tokenizer.TokenKind != GraphQLTokenKind.RightCurlyBracket);

            _tokenizer.Next();
        }
        else if (directives.Count == 0)
            throw SyntaxException.ExtendSchemaMissingAtLeastOne(_tokenizer.Location);

        return new ExtendSchemaDefinitionNode(directives, list);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ScalarTypeDefinitionNode ParseScalarTypeDefinition()
    {
        MandatoryNextToken(GraphQLTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        return new ScalarTypeDefinitionNode(UseTopLevelDescription(), name, ParseDirectivesOptional());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ExtendScalarTypeDefinitionNode ParseExtendScalarTypeDefinition()
    {
        MandatoryNextToken(GraphQLTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        return new ExtendScalarTypeDefinitionNode(name, ParseDirectivesOptional());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObjectTypeDefinitionNode ParseObjectTypeDefinition()
    {
        MandatoryNextToken(GraphQLTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        return new ObjectTypeDefinitionNode(UseTopLevelDescription(),
                                            name,
                                            ParseImplementsInterfacesOptional(),
                                            ParseDirectivesOptional(),
                                            ParseFieldsOptionalDefinition());
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ExtendObjectTypeDefinitionNode ParseExtendObjectTypeDefinition()
    {
        MandatoryNextToken(GraphQLTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();
        var implementsIntefaces = ParseImplementsInterfacesOptional();
        var directives = ParseDirectivesOptional();
        var fieldSet = ParseFieldsOptionalDefinition();

        if ((implementsIntefaces.Count == 0) && (directives.Count == 0) && (fieldSet.Count == 0))
            throw SyntaxException.ExtendObjectTypeMissingAtLeastOne(_tokenizer.Location);

        return new ExtendObjectTypeDefinitionNode(name, implementsIntefaces, directives, fieldSet);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private InterfaceTypeDefinitionNode ParseInterfaceTypeDefinition()
    {
        MandatoryNextToken(GraphQLTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        return new InterfaceTypeDefinitionNode(UseTopLevelDescription(), 
                                               name, 
                                               ParseImplementsInterfacesOptional(), 
                                               ParseDirectivesOptional(), 
                                               ParseFieldsOptionalDefinition());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ExtendInterfaceTypeDefinitionNode ParseExtendInterfaceTypeDefinition()
    {
        MandatoryNextToken(GraphQLTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();
        var implementsIntefaces = ParseImplementsInterfacesOptional();
        var directives = ParseDirectivesOptional();
        var fieldSet = ParseFieldsOptionalDefinition();

        if ((implementsIntefaces.Count == 0) && (directives.Count == 0) && (fieldSet.Count == 0))
            throw SyntaxException.ExtendInterfaceTypeMissingAtLeastOne(_tokenizer.Location);

        return new ExtendInterfaceTypeDefinitionNode(name, implementsIntefaces, directives, fieldSet);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private UnionTypeDefinitionNode ParseUnionTypeDefinition()
    {
        MandatoryNextToken(GraphQLTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        return new UnionTypeDefinitionNode(UseTopLevelDescription(), 
                                           name, 
                                           ParseDirectivesOptional(), 
                                           ParseMemberTypesOptional());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ExtendUnionTypeDefinitionNode ParseExtendUnionTypeDefinition()
    {
        MandatoryNextToken(GraphQLTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        var directives = ParseDirectivesOptional();
        var memberTypes = ParseMemberTypesOptional();

        if ((directives.Count == 0) && (memberTypes.Count == 0))
            throw SyntaxException.ExtendUnionTypeMissingAtLeastOne(_tokenizer.Location);

        return new ExtendUnionTypeDefinitionNode(name, directives, memberTypes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private EnumTypeDefinitionNode ParseEnumTypeDefinition()
    {
        MandatoryNextToken(GraphQLTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        return new EnumTypeDefinitionNode(UseTopLevelDescription(), 
                                          name, 
                                          ParseDirectivesOptional(), 
                                          ParseEnumValueTypesOptional());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ExtendEnumTypeDefinitionNode ParseExtendEnumTypeDefinition()
    {
        MandatoryNextToken(GraphQLTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        var directives = ParseDirectivesOptional();
        var enumValues = ParseEnumValueTypesOptional();

        if ((directives.Count == 0) && (enumValues.Count == 0))
            throw SyntaxException.ExtendEnumTypeMissingAtLeastOne(_tokenizer.Location);

        return new ExtendEnumTypeDefinitionNode(name, directives, enumValues);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private InputObjectTypeDefinitionNode ParseInputObjectTypeDefinition()
    {
        MandatoryNextToken(GraphQLTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();
        var directives = ParseDirectivesOptional();
        MandatoryTokenNext(GraphQLTokenKind.LeftCurlyBracket);
        var inputFields = ParseInputValueListDefinition();
        MandatoryToken(GraphQLTokenKind.RightCurlyBracket);
        _tokenizer.Next();

        return new InputObjectTypeDefinitionNode(UseTopLevelDescription(), 
                                                 name, 
                                                 directives, 
                                                 inputFields);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ExtendInputObjectTypeDefinitionNode ParseExtendInputObjectTypeDefinition()
    {
        MandatoryNextToken(GraphQLTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();
        var directives = ParseDirectivesOptional();

        InputValueDefinitionNodeList? inputFields = null;
        if (_tokenizer.TokenKind == GraphQLTokenKind.LeftCurlyBracket)
        {
            MandatoryTokenNext(GraphQLTokenKind.LeftCurlyBracket);
            inputFields = ParseInputValueListDefinition();
            MandatoryToken(GraphQLTokenKind.RightCurlyBracket);
            _tokenizer.Next();
        }

        if ((directives.Count == 0) && (inputFields is null))
            throw SyntaxException.ExtendInputObjectTypeMissingAtLeastOne(_tokenizer.Location);

        return new ExtendInputObjectTypeDefinitionNode(name, directives, inputFields ?? new());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DirectiveDefinitionNode ParseDirectiveDefinition()
    {
        MandatoryNextToken(GraphQLTokenKind.At);
        MandatoryNextToken(GraphQLTokenKind.Name);
        string name = _tokenizer.TokenValue;
        MandatoryNext();
        var arguments = ParseArgumentsOptionalDefinition();
        var repeatable = OptionalKeyword("repeatable");
        MandatoryKeyword("on");

        return new DirectiveDefinitionNode(UseTopLevelDescription(), 
                                           name, 
                                           arguments, 
                                           repeatable, 
                                           ParseDirectiveLocations());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private NameList ParseImplementsInterfacesOptional()
    {
        NameList list = new();

        if (OptionalKeyword("implements"))
        {
            OptionalToken(GraphQLTokenKind.Ampersand);
            MandatoryToken(GraphQLTokenKind.Name);
            list.Add(_tokenizer.TokenValue);
            _tokenizer.Next();

            while(_tokenizer.TokenKind == GraphQLTokenKind.Ampersand)
            {
                MandatoryNextToken(GraphQLTokenKind.Name);
                list.Add(_tokenizer.TokenValue);
                _tokenizer.Next();
            }
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private VariableDefinitionNodeList ParseVariablesOptionalDefinition()
    {
        if (_tokenizer.TokenKind == GraphQLTokenKind.LeftParenthesis)
        {
            MandatoryNext();
            var arguments = ParseVariableslDefinition();
            MandatoryTokenNext(GraphQLTokenKind.RightParenthesis);
            return arguments;
        }

        return new();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private VariableDefinitionNodeList ParseVariableslDefinition()
    {
        VariableDefinitionNodeList list = new();

        do
        {
            MandatoryTokenNext(GraphQLTokenKind.Dollar);
            MandatoryToken(GraphQLTokenKind.Name);
            string name = _tokenizer.TokenValue;
            MandatoryNextToken(GraphQLTokenKind.Colon);
            MandatoryNext();

            list.Add(new VariableDefinitionNode(name, 
                                                ParseType(), 
                                                ParseDefaultValueOptional(), 
                                                ParseDirectivesOptional()));

        } while (_tokenizer.TokenKind == GraphQLTokenKind.Dollar);

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SelectionDefinitionNodeList ParseSelectionSetOptional()
    {
        if (_tokenizer.TokenKind == GraphQLTokenKind.LeftCurlyBracket)
            return ParseSelectionSet();

        return new();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SelectionDefinitionNodeList ParseSelectionSet()
    {
        SelectionDefinitionNodeList list = new();

        MandatoryTokenNext(GraphQLTokenKind.LeftCurlyBracket);

        do
        {
            switch (_tokenizer.TokenKind)
            {
                case GraphQLTokenKind.Name:
                    {
                        MandatoryToken(GraphQLTokenKind.Name);
                        string alias = string.Empty;
                        string name = _tokenizer.TokenValue;
                        MandatoryNext();

                        if (_tokenizer.TokenKind == GraphQLTokenKind.Colon)
                        {
                            MandatoryNextToken(GraphQLTokenKind.Name);
                            alias = name;
                            name = _tokenizer.TokenValue;
                            MandatoryNext();
                        }

                        list.Add(new FieldSelectionNode(alias, 
                                                        name, 
                                                        ParseArgumentsOptional(constant: false), 
                                                        ParseDirectivesOptional(), 
                                                        ParseSelectionSetOptional()));
                    }
                    break;
                case GraphQLTokenKind.Spread:
                    {
                        MandatoryNext();

                        string name = string.Empty;
                        if (_tokenizer.TokenKind == GraphQLTokenKind.Name)
                        {
                            name = _tokenizer.TokenValue;
                            if (name != "on")
                            {
                                _tokenizer.Next();
                                list.Add(new FragmentSpreadSelectionNode(name, ParseDirectivesOptional()));
                                break;
                            }
                            else
                            {
                                MandatoryNextToken(GraphQLTokenKind.Name);
                                name = _tokenizer.TokenValue;
                                _tokenizer.Next();
                            }
                        }

                        list.Add(new InlineFragmentSelectionNode(name, 
                                                                 ParseDirectivesOptional(), 
                                                                 ParseSelectionSet()));
                    }
                    break;
                default:
                    throw SyntaxException.SelectionSetInvalidToken(_tokenizer.Location, _tokenizer.TokenKind);

            }
        } while (_tokenizer.TokenKind != GraphQLTokenKind.RightCurlyBracket);

        _tokenizer.Next();
        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private FieldDefinitionNodeList ParseFieldsOptionalDefinition()
    {
        FieldDefinitionNodeList list = new();

        if (_tokenizer.TokenKind == GraphQLTokenKind.LeftCurlyBracket)
        {
            MandatoryNext();

            do
            {
                var description = OptionalString();
                MandatoryToken(GraphQLTokenKind.Name);
                string name = _tokenizer.TokenValue;
                MandatoryNext();
                var arguments = ParseArgumentsOptionalDefinition();
                MandatoryTokenNext(GraphQLTokenKind.Colon);

                list.Add(new FieldDefinitionNode(description ?? string.Empty, 
                                                name, 
                                                arguments, 
                                                ParseType(), 
                                                ParseDirectivesOptional()));

            } while (_tokenizer.TokenKind != GraphQLTokenKind.RightCurlyBracket);

            _tokenizer.Next();
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private EnumValueDefinitionList ParseEnumValueTypesOptional()
    {
        EnumValueDefinitionList list = new();

        if (_tokenizer.TokenKind == GraphQLTokenKind.LeftCurlyBracket)
        {
            MandatoryNext();

            do
            {
                var description = OptionalString();
                MandatoryToken(GraphQLTokenKind.Name);
                string name = _tokenizer.TokenValue;
                MandatoryNext();
                
                list.Add(new EnumValueDefinition(description ?? string.Empty,
                                                 name, 
                                                 ParseDirectivesOptional()));

            } while (_tokenizer.TokenKind != GraphQLTokenKind.RightCurlyBracket);

            _tokenizer.Next();
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private NameList ParseMemberTypesOptional()
    {
        NameList list = new();

        if (_tokenizer.TokenKind == GraphQLTokenKind.Equals)
        {
            MandatoryNext();
            OptionalToken(GraphQLTokenKind.Vertical);
            MandatoryToken(GraphQLTokenKind.Name);
            list.Add(_tokenizer.TokenValue);
            _tokenizer.Next();

            while (_tokenizer.TokenKind == GraphQLTokenKind.Vertical)
            {
                MandatoryNextToken(GraphQLTokenKind.Name);
                list.Add(_tokenizer.TokenValue);
                _tokenizer.Next();
            }
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private InputValueDefinitionNodeList ParseArgumentsOptionalDefinition()
    {
        if (_tokenizer.TokenKind == GraphQLTokenKind.LeftParenthesis)
        {
            MandatoryNext();
            var arguments = ParseInputValueListDefinition();
            MandatoryTokenNext(GraphQLTokenKind.RightParenthesis);
            return arguments;
        }

        return new();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private InputValueDefinitionNodeList ParseInputValueListDefinition()
    {
        InputValueDefinitionNodeList list = new();

        do
        {
            var description = OptionalString();
            MandatoryToken(GraphQLTokenKind.Name);
            string name = _tokenizer.TokenValue;
            MandatoryNextToken(GraphQLTokenKind.Colon);
            MandatoryNext();

            list.Add(new InputValueDefinitionNode(description ?? string.Empty, 
                                                  name, 
                                                  ParseType(), 
                                                  ParseDefaultValueOptional(), 
                                                  ParseDirectivesOptional()));

        } while ((_tokenizer.TokenKind == GraphQLTokenKind.Name) || (_tokenizer.TokenKind == GraphQLTokenKind.StringValue));

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueNode? ParseDefaultValueOptional()
    {
        if (_tokenizer.TokenKind != GraphQLTokenKind.Equals)
            return null;

        MandatoryNext();
        return ParseValue(constant: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueNode ParseValue(bool constant)
    {
        ValueNode node;

        switch (_tokenizer.TokenKind)
        {
            case GraphQLTokenKind.Dollar:
                {
                    if (constant)
                        throw SyntaxException.TokenNotAllowedHere(_tokenizer.Location, _tokenizer.TokenKind);

                    MandatoryNextToken(GraphQLTokenKind.Name);
                    node = new VariableValueNode(_tokenizer.TokenValue);
                }
                break;
            case GraphQLTokenKind.IntValue:
                node = new IntValueNode(_tokenizer.TokenValue);
                break;
            case GraphQLTokenKind.FloatValue:
                node = new FloatValueNode(_tokenizer.TokenValue);
                break;
            case GraphQLTokenKind.StringValue:
                node = new StringValueNode(_tokenizer.TokenString);
                break;
            case GraphQLTokenKind.Name:
                {
                    var name = _tokenizer.TokenValue;
                    node = name switch
                    {
                        "true" => BooleanValueNode.True,
                        "false" => BooleanValueNode.False,
                        "null" => NullValueNode.Null,
                        _ => new EnumValueNode(name),
                    };
                }
                break;
            case GraphQLTokenKind.LeftSquareBracket:
                {
                    ValueNodeList list = new();

                    MandatoryNext();
                    while (_tokenizer.TokenKind != GraphQLTokenKind.RightSquareBracket)
                        list.Add(ParseValue(constant: constant));

                    node = new ListValueNode(list);
                }
                break;
            case GraphQLTokenKind.LeftCurlyBracket:
                {
                    ObjectFieldNodeList list = new();

                    MandatoryNext();
                    while (_tokenizer.TokenKind != GraphQLTokenKind.RightCurlyBracket)
                    {
                        MandatoryToken(GraphQLTokenKind.Name);
                        string name = _tokenizer.TokenValue;
                        MandatoryNextToken(GraphQLTokenKind.Colon);
                        MandatoryNext();

                        list.Add(new ObjectFieldNode(name, ParseValue(constant: constant)));
                    }

                    node = new ObjectValueNode(list);
                }
                break;
            default:
                throw SyntaxException.TokenNotAllowedHere(_tokenizer.Location, _tokenizer.TokenKind);
        }

        MandatoryNext();
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DirectiveNodeList ParseDirectivesOptional()
    {
        DirectiveNodeList list = new();

        while(_tokenizer.TokenKind == GraphQLTokenKind.At)
        {
            MandatoryNextToken(GraphQLTokenKind.Name);
            string name = _tokenizer.TokenValue;
            _tokenizer.Next();

            list.Add(new DirectiveNode(name, ParseArgumentsOptional(true)));
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObjectFieldNodeList ParseArgumentsOptional(bool constant)
    {
        ObjectFieldNodeList list = new();

        if (_tokenizer.TokenKind == GraphQLTokenKind.LeftParenthesis)
        {
            MandatoryNext();
            while (_tokenizer.TokenKind != GraphQLTokenKind.RightParenthesis)
            {
                MandatoryToken(GraphQLTokenKind.Name);
                string name = _tokenizer.TokenValue;
                MandatoryNextToken(GraphQLTokenKind.Colon);
                MandatoryNext();

                list.Add(new ObjectFieldNode(name, ParseValue(constant: constant)));
            }

            MandatoryNext();
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TypeNode ParseType()
    {
        switch(_tokenizer.TokenKind) 
        {
            case GraphQLTokenKind.Name:
                {
                    var name = _tokenizer.TokenValue;
                    MandatoryNext();

                    return new TypeNameNode(name, OptionalToken(GraphQLTokenKind.Exclamation));
                }
            case GraphQLTokenKind.LeftSquareBracket:
                {
                    MandatoryNext();
                    var listType = ParseType();
                    MandatoryTokenNext(GraphQLTokenKind.RightSquareBracket);

                    return new TypeListNode(listType, OptionalToken(GraphQLTokenKind.Exclamation));
                }
            default:
                throw SyntaxException.TypeMustBeNameOrList(_tokenizer.Location, _tokenizer.TokenKind);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DirectiveLocations ParseDirectiveLocations()
    {
        MandatoryNextToken(GraphQLTokenKind.Name);
        DirectiveLocations directiveLocations = StringToDriveLocations(_tokenizer.TokenValue);

        if (_tokenizer.Next())
        {
            while (_tokenizer.TokenKind == GraphQLTokenKind.Vertical)
            {
                MandatoryNextToken(GraphQLTokenKind.Name);
                directiveLocations |= StringToDriveLocations(_tokenizer.TokenValue);
                _tokenizer.Next();
            }
        }

        return directiveLocations;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DirectiveLocations StringToDriveLocations(string location)
    {
        return location switch
        {
            // Executable Directive Locations
            "QUERY" => DirectiveLocations.QUERY,
            "MUTATION" => DirectiveLocations.MUTATION,
            "SUBSCRIPTION" => DirectiveLocations.SUBSCRIPTION,
            "FIELD" => DirectiveLocations.FIELD,
            "FRAGMENT_DEFINITION" => DirectiveLocations.FRAGMENT_DEFINITION,
            "FRAGMENT_SPREAD" => DirectiveLocations.FRAGMENT_SPREAD,
            "INLINE_FRAGMENT" => DirectiveLocations.INLINE_FRAGMENT,
            "VARIABLE_DEFINITION" => DirectiveLocations.VARIABLE_DEFINITION,

            // Type System Directive Locations
            "SCHEMA" => DirectiveLocations.SCHEMA,
            "SCALAR" => DirectiveLocations.SCALAR,
            "OBJECT" => DirectiveLocations.OBJECT,
            "FIELD_DEFINITION" => DirectiveLocations.FIELD_DEFINITION,
            "ARGUMENT_DEFINITION" => DirectiveLocations.ARGUMENT_DEFINITION,
            "INTERFACE" => DirectiveLocations.INTERFACE,
            "UNION" => DirectiveLocations.UNION,
            "ENUM" => DirectiveLocations.ENUM,
            "ENUM_VALUE" => DirectiveLocations.ENUM_VALUE,
            "INPUT_OBJECT" => DirectiveLocations.INPUT_OBJECT,
            "INPUT_FIELD_DEFINITION" => DirectiveLocations.INPUT_FIELD_DEFINITION,

            _ => throw SyntaxException.ExpectedDirectiveLocationNotFound(_tokenizer.Location, location)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string UseTopLevelDescription()
    {
        string? description = _description;
        _description = null;
        return description ?? string.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private OperationType OperationTypeFromTokenValue()
    {
        return _tokenizer.TokenValue switch
        {
            "query" => OperationType.QUERY,
            "mutation" => OperationType.MUTATION,
            "subscription" => OperationType.SUBSCRIPTION,
            _ => throw SyntaxException.UnrecognizedOperationType(_tokenizer.Location, _tokenizer.TokenValue)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryNext()
    {
        if ((_tokenizer.TokenKind == GraphQLTokenKind.EndOfText) || !_tokenizer.Next())
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryToken(GraphQLTokenKind token) 
    {
        if (_tokenizer.TokenKind == GraphQLTokenKind.EndOfText)
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);

        if (_tokenizer.TokenKind != token)
            throw SyntaxException.ExpectedTokenNotFound(_tokenizer.Location, token, _tokenizer.TokenKind);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryNextToken(GraphQLTokenKind token)
    {
        if ((_tokenizer.TokenKind == GraphQLTokenKind.EndOfText) || !_tokenizer.Next())
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);

        if (_tokenizer.TokenKind != token)
            throw SyntaxException.ExpectedTokenNotFound(_tokenizer.Location, token, _tokenizer.TokenKind);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryTokenNext(GraphQLTokenKind token)
    {
        if (_tokenizer.TokenKind == GraphQLTokenKind.EndOfText)
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);

        if (_tokenizer.TokenKind != token)
            throw SyntaxException.ExpectedTokenNotFound(_tokenizer.Location, token, _tokenizer.TokenKind);

        if (!_tokenizer.Next())
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryKeyword(string keyword)
    {
        if (_tokenizer.TokenKind == GraphQLTokenKind.EndOfText)
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);

        if (_tokenizer.TokenKind != GraphQLTokenKind.Name)
            throw SyntaxException.ExpectedTokenNotFound(_tokenizer.Location, GraphQLTokenKind.Name, _tokenizer.TokenKind);

        if (_tokenizer.TokenValue != keyword)
            throw SyntaxException.ExpectedKeywordNotFound(_tokenizer.Location, "on", _tokenizer.TokenValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool OptionalToken(GraphQLTokenKind tokenKind)
    {
        if (_tokenizer.TokenKind == tokenKind)
        {
            _tokenizer.Next();
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool OptionalKeyword(string keyword)
    {
        if ((_tokenizer.TokenKind == GraphQLTokenKind.Name) && (_tokenizer.TokenValue == keyword))
        {
            _tokenizer.Next();
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string? OptionalString()
    {
        if (_tokenizer.TokenKind == GraphQLTokenKind.StringValue)
        {
            string ret = _tokenizer.TokenString;
            _tokenizer.Next();
            return ret;
        }

        return null;
    }
}
