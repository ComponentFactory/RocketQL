namespace RocketQL.Core.Serializers;

public ref struct SchemaDeserializer(ReadOnlySpan<char> text, string source)
{
    private readonly SyntaxNodeList _nodes = [];
    private DocumentTokenizer _tokenizer = new(text, source);
    private string? _description = null;

    public SchemaDeserializer(ReadOnlySpan<char> text,
                              [CallerFilePath] string filePath = "",
                              [CallerMemberName] string memberName = "",
                              [CallerLineNumber] int lineNumber = 0)
        : this(text, CallerExtensions.CallerToSource(filePath, memberName, lineNumber))
    {
    }

    public SyntaxNodeList Deserialize()
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
                            _nodes.Add(ParseSchemaDefinition());
                            break;
                        case "scalar":
                            _nodes.Add(ParseScalarTypeDefinition());
                            break;
                        case "type":
                            _nodes.Add(ParseObjectTypeDefinition());
                            break;
                        case "interface":
                            _nodes.Add(ParseInterfaceTypeDefinition());
                            break;
                        case "union":
                            _nodes.Add(ParseUnionTypeDefinition());
                            break;
                        case "enum":
                            _nodes.Add(ParseEnumTypeDefinition());
                            break;
                        case "input":
                            _nodes.Add(ParseInputObjectTypeDefinition());
                            break;
                        case "directive":
                            _nodes.Add(ParseDirectiveDefinition());
                            break;
                        case "extend":
                            {
                                MandatoryNextToken(DocumentTokenKind.Name);
                                switch (_tokenizer.TokenValue)
                                {
                                    case "schema":
                                        _nodes.Add(ParseExtendSchemaDefinition());
                                        break;
                                    case "scalar":
                                        _nodes.Add(ParseExtendScalarTypeDefinition());
                                        break;
                                    case "type":
                                        _nodes.Add(ParseExtendObjectTypeDefinition());
                                        break;
                                    case "interface":
                                        _nodes.Add(ParseExtendInterfaceTypeDefinition());
                                        break;
                                    case "union":
                                        _nodes.Add(ParseExtendUnionTypeDefinition());
                                        break;
                                    case "enum":
                                        _nodes.Add(ParseExtendEnumTypeDefinition());
                                        break;
                                    case "input":
                                        _nodes.Add(ParseExtendInputObjectTypeDefinition());
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

        return _nodes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxSchemaDefinitionNode ParseSchemaDefinition()
    {
        var schemaLocation = _tokenizer.Location;
        MandatoryNext();
        var directives = ParseDirectivesOptional();
        MandatoryTokenNext(DocumentTokenKind.LeftCurlyBracket);

        SyntaxOperationTypeDefinitionNodeList list = [];

        while (_tokenizer.TokenKind != DocumentTokenKind.RightCurlyBracket)
        {
            var operationLocation = _tokenizer.Location;
            MandatoryToken(DocumentTokenKind.Name);
            OperationType operationType = OperationTypeFromTokenValue();
            MandatoryNextToken(DocumentTokenKind.Colon);
            MandatoryNextToken(DocumentTokenKind.Name);
            string namedType = _tokenizer.TokenValue;
            MandatoryNext();

            list.Add(new SyntaxOperationTypeDefinitionNode(operationType, namedType, operationLocation));
        } 

        _tokenizer.Next();
        return new SyntaxSchemaDefinitionNode(UseTopLevelDescription(), directives, list, schemaLocation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxExtendSchemaDefinitionNode ParseExtendSchemaDefinition()
    {
        var schemaLocation = _tokenizer.Location;
        MandatoryNext();
        var directives = ParseDirectivesOptional();

        SyntaxOperationTypeDefinitionNodeList list = [];

        if (_tokenizer.TokenKind == DocumentTokenKind.LeftCurlyBracket)
        {
            MandatoryNext();

            do
            {
                var operationLocation = _tokenizer.Location;
                MandatoryToken(DocumentTokenKind.Name);
                OperationType operationType = OperationTypeFromTokenValue();
                MandatoryNextToken(DocumentTokenKind.Colon);
                MandatoryNextToken(DocumentTokenKind.Name);
                string namedType = _tokenizer.TokenValue;
                MandatoryNext();

                list.Add(new SyntaxOperationTypeDefinitionNode(operationType, namedType, operationLocation));

            } while (_tokenizer.TokenKind != DocumentTokenKind.RightCurlyBracket);

            _tokenizer.Next();
        }
        else if (directives.Count == 0)
            throw SyntaxException.ExtendSchemaMissingAtLeastOne(_tokenizer.Location);

        return new SyntaxExtendSchemaDefinitionNode(directives, list, schemaLocation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxScalarTypeDefinitionNode ParseScalarTypeDefinition()
    {
        var location = _tokenizer.Location;
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        return new SyntaxScalarTypeDefinitionNode(UseTopLevelDescription(), 
                                                  name, 
                                                  ParseDirectivesOptional(), 
                                                  location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxExtendScalarTypeDefinitionNode ParseExtendScalarTypeDefinition()
    {
        var location = _tokenizer.Location;
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        return new SyntaxExtendScalarTypeDefinitionNode(name, ParseDirectivesOptional(), location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxObjectTypeDefinitionNode ParseObjectTypeDefinition()
    {
        var location = _tokenizer.Location;
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        return new SyntaxObjectTypeDefinitionNode(UseTopLevelDescription(),
                                                  name,
                                                  ParseImplementsInterfacesOptional(),
                                                  ParseDirectivesOptional(),
                                                  ParseFieldsOptionalDefinition(),
                                                  location);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxExtendObjectTypeDefinitionNode ParseExtendObjectTypeDefinition()
    {
        var location = _tokenizer.Location; 
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();
        var implementsIntefaces = ParseImplementsInterfacesOptional();
        var directives = ParseDirectivesOptional();
        var fieldSet = ParseFieldsOptionalDefinition();

        if (implementsIntefaces.Count == 0 && directives.Count == 0 && fieldSet.Count == 0)
            throw SyntaxException.ExtendObjectTypeMissingAtLeastOne(_tokenizer.Location);

        return new SyntaxExtendObjectTypeDefinitionNode(name, 
                                                        implementsIntefaces, 
                                                        directives, 
                                                        fieldSet, 
                                                        location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxInterfaceTypeDefinitionNode ParseInterfaceTypeDefinition()
    {
        var location = _tokenizer.Location;
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        return new SyntaxInterfaceTypeDefinitionNode(UseTopLevelDescription(),
                                                     name,
                                                     ParseImplementsInterfacesOptional(),
                                                     ParseDirectivesOptional(),
                                                     ParseFieldsOptionalDefinition(),
                                                     location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxExtendInterfaceTypeDefinitionNode ParseExtendInterfaceTypeDefinition()
    {
        var location = _tokenizer.Location;
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();
        var implementsIntefaces = ParseImplementsInterfacesOptional();
        var directives = ParseDirectivesOptional();
        var fieldSet = ParseFieldsOptionalDefinition();

        if (implementsIntefaces.Count == 0 && directives.Count == 0 && fieldSet.Count == 0)
            throw SyntaxException.ExtendInterfaceTypeMissingAtLeastOne(_tokenizer.Location);

        return new SyntaxExtendInterfaceTypeDefinitionNode(name, 
                                                           implementsIntefaces, 
                                                           directives, 
                                                           fieldSet, 
                                                           location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxUnionTypeDefinitionNode ParseUnionTypeDefinition()
    {
        var location = _tokenizer.Location;
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        return new SyntaxUnionTypeDefinitionNode(UseTopLevelDescription(),
                                                 name,
                                                 ParseDirectivesOptional(),
                                                 ParseMemberTypesOptional(),
                                                 location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxExtendUnionTypeDefinitionNode ParseExtendUnionTypeDefinition()
    {
        var location = _tokenizer.Location;
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        var directives = ParseDirectivesOptional();
        var memberTypes = ParseMemberTypesOptional();

        if (directives.Count == 0 && memberTypes.Count == 0)
            throw SyntaxException.ExtendUnionTypeMissingAtLeastOne(_tokenizer.Location);

        return new SyntaxExtendUnionTypeDefinitionNode(name, 
                                                       directives, 
                                                       memberTypes, 
                                                       location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxEnumTypeDefinitionNode ParseEnumTypeDefinition()
    {
        var location = _tokenizer.Location;
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        return new SyntaxEnumTypeDefinitionNode(UseTopLevelDescription(),
                                                name,
                                                ParseDirectivesOptional(),
                                                ParseEnumValueTypesOptional(),
                                                location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxExtendEnumTypeDefinitionNode ParseExtendEnumTypeDefinition()
    {
        var location = _tokenizer.Location;
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();

        var directives = ParseDirectivesOptional();
        var enumValues = ParseEnumValueTypesOptional();

        if (directives.Count == 0 && enumValues.Count == 0)
            throw SyntaxException.ExtendEnumTypeMissingAtLeastOne(_tokenizer.Location);

        return new SyntaxExtendEnumTypeDefinitionNode(name, 
                                                      directives, 
                                                      enumValues, 
                                                      location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxInputObjectTypeDefinitionNode ParseInputObjectTypeDefinition()
    {
        var location = _tokenizer.Location;
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();
        var directives = ParseDirectivesOptional();

        SyntaxInputValueDefinitionNodeList? inputFields = null;
        if (_tokenizer.TokenKind == DocumentTokenKind.LeftCurlyBracket)
        {
            MandatoryTokenNext(DocumentTokenKind.LeftCurlyBracket);
            inputFields = ParseInputValueListDefinition();
            MandatoryToken(DocumentTokenKind.RightCurlyBracket);
            _tokenizer.Next();
        }

        return new SyntaxInputObjectTypeDefinitionNode(UseTopLevelDescription(),
                                                       name,
                                                       directives,
                                                       inputFields ?? [],
                                                       location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxExtendInputObjectTypeDefinitionNode ParseExtendInputObjectTypeDefinition()
    {
        var location = _tokenizer.Location;
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();
        var directives = ParseDirectivesOptional();

        SyntaxInputValueDefinitionNodeList? inputFields = null;
        if (_tokenizer.TokenKind == DocumentTokenKind.LeftCurlyBracket)
        {
            MandatoryTokenNext(DocumentTokenKind.LeftCurlyBracket);
            inputFields = ParseInputValueListDefinition();
            MandatoryToken(DocumentTokenKind.RightCurlyBracket);
            _tokenizer.Next();
        }

        if (directives.Count == 0 && inputFields is null)
            throw SyntaxException.ExtendInputObjectTypeMissingAtLeastOne(_tokenizer.Location);

        return new SyntaxExtendInputObjectTypeDefinitionNode(name, 
                                                             directives, 
                                                             inputFields ?? [], 
                                                             location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxDirectiveDefinitionNode ParseDirectiveDefinition()
    {
        var location = _tokenizer.Location;
        MandatoryNextToken(DocumentTokenKind.At);
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        MandatoryNext();
        var arguments = ParseArgumentsOptionalDefinition();
        var repeatable = OptionalKeyword("repeatable");
        MandatoryKeyword("on");

        return new SyntaxDirectiveDefinitionNode(UseTopLevelDescription(),
                                                 "@" + name,
                                                 arguments,
                                                 repeatable,
                                                 ParseDirectiveLocations(),
                                                 location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxNameList ParseImplementsInterfacesOptional()
    {
        SyntaxNameList list = [];

        if (OptionalKeyword("implements"))
        {
            OptionalToken(DocumentTokenKind.Ampersand);
            MandatoryToken(DocumentTokenKind.Name);
            list.Add(new SyntaxNameNode(_tokenizer.TokenValue, _tokenizer.Location));
            _tokenizer.Next();

            while (_tokenizer.TokenKind == DocumentTokenKind.Ampersand)
            {
                MandatoryNextToken(DocumentTokenKind.Name);
                list.Add(new SyntaxNameNode(_tokenizer.TokenValue, _tokenizer.Location));
                _tokenizer.Next();
            }
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxVariableDefinitionNodeList ParseVariableslDefinition()
    {
        SyntaxVariableDefinitionNodeList list = [];

        do
        {
            var location = _tokenizer.Location;
            MandatoryTokenNext(DocumentTokenKind.Dollar);
            MandatoryToken(DocumentTokenKind.Name);
            string name = _tokenizer.TokenValue;
            MandatoryNextToken(DocumentTokenKind.Colon);
            MandatoryNext();

            list.Add(new SyntaxVariableDefinitionNode(name,
                                                      ParseType(),
                                                      ParseDefaultValueOptional(),
                                                      ParseDirectivesOptional(),
                                                      location));

        } while (_tokenizer.TokenKind == DocumentTokenKind.Dollar);

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxSelectionDefinitionNodeList ParseSelectionSetOptional()
    {
        if (_tokenizer.TokenKind == DocumentTokenKind.LeftCurlyBracket)
            return ParseSelectionSet();

        return [];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxSelectionDefinitionNodeList ParseSelectionSet()
    {
        SyntaxSelectionDefinitionNodeList list = [];

        MandatoryTokenNext(DocumentTokenKind.LeftCurlyBracket);

        do
        {
            switch (_tokenizer.TokenKind)
            {
                case DocumentTokenKind.Name:
                    {
                        var location = _tokenizer.Location;
                        MandatoryToken(DocumentTokenKind.Name);
                        string alias = "";
                        string name = _tokenizer.TokenValue;
                        MandatoryNext();

                        if (_tokenizer.TokenKind == DocumentTokenKind.Colon)
                        {
                            MandatoryNextToken(DocumentTokenKind.Name);
                            alias = name;
                            name = _tokenizer.TokenValue;
                            MandatoryNext();
                        }

                        list.Add(new SyntaxFieldSelectionNode(alias,
                                                              name,
                                                              ParseArgumentsOptional(constant: false),
                                                              ParseDirectivesOptional(),
                                                              ParseSelectionSetOptional(),
                                                              location));
                    }
                    break;
                case DocumentTokenKind.Spread:
                    {
                        var fragmentLocation = _tokenizer.Location;
                        MandatoryNext();

                        string name = "";
                        if (_tokenizer.TokenKind == DocumentTokenKind.Name)
                        {
                            name = _tokenizer.TokenValue;
                            if (name != "on")
                            {
                                var spreadLocation = _tokenizer.Location;
                                _tokenizer.Next();
                                list.Add(new SyntaxFragmentSpreadSelectionNode(name, ParseDirectivesOptional(), spreadLocation));
                                break;
                            }
                            else
                            {
                                MandatoryNextToken(DocumentTokenKind.Name);
                                name = _tokenizer.TokenValue;
                                _tokenizer.Next();
                            }
                        }

                        list.Add(new SyntaxInlineFragmentSelectionNode(name,
                                                                       ParseDirectivesOptional(),
                                                                       ParseSelectionSet(),
                                                                       fragmentLocation));
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
    private SyntaxFieldDefinitionNodeList ParseFieldsOptionalDefinition()
    {
        SyntaxFieldDefinitionNodeList list = [];

        if (_tokenizer.TokenKind == DocumentTokenKind.LeftCurlyBracket)
        {
            MandatoryNext();

            while (_tokenizer.TokenKind != DocumentTokenKind.RightCurlyBracket)
            {
                var location = _tokenizer.Location;
                var description = OptionalString();
                MandatoryToken(DocumentTokenKind.Name);
                string name = _tokenizer.TokenValue;
                MandatoryNext();
                var arguments = ParseArgumentsOptionalDefinition();
                MandatoryTokenNext(DocumentTokenKind.Colon);

                list.Add(new SyntaxFieldDefinitionNode(description ?? "",
                                                       name,
                                                       arguments,
                                                       ParseType(),
                                                       ParseDirectivesOptional(),
                                                       location));

            } 

            _tokenizer.Next();
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxEnumValueDefinitionList ParseEnumValueTypesOptional()
    {
        SyntaxEnumValueDefinitionList list = [];

        if (_tokenizer.TokenKind == DocumentTokenKind.LeftCurlyBracket)
        {
            MandatoryNext();

            while (_tokenizer.TokenKind != DocumentTokenKind.RightCurlyBracket)
            {
                var location = _tokenizer.Location;
                var description = OptionalString();
                MandatoryToken(DocumentTokenKind.Name);
                string name = _tokenizer.TokenValue;
                MandatoryNext();

                list.Add(new SyntaxEnumValueDefinition(description ?? "",
                                                       name,
                                                       ParseDirectivesOptional(),
                                                       location));
            } 

            _tokenizer.Next();
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxNameList ParseMemberTypesOptional()
    {
        SyntaxNameList list = [];

        if (_tokenizer.TokenKind == DocumentTokenKind.Equals)
        {
            MandatoryNext();
            OptionalToken(DocumentTokenKind.Vertical);
            MandatoryToken(DocumentTokenKind.Name);
            list.Add(new SyntaxNameNode(_tokenizer.TokenValue, _tokenizer.Location));
            _tokenizer.Next();

            while (_tokenizer.TokenKind == DocumentTokenKind.Vertical)
            {
                MandatoryNextToken(DocumentTokenKind.Name);
                list.Add(new SyntaxNameNode(_tokenizer.TokenValue, _tokenizer.Location));
                _tokenizer.Next();
            }
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxInputValueDefinitionNodeList ParseArgumentsOptionalDefinition()
    {
        if (_tokenizer.TokenKind == DocumentTokenKind.LeftParenthesis)
        {
            MandatoryNext();
            var arguments = ParseInputValueListDefinition();
            MandatoryTokenNext(DocumentTokenKind.RightParenthesis);
            return arguments;
        }

        return [];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxInputValueDefinitionNodeList ParseInputValueListDefinition()
    {
        SyntaxInputValueDefinitionNodeList list = [];

        while (_tokenizer.TokenKind == DocumentTokenKind.Name || _tokenizer.TokenKind == DocumentTokenKind.StringValue)
        {
            var location = _tokenizer.Location;
            var description = OptionalString();
            MandatoryToken(DocumentTokenKind.Name);
            string name = _tokenizer.TokenValue;
            MandatoryNextToken(DocumentTokenKind.Colon);
            MandatoryNext();

            list.Add(new SyntaxInputValueDefinitionNode(description ?? "",
                                                        name,
                                                        ParseType(),
                                                        ParseDefaultValueOptional(),
                                                        ParseDirectivesOptional(),
                                                        location));
        } 

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
                    node = new VariableValueNode("$" + _tokenizer.TokenValue);
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
                    ValueNodeList list = [];

                    MandatoryNext();
                    while (_tokenizer.TokenKind != DocumentTokenKind.RightSquareBracket)
                        list.Add(ParseValue(constant: constant));

                    node = new ListValueNode(list);
                }
                break;
            case DocumentTokenKind.LeftCurlyBracket:
                {
                    SyntaxObjectFieldNodeList list = [];

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
    private SyntaxDirectiveNodeList ParseDirectivesOptional()
    {
        SyntaxDirectiveNodeList list = [];

        while (_tokenizer.TokenKind == DocumentTokenKind.At)
        {
            var location = _tokenizer.Location;
            MandatoryNextToken(DocumentTokenKind.Name);
            string name = _tokenizer.TokenValue;
            _tokenizer.Next();

            list.Add(new SyntaxDirectiveNode("@" + name, ParseArgumentsOptional(true), location));
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxObjectFieldNodeList ParseArgumentsOptional(bool constant)
    {
        SyntaxObjectFieldNodeList list = [];

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

            if (_tokenizer.TokenKind == DocumentTokenKind.RightParenthesis)
                _tokenizer.Next();
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxTypeNode ParseType()
    {
        switch (_tokenizer.TokenKind)
        {
            case DocumentTokenKind.Name:
                {
                    var location = _tokenizer.Location;
                    var name = _tokenizer.TokenValue;
                    MandatoryNext();
                    SyntaxTypeNode ret = new SyntaxTypeNameNode(name, location);

                    if (OptionalToken(DocumentTokenKind.Exclamation))
                        ret = new SyntaxTypeNonNullNode(ret, location);

                    return ret;
                }
            case DocumentTokenKind.LeftSquareBracket:
                {
                    var location = _tokenizer.Location; 
                    MandatoryNext();
                    var listType = ParseType();
                    MandatoryTokenNext(DocumentTokenKind.RightSquareBracket);
                    SyntaxTypeNode ret = new SyntaxTypeListNode(listType, location);

                    if (OptionalToken(DocumentTokenKind.Exclamation))
                        ret = new SyntaxTypeNonNullNode(ret, location);

                    return ret;
                }
            default:
                throw SyntaxException.TypeMustBeNameOrList(_tokenizer.Location, _tokenizer.TokenKind.ToString());
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DirectiveLocations ParseDirectiveLocations()
    {
        MandatoryNext();
        OptionalToken(DocumentTokenKind.Vertical);
        MandatoryToken(DocumentTokenKind.Name);
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
    private readonly DirectiveLocations StringToDriveLocations(string location)
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
        return description ?? "";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly OperationType OperationTypeFromTokenValue()
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
    private readonly void MandatoryToken(DocumentTokenKind token)
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
    private readonly void MandatoryKeyword(string keyword)
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
