namespace DotNetQL.Parser;

public ref struct Tokenizer
{
    private static readonly TokenKind[] _map = new TokenKind[65536];
    private static readonly TokenKind[] _hex = new TokenKind[65536];
    private static readonly string _spaces = new(' ', 256);

    private char _c = ' ';
    private int _index = 0;
    private readonly int _length = 0;
    private int _lineIndex = 0;
    private int _lineNumber = 1;
    private int _tokenIndex = 0;
    private TokenKind _tokenKind = TokenKind.StartOfText;
    private StringBuilder _sb = new();
    private readonly ReadOnlySpan<char> _text;
    private readonly ReadOnlySpan<char> _whitespace = _spaces;

    static Tokenizer()
    {
        // All _map and _hex entries are implicitly TokenKind.IllegalCharacter because it
        // has enumeration value of 0 and the arrays are initialized to all 0 by default

        // Skip the Byte Order Mark (BOM) and simple whitespace
        _map[0xFEFF] = TokenKind.Skip;
        _map[' '] = TokenKind.Skip;
        _map['\t'] = TokenKind.Skip;
        _map[','] = TokenKind.Skip;

        // Line terminators
        _map['\n'] = TokenKind.NewLine;
        _map['\r'] = TokenKind.CarriageReturn;

        // Single line comment
        _map['#'] = TokenKind.Hash;

        // Punctuators
        _map['!'] = TokenKind.Exclamation;
        _map['$'] = TokenKind.Dollar;
        _map['&'] = TokenKind.Ampersand;
        _map['('] = TokenKind.LeftParenthesis;
        _map[')'] = TokenKind.RightParenthesis;
        _map[':'] = TokenKind.Colon;
        _map['='] = TokenKind.Equals;
        _map['@'] = TokenKind.At;
        _map['['] = TokenKind.LeftSquareBracket;
        _map[']'] = TokenKind.RightSquareBracket;
        _map['{'] = TokenKind.LeftCurlyBracket;
        _map['}'] = TokenKind.RightCurlyBracket;
        _map['|'] = TokenKind.Vertical;
        _map['.'] = TokenKind.Dot;

        // Names (letters and numbers)
        _map['_'] = TokenKind.Underscore;
        for (char c = 'A'; c <= 'Z'; c++)
            _map[c] = TokenKind.Letter;
        for (char c = 'a'; c <= 'z'; c++)
            _map[c] = TokenKind.Letter;
        for (char c = '0'; c <= '9'; c++)
            _map[c] = TokenKind.Digit;

        // Signs for numbers
        _map['-'] = TokenKind.Minus;
        _map['+'] = TokenKind.Plus;

        // Strings
        _map['"'] = TokenKind.DoubleQuote;

        // Hexadecimal values
        for (char c = 'A'; c <= 'F'; c++)
            _hex[c] = TokenKind.Hexadecimal;
        for (char c = 'a'; c <= 'f'; c++)
            _hex[c] = TokenKind.Hexadecimal;
        for (char c = '0'; c <= '9'; c++)
            _hex[c] = TokenKind.Hexadecimal;
        _hex['}'] = TokenKind.RightCurlyBracket;
    }

    public Tokenizer(ReadOnlySpan<char> text)
    {
        if (text.Length == 0)
            _tokenKind = TokenKind.EndOfText;
        else
        {
            _text = text;
            _length = _text.Length;
            _tokenKind = TokenKind.StartOfText;
        }
    }

    public TokenKind Token => _tokenKind;
    public int LineNumber => _lineNumber;
    public int ColumnNumber => 1 + _tokenIndex - _lineIndex;
    public string TokenValue => new(_text.Slice(_tokenIndex, _index - _tokenIndex));
    public string TokenString => _sb.ToString();

    public TokenKind Next()
    {
        while(_index < _length)
        {
            _c = _text[_index++];
            TokenKind token = _map[_c];

            switch (token)
            {
                case TokenKind.IllegalCharacter:
                    throw new ApplicationException("IllegalCharacter");
                case TokenKind.Skip:
                    break;
                case TokenKind.NewLine:
                    ScanNewLine();
                    break;
                case TokenKind.CarriageReturn:
                    ScanCarriageReturn();
                    break;
                case TokenKind.Hash:
                    ScanComment();
                    break;
                case TokenKind.Exclamation:
                case TokenKind.Dollar:
                case TokenKind.Ampersand:
                case TokenKind.LeftParenthesis:
                case TokenKind.RightParenthesis:
                case TokenKind.Colon:
                case TokenKind.Equals:
                case TokenKind.At:
                case TokenKind.LeftSquareBracket:
                case TokenKind.RightSquareBracket:
                case TokenKind.LeftCurlyBracket:
                case TokenKind.RightCurlyBracket:
                case TokenKind.Vertical:
                    return ScanPunctuator(token);
                case TokenKind.Dot:
                    return ScanSpread();
                case TokenKind.Underscore:
                case TokenKind.Letter:
                    return ScanName();
                case TokenKind.Digit:
                case TokenKind.Minus:
                    return ScanNumber();
                case TokenKind.DoubleQuote:
                    return ScanString();
                default:
                    throw new ApplicationException($"Unrecognized '{_c}'");
            }
        }

        _tokenIndex = _index;
        return _tokenKind = TokenKind.EndOfText;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ScanNewLine()
    {
        _lineNumber++;
        _lineIndex = _index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ScanCarriageReturn()
    {
        // Skip over any optional following newline
        if ((_index < _length) && (_text[_index] == '\n'))
            _index++;

        _lineNumber++;
        _lineIndex = _index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ScanComment()
    {
        // Single line comment, ignore everything until we reach the end of the line
        while ((_index < _length) && (_text[_index] != '\r') && (_text[_index] != '\n'))
            _index++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TokenKind ScanPunctuator(TokenKind token)
    {
        _tokenIndex = _index - 1;
        return _tokenKind = token;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TokenKind ScanSpread()
    {
        if ((_index + 1) >= _length)
            throw new ApplicationException($"Unexpected end of file.");

        if ((_text[_index] == '.') && (_text[_index + 1] == '.'))
        {
            _tokenIndex = _index - 1;
            _index += 2;
            return _tokenKind = TokenKind.Spread;
        }

        throw new ApplicationException($"Spread operator requires 3 dots in sequence.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TokenKind ScanName()
    {
        _tokenIndex = _index - 1;
        while (_index < _length)
        {
            switch (_map[_text[_index]])
            {
                case TokenKind.Underscore:
                case TokenKind.Letter:
                case TokenKind.Digit:
                    _index++;
                    continue;
            }

            return _tokenKind = TokenKind.Name;
        }

        return _tokenKind = TokenKind.Name;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TokenKind ScanNumber()
    {
        _tokenIndex = _index - 1;
        if (_c == '-')
        {
            if (_index == _length)
                throw new ApplicationException($"Unexpected end of file.");

            _c = _text[_index++];
            if (_map[_c] != TokenKind.Digit)
                throw new ApplicationException($"Minus must be followed by a digit.");
        }

        if (_c != '0')
        {
            // Skip over all the whole number digits
            while ((_index < _length) && (_map[_text[_index]] == TokenKind.Digit))
                _index++;
        }

        if (_index == _length)
            return _tokenKind = TokenKind.IntValue;

        _c = _text[_index];
        if (_c == '.')
        {
            if (++_index == _length)
                throw new ApplicationException($"Unexpected end of file.");

            _c = _text[_index++];
            if (_map[_c] != TokenKind.Digit)
                throw new ApplicationException($"Decimal point must be followed by a digit.");

            while ((_index < _length) && (_map[_text[_index]] == TokenKind.Digit))
                _index++;

            if (_index == _length)
                return _tokenKind = TokenKind.FloatValue;

            _c = _text[_index];
            if ((_c == 'e') || (_c == 'E'))
            {
                if (++_index == _length)
                    throw new ApplicationException($"Unexpected end of file.");

                _c = _text[_index++];
                if ((_map[_c] == TokenKind.Minus) || (_map[_c] == TokenKind.Plus))
                {
                    if (_index == _length)
                        throw new ApplicationException($"Unexpected end of file.");

                    _c = _text[_index++];
                }

                if (_map[_c] != TokenKind.Digit)
                    throw new ApplicationException($"Exponent must be followed by a digit.");

                while ((_index < _length) && (_map[_text[_index]] == TokenKind.Digit))
                    _index++;

                if (_index == _length)
                    return _tokenKind = TokenKind.FloatValue;

                switch (_map[_text[_index]])
                {
                    case TokenKind.Dot:
                        throw new ApplicationException($"Float value cannot be followed by a decimal point.");
                    case TokenKind.Underscore:
                        throw new ApplicationException($"Float value cannot be followed by an underscore.");
                    case TokenKind.Letter:
                        throw new ApplicationException($"Float value cannot be followed by a letter.");
                }
            }

            return _tokenKind = TokenKind.FloatValue;
        }
        else if ((_c == 'e') || (_c == 'E'))
        {
            if (++_index == _length)
                throw new ApplicationException($"Unexpected end of file.");

            _c = _text[_index++];
            if ((_map[_c] == TokenKind.Minus) || (_map[_c] == TokenKind.Plus))
            {
                if (_index == _length)
                    throw new ApplicationException($"Unexpected end of file.");

                _c = _text[_index++];
            }

            if (_map[_c] != TokenKind.Digit)
                throw new ApplicationException($"Exponent must be followed by a digit.");

            while (_index < _length && _map[_text[_index]] == TokenKind.Digit)
                _index++;

            if (_index == _length)
                return _tokenKind = TokenKind.FloatValue;

            switch (_map[_text[_index]])
            {
                case TokenKind.Dot:
                    throw new ApplicationException($"Float value cannot be followed by a decimal point.");
                case TokenKind.Underscore:
                    throw new ApplicationException($"Float value cannot be followed by an underscore.");
                case TokenKind.Letter:
                    throw new ApplicationException($"Float value cannot be followed by a letter.");
            }

            return _tokenKind = TokenKind.FloatValue;
        }
        else if (_c == '_')
            throw new ApplicationException($"Integer value cannot be followed by an underscore.");
        else if (_map[_c] == TokenKind.Letter)
            throw new ApplicationException($"Integer value cannot be followed by a letter.");
        else if (_map[_c] == TokenKind.Digit)
            throw new ApplicationException($"Numbers cannot have a leading zero.");

        return _tokenKind = TokenKind.IntValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TokenKind ScanString()
    {
        if (((_index + 1) < _length) && (_text[_index] == '"') && (_text[_index + 1] == '"'))
            return ScanBlockString();
        else
            return ScanSimpleString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TokenKind ScanBlockString()
    {
        _index += 2;
        _tokenIndex = _index;

        // Common indent found across all non-whitespace and non-first line
        int indent = int.MaxValue;

        // Start of the current line being scanned
        int currentLine = _tokenIndex;

        // Is the current line only whitespace characters
        bool onlyWhitespace = true;

        // Points to the first and last real lines, a real line has at least 1 non-whitespace character
        int firstRealLine = currentLine;
        int lastRealLine = currentLine;

        // Points to the last non-whitespace character scanned
        int lastRealChar = currentLine;

        // We always start by scanning the first line
        bool isFirstLine = true;

        // Are we still looking for the first real line, the first line that is not just whitespace
        bool findingFirstReal = true;

        while (_index < _length)
        {
            switch (_map[_text[_index++]])
            {
                case TokenKind.Skip:
                    break;
                case TokenKind.NewLine:
                    currentLine = _index;

                    if (findingFirstReal)
                    {
                        if (onlyWhitespace)
                            firstRealLine = currentLine;
                        else
                            findingFirstReal = false;
                    }

                    if (lastRealChar > lastRealLine)
                        lastRealLine = _index - 1;

                    isFirstLine = false;
                    onlyWhitespace = true;
                    break;
                case TokenKind.CarriageReturn:
                    // TODO
                    break;
                case TokenKind.DoubleQuote:
                    if (((_index + 1) < _length) && (_text[_index] == '"') && (_text[_index + 1] == '"'))
                        return GenerateStringForBlock(firstRealLine, lastRealChar, lastRealLine, indent);

                    break;
                default:
                    lastRealChar = _index;
                    if (onlyWhitespace)
                    {
                        if (!isFirstLine)
                            indent = Math.Min(indent, _index - currentLine - 1);

                        onlyWhitespace = false;
                    }
                    break;
            }
        }

        throw new ApplicationException($"Unexpected end of file.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TokenKind GenerateStringForBlock(int firstRealLine, int lastRealChar, int lastRealLine, int indent)
    {
        _sb.Clear();

        // If the three quotes are preceded by a backslash then we want to ignore the backslash
        int endIndex = _index - 1;
        if (_text[endIndex - 1] == '\\')
            endIndex--;

        // If the last real character is within the last line, we need to process the entire last line
        if (lastRealChar > lastRealLine)
            lastRealLine = endIndex;

        // If one of more of the ending lines are empty, we do not need to process them
        if (lastRealLine < endIndex)
            endIndex = lastRealLine;

        int currentLine = firstRealLine;
        bool onlyWhitespace = true;

        for (int i = firstRealLine; i < endIndex; i++)
        {
            switch (_map[_text[i]])
            {
                case TokenKind.Skip:
                    break;
                case TokenKind.NewLine:
                    AppendLineToBlock(currentLine, firstRealLine, i, onlyWhitespace, indent);

                    firstRealLine = i + 1;
                    currentLine = firstRealLine;
                    onlyWhitespace = true;
                    break;
                case TokenKind.CarriageReturn:
                    // TODO
                    break;
                default:
                    if (onlyWhitespace)
                    {
                        firstRealLine = i;
                        onlyWhitespace = false;
                    }
                    break;
            }
        }

        // Process any remaining line
        if (firstRealLine < endIndex)
            AppendLineToBlock(currentLine, firstRealLine, endIndex, onlyWhitespace, indent);

        _index += 2;
        return _tokenKind = TokenKind.StringValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendLineToBlock(int currentLine, int firstRealLine, int endIndex, bool onlyWhitespace, int indent)
    {
        if (_sb.Length > 0)
            _sb.Append('\n');

        // The first line never has indent applied
        if (currentLine == _tokenIndex)
            _sb.Append(_text[_tokenIndex..endIndex]);
        else if (onlyWhitespace)
            AppendCommonIndent(endIndex, currentLine, indent);
        else
        {
            AppendCommonIndent(firstRealLine, currentLine, indent);
            _sb.Append(_text[firstRealLine..endIndex]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendCommonIndent(int index, int currentLine, int indent)
    {
        int len = index - currentLine - indent;
        if (len > 0)
        {
            if (len <= _whitespace.Length)
                _sb.Append(_whitespace[..len]);
            else
                _sb.Append(new string(' ', len));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TokenKind ScanSimpleString()
    {
        _tokenIndex = _index;
        while (_index < _length)
        {
            _c = _text[_index];
            if (_c == '"')
            {
                _index++;
                return _tokenKind = TokenKind.StringValue;
            }
            else if (_c == '\\')
            {
                if (++_index == _length)
                    throw new ApplicationException($"Unexpected end of file.");

                _c = _text[_index];
                switch (_c)
                {
                    case '\"':
                    case '\\':
                    case '/':
                    case 'b':
                    case 'f':
                    case 'n':
                    case 'r':
                    case 't':
                        break;
                    case 'u':
                        _index++;
                        if (_index == _length)
                            throw new ApplicationException($"Unexpected end of file.");

                        _c = _text[_index];
                        if (_c == '{')
                        {
                            int digits = 0;
                            while (true)
                            {
                                if (++_index == _length)
                                    throw new ApplicationException($"Unexpected end of file.");

                                _c = _text[_index];
                                TokenKind hexToken = _hex[_c];
                                if (hexToken == TokenKind.Hexadecimal)
                                    digits++;
                                else
                                {
                                    if (hexToken == TokenKind.RightCurlyBracket)
                                    {
                                        if (digits == 0)
                                            throw new ApplicationException($"Escaped character must have at least 1 hexadecimal digit.");

                                        break;
                                    }
                                    else
                                        throw new ApplicationException($"Escape code specify value using hexadecimal character.");
                                }
                            }
                        }
                        else
                        {
                            if ((_index + 3) >= _length)
                                throw new ApplicationException($"Unexpected end of file.");

                            TokenKind hexToken1 = _hex[_c];
                            TokenKind hexToken2 = _hex[_text[_index++]];
                            TokenKind hexToken3 = _hex[_text[_index++]];
                            TokenKind hexToken4 = _hex[_text[_index++]];
                            if ((hexToken1 != TokenKind.Hexadecimal) || (hexToken2 != TokenKind.Hexadecimal) || (hexToken3 != TokenKind.Hexadecimal) || (hexToken4 != TokenKind.Hexadecimal))
                                throw new ApplicationException($"Escape code specify value using hexadecimal character.");
                        }
                        break;
                    default:
                        throw new ApplicationException($"Escaped character is not one of '\"\t\\\t/\tb\tf\tn\tr\tt\r\n'.");
                }
            }

            _index++;
        }

        throw new ApplicationException($"Unexpected end of file.");
    }
}
