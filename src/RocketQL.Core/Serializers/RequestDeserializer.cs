using System.Xml.Linq;

namespace RocketQL.Core.Serializers;

public ref struct RequestDeserializer
{
    private readonly SyntaxNodeList _nodes;
    private DocumentTokenizer _tokenizer;

    public RequestDeserializer(ReadOnlySpan<char> text,
                               [CallerFilePath] string filePath = "",
                               [CallerMemberName] string memberName = "",
                               [CallerLineNumber] int lineNumber = 0)
        : this(text, CallerExtensions.CallerToSource(filePath, memberName, lineNumber))
    {
    }

    public RequestDeserializer(ReadOnlySpan<char> text, string source)
    {
        _nodes = [];
        _tokenizer = new DocumentTokenizer(text, source);
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
                case DocumentTokenKind.Name:
                    switch (_tokenizer.TokenValue)
                    {
                        case "query":
                        case "mutation":
                        case "subscription":
                            _nodes.Add(ParseOperationDefinition());
                            break;
                        case "fragment":
                            _nodes.Add(ParseFragmentDefinition());
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
                    _nodes.Add(new SyntaxOperationDefinitionNode(OperationType.QUERY, "", [], [], ParseSelectionSet(), _tokenizer.Location));
                    break;
                default:
                    throw SyntaxException.UnrecognizedToken(_tokenizer.Location, _tokenizer.TokenKind.ToString());
            }
        }

        return _nodes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxOperationDefinitionNode ParseOperationDefinition()
    {
        var location = _tokenizer.Location;
        OperationType operationType = OperationTypeFromTokenValue();
        MandatoryNext();

        string name = "";
        if (_tokenizer.TokenKind == DocumentTokenKind.Name)
        {
            name = _tokenizer.TokenValue;
            MandatoryNext();
        }

        return new SyntaxOperationDefinitionNode(operationType,
                                                 name,
                                                 ParseVariablesOptionalDefinition(),
                                                 ParseDirectivesOptional(),
                                                 ParseSelectionSet(),
                                                 location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxFragmentDefinitionNode ParseFragmentDefinition()
    {
        var location = _tokenizer.Location;
        MandatoryNextToken(DocumentTokenKind.Name);
        string name = _tokenizer.TokenValue;
        if (name == "on")
            throw SyntaxException.FragmentNameCannotBeOn(_tokenizer.Location);

        MandatoryNext();
        MandatoryKeyword("on");
        MandatoryNextToken(DocumentTokenKind.Name);
        string typeCondition = _tokenizer.TokenValue;
        MandatoryNext();

        return new SyntaxFragmentDefinitionNode(name, 
                                                typeCondition, 
                                                ParseDirectivesOptional(), 
                                                ParseSelectionSet(), 
                                                location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxVariableDefinitionNodeList ParseVariablesOptionalDefinition()
    {
        if (_tokenizer.TokenKind == DocumentTokenKind.LeftParenthesis)
        {
            MandatoryNext();
            var arguments = ParseVariablesDefinition();
            MandatoryTokenNext(DocumentTokenKind.RightParenthesis);
            return arguments;
        }

        return new();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxVariableDefinitionNodeList ParseVariablesDefinition()
    {
        SyntaxVariableDefinitionNodeList list = new();

        do
        {
            var location = _tokenizer.Location;
            MandatoryTokenNext(DocumentTokenKind.Dollar);
            MandatoryToken(DocumentTokenKind.Name);
            string name = _tokenizer.TokenValue;
            MandatoryNextToken(DocumentTokenKind.Colon);
            MandatoryNext();

            list.Add(new SyntaxVariableDefinitionNode("$" + name,
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

        return new();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxSelectionDefinitionNodeList ParseSelectionSet()
    {
        SyntaxSelectionDefinitionNodeList list = new();

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
                    ValueNodeList list = new();

                    MandatoryNext();
                    while (_tokenizer.TokenKind != DocumentTokenKind.RightSquareBracket)
                        list.Add(ParseValue(constant: constant));

                    node = new ListValueNode(list);
                }
                break;
            case DocumentTokenKind.LeftCurlyBracket:
                {
                    SyntaxObjectFieldNodeList list = new();

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
        SyntaxDirectiveNodeList list = new();

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
        SyntaxObjectFieldNodeList list = new();

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
