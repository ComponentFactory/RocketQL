namespace RocketQL.Core.Serializers;

public ref struct RequestDeserializer
{
    private DocumentTokenizer _tokenizer;

    public RequestDeserializer(string source, ReadOnlySpan<char> text)
    {
        _tokenizer = new DocumentTokenizer(source, text);
    }

    public RequestNode Deserialize()
    {
        OperationDefinitionNodeList _operations = new();
        FragmentDefinitionNodeList _fragments = new();

        // Move to the first real token
        _tokenizer.Next();

        // Keep processing until we reach the end of the tokens or throw because of an exception
        while (_tokenizer.TokenKind != DocumentTokenKind.EndOfText)
        {
            switch (_tokenizer.TokenKind)
            {
                case DocumentTokenKind.Name:
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
                        case "scalar":
                        case "type":
                        case "interface":
                        case "union":
                        case "enum":
                        case "input":
                        case "directive":
                            throw SyntaxException.DefinintionNotAllowedInOperation(_tokenizer.Location, _tokenizer.TokenValue);
                        case "extend":
                            {
                                MandatoryNextToken(DocumentTokenKind.Name);
                                switch (_tokenizer.TokenValue)
                                {
                                    case "schema":
                                    case "scalar":
                                    case "type":
                                    case "interface":
                                    case "union":
                                    case "enum":
                                    case "input":
                                        throw SyntaxException.ExtendDefinintionNotAllowedInOperation(_tokenizer.Location, _tokenizer.TokenValue);
                                    default:
                                        throw SyntaxException.UnrecognizedKeyword(_tokenizer.Location, _tokenizer.TokenValue);
                                }
                            }
                        default:
                            throw SyntaxException.UnrecognizedKeyword(_tokenizer.Location, _tokenizer.TokenValue);
                    }
                    break;
                case DocumentTokenKind.LeftCurlyBracket:
                    _operations.Add(new OperationDefinitionNode(OperationType.QUERY, string.Empty, new(), new(), ParseSelectionSet()));
                    break;
                default:
                    throw SyntaxException.UnrecognizedToken(_tokenizer.Location, _tokenizer.TokenKind.ToString());
            }
        }

        return new RequestNode(_operations, _fragments);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private OperationDefinitionNode ParseOperationDefinition()
    {
        OperationType operationType = OperationTypeFromTokenValue();
        MandatoryNext();

        string name = string.Empty;
        if (_tokenizer.TokenKind == DocumentTokenKind.Name)
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
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        if (name == "on")
            throw SyntaxException.FragmentNameCannotBeOn(_tokenizer.Location);

        MandatoryNext();
        MandatoryKeyword("on");
        MandatoryNextToken(DocumentTokenKind.Name);
        string typeCondition = _tokenizer.TokenValue;
        MandatoryNext();

        return new FragmentDefinitionNode(name, typeCondition, ParseDirectivesOptional(), ParseSelectionSet());
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
