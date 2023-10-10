namespace RocketQL.Core.Serializers;

public ref struct SchemaDeserializer
{
    private SchemaNode _schema;
    private DocumentTokenizer _tokenizer;
    private string? _description = null;

    public SchemaDeserializer(string source, ReadOnlySpan<char> text)
        : this(source, new SchemaNode(new(), new(), new(), new(), new(), new(), new(), new(), new(), new(), new(), new(), new(), new(), new()), text)
    {
    }

    public SchemaDeserializer(string source, SchemaNode schema, ReadOnlySpan<char> text)
    {
        _schema = schema;
        _tokenizer = new DocumentTokenizer(source, text);
    }

    public SchemaNode Deserialize()
    {
        // Move to the first real token
        _tokenizer.Next();

        // Keep processing until we reach the end of the tokens or throw because of an exception
        while (_tokenizer.TokenKind != DocumentTokenKind.EndOfText)
        {
            switch (_tokenizer.TokenKind)
            {
                case DocumentTokenKind.StringValue:
                    _description = _tokenizer.TokenString;
                    _tokenizer.Next();
                    break;
                case DocumentTokenKind.Name:
                    switch (_tokenizer.TokenValue)
                    {
                        case "query":
                        case "mutation":
                        case "subscription":
                        case "fragment":
                            throw SyntaxException.QueryNotAllowedInSchema(_tokenizer.Location);
                        case "schema":
                            _schema.Schemas.Add(ParseSchemaDefinition());
                            break;
                        case "scalar":
                            _schema.ScalarTypes.Add(ParseScalarTypeDefinition());
                            break;
                        case "type":
                            _schema.ObjectTypes.Add(ParseObjectTypeDefinition());
                            break;
                        case "interface":
                            _schema.InterfaceTypes.Add(ParseInterfaceTypeDefinition());
                            break;
                        case "union":
                            _schema.UnionTypes.Add(ParseUnionTypeDefinition());
                            break;
                        case "enum":
                            _schema.EnumTypes.Add(ParseEnumTypeDefinition());
                            break;
                        case "input":
                            _schema.InputObjectTypes.Add(ParseInputObjectTypeDefinition());
                            break;
                        case "directive":
                            _schema.Directives.Add(ParseDirectiveDefinition());
                            break;
                        case "extend":
                            {
                                MandatoryNextToken(DocumentTokenKind.Name);
                                switch (_tokenizer.TokenValue)
                                {
                                    case "schema":
                                        _schema.ExtendSchemas.Add(ParseExtendSchemaDefinition());
                                        break;
                                    case "scalar":
                                        _schema.ExtendScalarTypes.Add(ParseExtendScalarTypeDefinition());
                                        break;
                                    case "type":
                                        _schema.ExtendObjectTypes.Add(ParseExtendObjectTypeDefinition());
                                        break;
                                    case "interface":
                                        _schema.ExtendInterfaceTypes.Add(ParseExtendInterfaceTypeDefinition());
                                        break;
                                    case "union":
                                        _schema.ExtendUnionTypes.Add(ParseExtendUnionTypeDefinition());
                                        break;
                                    case "enum":
                                        _schema.ExtendEnumTypes.Add(ParseExtendEnumTypeDefinition());
                                        break;
                                    case "input":
                                        _schema.ExtendInputObjectTypes.Add(ParseExtendInputObjectTypeDefinition());
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
                case DocumentTokenKind.LeftCurlyBracket:
                    throw SyntaxException.UnnamedQueryNotAllowedInSchema(_tokenizer.Location);
                default:
                    throw SyntaxException.UnrecognizedToken(_tokenizer.Location, _tokenizer.TokenKind.ToString());
            }
        }

        return _schema;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SchemaDefinitionNode ParseSchemaDefinition()
    {
        MandatoryNext();
        var directives = ParseDirectivesOptional();
        MandatoryTokenNext(DocumentTokenKind.LeftCurlyBracket);

        OperationTypeDefinitionNodeList list = new();

        do
        {
            MandatoryToken(DocumentTokenKind.Name);
            OperationType operationType = OperationTypeFromTokenValue();
            MandatoryNextToken(DocumentTokenKind.Colon);
            MandatoryNextToken(DocumentTokenKind.Name);
            string namedType = _tokenizer.TokenValue;
            MandatoryNext();

            list.Add(new OperationTypeDefinitionNode(operationType, namedType));

        } while (_tokenizer.TokenKind != DocumentTokenKind.RightCurlyBracket);

        _tokenizer.Next();
        return new SchemaDefinitionNode(UseTopLevelDescription(), directives, list);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ExtendSchemaDefinitionNode ParseExtendSchemaDefinition()
    {
        MandatoryNext();
        var directives = ParseDirectivesOptional();

        OperationTypeDefinitionNodeList list = new();

        if (_tokenizer.TokenKind == DocumentTokenKind.LeftCurlyBracket)
        {
            MandatoryNext();

            do
            {
                MandatoryToken(DocumentTokenKind.Name);
                OperationType operationType = OperationTypeFromTokenValue();
                MandatoryNextToken(DocumentTokenKind.Colon);
                MandatoryNextToken(DocumentTokenKind.Name);
                string namedType = _tokenizer.TokenValue;
                MandatoryNext();

                list.Add(new OperationTypeDefinitionNode(operationType, namedType));

            } while (_tokenizer.TokenKind != DocumentTokenKind.RightCurlyBracket);

            _tokenizer.Next();
        }
        else if (directives.Count == 0)
            throw SyntaxException.ExtendSchemaMissingAtLeastOne(_tokenizer.Location);

        return new ExtendSchemaDefinitionNode(directives, list);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ScalarTypeDefinitionNode ParseScalarTypeDefinition()
    {
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        return new ScalarTypeDefinitionNode(UseTopLevelDescription(), name, ParseDirectivesOptional());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ExtendScalarTypeDefinitionNode ParseExtendScalarTypeDefinition()
    {
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        return new ExtendScalarTypeDefinitionNode(name, ParseDirectivesOptional());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObjectTypeDefinitionNode ParseObjectTypeDefinition()
    {
        MandatoryNextToken(DocumentTokenKind.Name);
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
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();
        var implementsIntefaces = ParseImplementsInterfacesOptional();
        var directives = ParseDirectivesOptional();
        var fieldSet = ParseFieldsOptionalDefinition();

        if (implementsIntefaces.Count == 0 && directives.Count == 0 && fieldSet.Count == 0)
            throw SyntaxException.ExtendObjectTypeMissingAtLeastOne(_tokenizer.Location);

        return new ExtendObjectTypeDefinitionNode(name, implementsIntefaces, directives, fieldSet);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private InterfaceTypeDefinitionNode ParseInterfaceTypeDefinition()
    {
        MandatoryNextToken(DocumentTokenKind.Name);
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
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();
        var implementsIntefaces = ParseImplementsInterfacesOptional();
        var directives = ParseDirectivesOptional();
        var fieldSet = ParseFieldsOptionalDefinition();

        if (implementsIntefaces.Count == 0 && directives.Count == 0 && fieldSet.Count == 0)
            throw SyntaxException.ExtendInterfaceTypeMissingAtLeastOne(_tokenizer.Location);

        return new ExtendInterfaceTypeDefinitionNode(name, implementsIntefaces, directives, fieldSet);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private UnionTypeDefinitionNode ParseUnionTypeDefinition()
    {
        MandatoryNextToken(DocumentTokenKind.Name);
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
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        var directives = ParseDirectivesOptional();
        var memberTypes = ParseMemberTypesOptional();

        if (directives.Count == 0 && memberTypes.Count == 0)
            throw SyntaxException.ExtendUnionTypeMissingAtLeastOne(_tokenizer.Location);

        return new ExtendUnionTypeDefinitionNode(name, directives, memberTypes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private EnumTypeDefinitionNode ParseEnumTypeDefinition()
    {
        MandatoryNextToken(DocumentTokenKind.Name);
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
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        var directives = ParseDirectivesOptional();
        var enumValues = ParseEnumValueTypesOptional();

        if (directives.Count == 0 && enumValues.Count == 0)
            throw SyntaxException.ExtendEnumTypeMissingAtLeastOne(_tokenizer.Location);

        return new ExtendEnumTypeDefinitionNode(name, directives, enumValues);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private InputObjectTypeDefinitionNode ParseInputObjectTypeDefinition()
    {
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();
        var directives = ParseDirectivesOptional();
        MandatoryTokenNext(DocumentTokenKind.LeftCurlyBracket);
        var inputFields = ParseInputValueListDefinition();
        MandatoryToken(DocumentTokenKind.RightCurlyBracket);
        _tokenizer.Next();

        return new InputObjectTypeDefinitionNode(UseTopLevelDescription(),
                                                 name,
                                                 directives,
                                                 inputFields);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ExtendInputObjectTypeDefinitionNode ParseExtendInputObjectTypeDefinition()
    {
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();
        var directives = ParseDirectivesOptional();

        InputValueDefinitionNodeList? inputFields = null;
        if (_tokenizer.TokenKind == DocumentTokenKind.LeftCurlyBracket)
        {
            MandatoryTokenNext(DocumentTokenKind.LeftCurlyBracket);
            inputFields = ParseInputValueListDefinition();
            MandatoryToken(DocumentTokenKind.RightCurlyBracket);
            _tokenizer.Next();
        }

        if (directives.Count == 0 && inputFields is null)
            throw SyntaxException.ExtendInputObjectTypeMissingAtLeastOne(_tokenizer.Location);

        return new ExtendInputObjectTypeDefinitionNode(name, directives, inputFields ?? new());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DirectiveDefinitionNode ParseDirectiveDefinition()
    {
        var location = _tokenizer.Location;
        MandatoryNextToken(DocumentTokenKind.At);
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        MandatoryNext();
        var arguments = ParseArgumentsOptionalDefinition();
        var repeatable = OptionalKeyword("repeatable");
        MandatoryKeyword("on");

        return new DirectiveDefinitionNode(UseTopLevelDescription(),
                                           name,
                                           arguments,
                                           repeatable,
                                           ParseDirectiveLocations(),
                                           location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private NameList ParseImplementsInterfacesOptional()
    {
        NameList list = new();

        if (OptionalKeyword("implements"))
        {
            OptionalToken(DocumentTokenKind.Ampersand);
            MandatoryToken(DocumentTokenKind.Name);
            list.Add(_tokenizer.TokenValue);
            _tokenizer.Next();

            while (_tokenizer.TokenKind == DocumentTokenKind.Ampersand)
            {
                MandatoryNextToken(DocumentTokenKind.Name);
                list.Add(_tokenizer.TokenValue);
                _tokenizer.Next();
            }
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private VariableDefinitionNodeList ParseVariablesOptionalDefinition()
    {
        if (_tokenizer.TokenKind == DocumentTokenKind.LeftParenthesis)
        {
            MandatoryNext();
            var arguments = ParseVariableslDefinition();
            MandatoryTokenNext(DocumentTokenKind.RightParenthesis);
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
            MandatoryTokenNext(DocumentTokenKind.Dollar);
            MandatoryToken(DocumentTokenKind.Name);
            string name = _tokenizer.TokenValue;
            MandatoryNextToken(DocumentTokenKind.Colon);
            MandatoryNext();

            list.Add(new VariableDefinitionNode(name,
                                                ParseType(),
                                                ParseDefaultValueOptional(),
                                                ParseDirectivesOptional()));

        } while (_tokenizer.TokenKind == DocumentTokenKind.Dollar);

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SelectionDefinitionNodeList ParseSelectionSetOptional()
    {
        if (_tokenizer.TokenKind == DocumentTokenKind.LeftCurlyBracket)
            return ParseSelectionSet();

        return new();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SelectionDefinitionNodeList ParseSelectionSet()
    {
        SelectionDefinitionNodeList list = new();

        MandatoryTokenNext(DocumentTokenKind.LeftCurlyBracket);

        do
        {
            switch (_tokenizer.TokenKind)
            {
                case DocumentTokenKind.Name:
                    {
                        MandatoryToken(DocumentTokenKind.Name);
                        string alias = string.Empty;
                        string name = _tokenizer.TokenValue;
                        MandatoryNext();

                        if (_tokenizer.TokenKind == DocumentTokenKind.Colon)
                        {
                            MandatoryNextToken(DocumentTokenKind.Name);
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
                case DocumentTokenKind.Spread:
                    {
                        MandatoryNext();

                        string name = string.Empty;
                        if (_tokenizer.TokenKind == DocumentTokenKind.Name)
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
                                MandatoryNextToken(DocumentTokenKind.Name);
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
                    throw SyntaxException.SelectionSetInvalidToken(_tokenizer.Location, _tokenizer.TokenKind.ToString());

            }
        } while (_tokenizer.TokenKind != DocumentTokenKind.RightCurlyBracket);

        _tokenizer.Next();
        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private FieldDefinitionNodeList ParseFieldsOptionalDefinition()
    {
        FieldDefinitionNodeList list = new();

        if (_tokenizer.TokenKind == DocumentTokenKind.LeftCurlyBracket)
        {
            MandatoryNext();

            do
            {
                var description = OptionalString();
                MandatoryToken(DocumentTokenKind.Name);
                string name = _tokenizer.TokenValue;
                MandatoryNext();
                var arguments = ParseArgumentsOptionalDefinition();
                MandatoryTokenNext(DocumentTokenKind.Colon);

                list.Add(new FieldDefinitionNode(description ?? string.Empty,
                                                name,
                                                arguments,
                                                ParseType(),
                                                ParseDirectivesOptional()));

            } while (_tokenizer.TokenKind != DocumentTokenKind.RightCurlyBracket);

            _tokenizer.Next();
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private EnumValueDefinitionList ParseEnumValueTypesOptional()
    {
        EnumValueDefinitionList list = new();

        if (_tokenizer.TokenKind == DocumentTokenKind.LeftCurlyBracket)
        {
            MandatoryNext();

            do
            {
                var description = OptionalString();
                MandatoryToken(DocumentTokenKind.Name);
                string name = _tokenizer.TokenValue;
                MandatoryNext();

                list.Add(new EnumValueDefinition(description ?? string.Empty,
                                                 name,
                                                 ParseDirectivesOptional()));

            } while (_tokenizer.TokenKind != DocumentTokenKind.RightCurlyBracket);

            _tokenizer.Next();
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private NameList ParseMemberTypesOptional()
    {
        NameList list = new();

        if (_tokenizer.TokenKind == DocumentTokenKind.Equals)
        {
            MandatoryNext();
            OptionalToken(DocumentTokenKind.Vertical);
            MandatoryToken(DocumentTokenKind.Name);
            list.Add(_tokenizer.TokenValue);
            _tokenizer.Next();

            while (_tokenizer.TokenKind == DocumentTokenKind.Vertical)
            {
                MandatoryNextToken(DocumentTokenKind.Name);
                list.Add(_tokenizer.TokenValue);
                _tokenizer.Next();
            }
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private InputValueDefinitionNodeList ParseArgumentsOptionalDefinition()
    {
        if (_tokenizer.TokenKind == DocumentTokenKind.LeftParenthesis)
        {
            MandatoryNext();
            var arguments = ParseInputValueListDefinition();
            MandatoryTokenNext(DocumentTokenKind.RightParenthesis);
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
            MandatoryToken(DocumentTokenKind.Name);
            string name = _tokenizer.TokenValue;
            MandatoryNextToken(DocumentTokenKind.Colon);
            MandatoryNext();

            list.Add(new InputValueDefinitionNode(description ?? string.Empty,
                                                  name,
                                                  ParseType(),
                                                  ParseDefaultValueOptional(),
                                                  ParseDirectivesOptional()));

        } while (_tokenizer.TokenKind == DocumentTokenKind.Name || _tokenizer.TokenKind == DocumentTokenKind.StringValue);

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueNode? ParseDefaultValueOptional()
    {
        if (_tokenizer.TokenKind != DocumentTokenKind.Equals)
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
            case DocumentTokenKind.Dollar:
                {
                    if (constant)
                        throw SyntaxException.TokenNotAllowedHere(_tokenizer.Location, _tokenizer.TokenKind.ToString());

                    MandatoryNextToken(DocumentTokenKind.Name);
                    node = new VariableValueNode(_tokenizer.TokenValue);
                }
                break;
            case DocumentTokenKind.IntValue:
                node = new IntValueNode(_tokenizer.TokenValue);
                break;
            case DocumentTokenKind.FloatValue:
                node = new FloatValueNode(_tokenizer.TokenValue);
                break;
            case DocumentTokenKind.StringValue:
                node = new StringValueNode(_tokenizer.TokenString);
                break;
            case DocumentTokenKind.Name:
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
            case DocumentTokenKind.LeftSquareBracket:
                {
                    ValueNodeList list = new();

                    MandatoryNext();
                    while (_tokenizer.TokenKind != DocumentTokenKind.RightSquareBracket)
                        list.Add(ParseValue(constant: constant));

                    node = new ListValueNode(list);
                }
                break;
            case DocumentTokenKind.LeftCurlyBracket:
                {
                    ObjectFieldNodeList list = new();

                    MandatoryNext();
                    while (_tokenizer.TokenKind != DocumentTokenKind.RightCurlyBracket)
                    {
                        MandatoryToken(DocumentTokenKind.Name);
                        string name = _tokenizer.TokenValue;
                        MandatoryNextToken(DocumentTokenKind.Colon);
                        MandatoryNext();

                        list.Add(new ObjectFieldNode(name, ParseValue(constant: constant)));
                    }

                    node = new ObjectValueNode(list);
                }
                break;
            default:
                throw SyntaxException.TokenNotAllowedHere(_tokenizer.Location, _tokenizer.TokenKind.ToString());
        }

        MandatoryNext();
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DirectiveNodeList ParseDirectivesOptional()
    {
        DirectiveNodeList list = new();

        while (_tokenizer.TokenKind == DocumentTokenKind.At)
        {
            MandatoryNextToken(DocumentTokenKind.Name);
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

        if (_tokenizer.TokenKind == DocumentTokenKind.LeftParenthesis)
        {
            MandatoryNext();
            while (_tokenizer.TokenKind != DocumentTokenKind.RightParenthesis)
            {
                MandatoryToken(DocumentTokenKind.Name);
                string name = _tokenizer.TokenValue;
                MandatoryNextToken(DocumentTokenKind.Colon);
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
        switch (_tokenizer.TokenKind)
        {
            case DocumentTokenKind.Name:
                {
                    var name = _tokenizer.TokenValue;
                    MandatoryNext();

                    return new TypeNameNode(name, OptionalToken(DocumentTokenKind.Exclamation));
                }
            case DocumentTokenKind.LeftSquareBracket:
                {
                    MandatoryNext();
                    var listType = ParseType();
                    MandatoryTokenNext(DocumentTokenKind.RightSquareBracket);

                    return new TypeListNode(listType, OptionalToken(DocumentTokenKind.Exclamation));
                }
            default:
                throw SyntaxException.TypeMustBeNameOrList(_tokenizer.Location, _tokenizer.TokenKind.ToString());
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DirectiveLocations ParseDirectiveLocations()
    {
        MandatoryNextToken(DocumentTokenKind.Name);
        DirectiveLocations directiveLocations = StringToDriveLocations(_tokenizer.TokenValue);

        if (_tokenizer.Next())
        {
            while (_tokenizer.TokenKind == DocumentTokenKind.Vertical)
            {
                MandatoryNextToken(DocumentTokenKind.Name);
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
        if (_tokenizer.TokenKind == DocumentTokenKind.EndOfText || !_tokenizer.Next())
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryToken(DocumentTokenKind token)
    {
        if (_tokenizer.TokenKind == DocumentTokenKind.EndOfText)
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);

        if (_tokenizer.TokenKind != token)
            throw SyntaxException.ExpectedTokenNotFound(_tokenizer.Location, token.ToString(), _tokenizer.TokenKind.ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryNextToken(DocumentTokenKind token)
    {
        if (_tokenizer.TokenKind == DocumentTokenKind.EndOfText || !_tokenizer.Next())
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);

        if (_tokenizer.TokenKind != token)
            throw SyntaxException.ExpectedTokenNotFound(_tokenizer.Location, token.ToString(), _tokenizer.TokenKind.ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryTokenNext(DocumentTokenKind token)
    {
        if (_tokenizer.TokenKind == DocumentTokenKind.EndOfText)
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);

        if (_tokenizer.TokenKind != token)
            throw SyntaxException.ExpectedTokenNotFound(_tokenizer.Location, token.ToString(), _tokenizer.TokenKind.ToString());

        if (!_tokenizer.Next())
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryKeyword(string keyword)
    {
        if (_tokenizer.TokenKind == DocumentTokenKind.EndOfText)
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);

        if (_tokenizer.TokenKind != DocumentTokenKind.Name)
            throw SyntaxException.ExpectedTokenNotFound(_tokenizer.Location, DocumentTokenKind.Name.ToString(), _tokenizer.TokenKind.ToString());

        if (_tokenizer.TokenValue != keyword)
            throw SyntaxException.ExpectedKeywordNotFound(_tokenizer.Location, "on", _tokenizer.TokenValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool OptionalToken(DocumentTokenKind tokenKind)
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
        if (_tokenizer.TokenKind == DocumentTokenKind.Name && _tokenizer.TokenValue == keyword)
        {
            _tokenizer.Next();
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string? OptionalString()
    {
        if (_tokenizer.TokenKind == DocumentTokenKind.StringValue)
        {
            string ret = _tokenizer.TokenString;
            _tokenizer.Next();
            return ret;
        }

        return null;
    }
}
