namespace RocketQL.Core;

public ref struct GraphQLTokenizer
{

    private static readonly FullTokenKind[] _mapKind = new FullTokenKind[65536];
    private static readonly FullTokenKind[] _hexKind = new FullTokenKind[65536];
    private static readonly byte[] _hexValues = new byte[65536];
    private static readonly EscapeKind[] _escKind = new EscapeKind[65536];
    private static readonly char[] _escChar = new char[(int)EscapeKind.u];
    private static readonly ThreadLocal<StringBuilder> _cachedBuilder = new(() => new(4096));

    private readonly ReadOnlySpan<char> _text;
    private readonly int _length = 0;
    private readonly StringBuilder _sb;
    private FullTokenKind _tokenKind = FullTokenKind.StartOfText;
    private char _c = ' ';
    private int _index = 0;
    private int _lineIndex = 0;
    private int _lineNumber = 1;
    private int _tokenIndex = 0;

    static GraphQLTokenizer()
    {
        // All _mapKind and _hexKind entries are implicitly FullTokenKind.IllegalCharacter because
        // ithas the enumeration value of 0 and the arrays are initialized to all 0 by default

        // Skip the Byte Order Mark (BOM) and simple whitespace
        _mapKind[0xFEFF] = FullTokenKind.Skip;
        _mapKind[' '] = FullTokenKind.Skip;
        _mapKind['\t'] = FullTokenKind.Skip;
        _mapKind[','] = FullTokenKind.Skip;

        // Individual characters, mostly punctuators
        _mapKind['\n'] = FullTokenKind.NewLine;
        _mapKind['\r'] = FullTokenKind.CarriageReturn;
        _mapKind['#'] = FullTokenKind.Hash;
        _mapKind['!'] = FullTokenKind.Exclamation;
        _mapKind['$'] = FullTokenKind.Dollar;
        _mapKind['&'] = FullTokenKind.Ampersand;
        _mapKind['('] = FullTokenKind.LeftParenthesis;
        _mapKind[')'] = FullTokenKind.RightParenthesis;
        _mapKind[':'] = FullTokenKind.Colon;
        _mapKind['='] = FullTokenKind.Equals;
        _mapKind['@'] = FullTokenKind.At;
        _mapKind['['] = FullTokenKind.LeftSquareBracket;
        _mapKind[']'] = FullTokenKind.RightSquareBracket;
        _mapKind['{'] = FullTokenKind.LeftCurlyBracket;
        _mapKind['}'] = FullTokenKind.RightCurlyBracket;
        _mapKind['|'] = FullTokenKind.Vertical;
        _mapKind['.'] = FullTokenKind.Dot;
        _mapKind['-'] = FullTokenKind.Minus;
        _mapKind['+'] = FullTokenKind.Plus;
        _mapKind['"'] = FullTokenKind.DoubleQuote;

        // Letters and numbers
        _mapKind['_'] = FullTokenKind.Underscore;
        for (char c = 'A'; c <= 'Z'; c++)
            _mapKind[c] = FullTokenKind.Letter;
        for (char c = 'a'; c <= 'z'; c++)
            _mapKind[c] = FullTokenKind.Letter;
        for (char c = '0'; c <= '9'; c++)
            _mapKind[c] = FullTokenKind.Digit;

        // Hexadecimal characters
        for (char c = 'A'; c <= 'F'; c++)
            _hexKind[c] = FullTokenKind.Hexadecimal;
        for (char c = 'a'; c <= 'f'; c++)
            _hexKind[c] = FullTokenKind.Hexadecimal;
        for (char c = '0'; c <= '9'; c++)
            _hexKind[c] = FullTokenKind.Hexadecimal;
        _hexKind['}'] = FullTokenKind.RightCurlyBracket;

        // Hexadecimal values
        for (char c = 'A'; c <= 'F'; c++)
            _hexValues[c] = (byte)(c + 10 - 'A');
        for (char c = 'a'; c <= 'f'; c++)
            _hexValues[c] = (byte)(c + 10 - 'a');
        for (char c = '0'; c <= '9'; c++)
            _hexValues[c] = (byte)(c - '0');

        // Escape characters in simple strings
        _escKind['\"'] = EscapeKind.DoubleQuote;
        _escKind['\\'] = EscapeKind.Backslash;
        _escKind['/'] = EscapeKind.Slash;
        _escKind['b'] = EscapeKind.Backspace;
        _escKind['f'] = EscapeKind.Feed;
        _escKind['n'] = EscapeKind.NewLine;
        _escKind['r'] = EscapeKind.CarriageReturn;
        _escKind['t'] = EscapeKind.Tab;
        _escKind['u'] = EscapeKind.u;

        _escChar[(int)EscapeKind.DoubleQuote] = '\"';
        _escChar[(int)EscapeKind.Backslash] = '\\';
        _escChar[(int)EscapeKind.Slash] = '/';
        _escChar[(int)EscapeKind.Backspace] = '\b';
        _escChar[(int)EscapeKind.Feed] = '\f';
        _escChar[(int)EscapeKind.NewLine] = '\n';
        _escChar[(int)EscapeKind.CarriageReturn] = '\r';
        _escChar[(int)EscapeKind.Tab] = '\t';
    }

    public GraphQLTokenizer(string text)
        : this(text.AsSpan())
    {
    }

    public GraphQLTokenizer(ReadOnlySpan<char> text)
    {
        _sb = _cachedBuilder.Value!;

        if (text.Length == 0)
            _tokenKind = FullTokenKind.EndOfText;
        else
        {
            _text = text;
            _length = _text.Length;
            _tokenKind = FullTokenKind.StartOfText;
        }
    }

    public GraphQLTokenKind TokenKind => (GraphQLTokenKind)_tokenKind;
    public string TokenValue => new(_text.Slice(_tokenIndex, _index - _tokenIndex));
    public string TokenString => _sb.ToString();
    public int LineNumber => _lineNumber;
    public int ColumnNumber => 1 + _tokenIndex - _lineIndex;
    public Location Location => new(_index, LineNumber, ColumnNumber);

    public bool Next()
    {
        while(_index < _length)
        {
            _c = _text[_index++];
            FullTokenKind token = _mapKind[_c];

            switch (token)
            {
                case FullTokenKind.IllegalCharacter:
                    _tokenIndex = _index - 1;
                    throw SyntaxException.IllegalCharacterCode(Location, _c);
                case FullTokenKind.Skip:
                    break;
                case FullTokenKind.NewLine:
                    ScanNewLine();
                    break;
                case FullTokenKind.CarriageReturn:
                    ScanCarriageReturn();
                    break;
                case FullTokenKind.Hash:
                    ScanComment();
                    break;
                case FullTokenKind.Exclamation:
                case FullTokenKind.Dollar:
                case FullTokenKind.Ampersand:
                case FullTokenKind.LeftParenthesis:
                case FullTokenKind.RightParenthesis:
                case FullTokenKind.Colon:
                case FullTokenKind.Equals:
                case FullTokenKind.At:
                case FullTokenKind.LeftSquareBracket:
                case FullTokenKind.RightSquareBracket:
                case FullTokenKind.LeftCurlyBracket:
                case FullTokenKind.RightCurlyBracket:
                case FullTokenKind.Vertical:
                    ScanPunctuator(token);
                    return true;
                case FullTokenKind.Dot:
                    ScanSpread();
                    return true;
                case FullTokenKind.Underscore:
                case FullTokenKind.Letter:
                    ScanName();
                    return true;
                case FullTokenKind.Digit:
                case FullTokenKind.Minus:
                    ScanNumber();
                    return true;
                case FullTokenKind.DoubleQuote:
                    ScanString();
                    return true;
                default:
                    _tokenIndex = _index - 1;
                    throw SyntaxException.UnrecognizedCharacterCode(Location, _c);
            }
        }

        _tokenIndex = _index;
        _tokenKind = FullTokenKind.EndOfText;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanNewLine()
    {
        _lineNumber++;
        _lineIndex = _index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanCarriageReturn()
    {
        // Skip over any optional following newline
        if ((_index < _length) && (_text[_index] == '\n'))
            _index++;

        _lineNumber++;
        _lineIndex = _index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanComment()
    {
        // Single line comment, ignore everything until we reach the end of the line
        while ((_index < _length) && (_text[_index] != '\r') && (_text[_index] != '\n'))
            _index++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanPunctuator(FullTokenKind token)
    {
        _tokenIndex = _index - 1;
        _tokenKind = token;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanSpread()
    {
        _tokenIndex = _index - 1;

        if ((_index + 1) >= _length)
        {
            _index = _length;
            throw SyntaxException.UnexpectedEndOfFile(Location);
        }

        if ((_text[_index] == '.') && (_text[_index + 1] == '.'))
        {
            _index += 2;
            _tokenKind = FullTokenKind.Spread;
        }
        else
        {
            if (_text[_index] == '.')
                _index++;

            throw SyntaxException.SpreadNeedsThreeDots(Location);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanName()
    {
        _tokenIndex = _index - 1;
        while (_index < _length)
        {
            switch (_mapKind[_text[_index]])
            {
                case FullTokenKind.Underscore:
                case FullTokenKind.Letter:
                case FullTokenKind.Digit:
                    _index++;
                    continue;
            }

            break;
        }

        _tokenKind = FullTokenKind.Name;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanNumber()
    {
        _tokenIndex = _index - 1;
        if (_c == '-')
        {
            if (_index == _length)
                throw SyntaxException.UnexpectedEndOfFile(Location);

            _c = _text[_index];
            if (_mapKind[_c] != FullTokenKind.Digit)
                throw SyntaxException.MinusMustBeFollowedByDigit(Location);

            _index++;
        }

        if (_c != '0')
        {
            // Skip over all the whole number digits
            while ((_index < _length) && (_mapKind[_text[_index]] == FullTokenKind.Digit))
                _index++;
        }

        if (_index == _length)
        {
            _tokenKind = FullTokenKind.IntValue;
            return;
        }

        _c = _text[_index];
        if (_c == '.')
        {
            if (++_index == _length)
                throw SyntaxException.UnexpectedEndOfFile(Location);

            _c = _text[_index];
            if (_mapKind[_c] != FullTokenKind.Digit)
                throw SyntaxException.PointMustBeFollowedByDigit(Location);

            _index++;
            while ((_index < _length) && (_mapKind[_text[_index]] == FullTokenKind.Digit))
                _index++;

            if (_index < _length)
            {
                _c = _text[_index];
                if ((_c == 'e') || (_c == 'E'))
                    ScanFloatExponent();
            }

            _tokenKind = FullTokenKind.FloatValue;
            return;
        }
        else if ((_c == 'e') || (_c == 'E'))
            ScanFloatExponent();
        else
        {
            switch (_mapKind[_text[_index]])
            {
                case FullTokenKind.Underscore:
                case FullTokenKind.Letter:
                    throw SyntaxException.IntCannotBeFollowed(Location, _mapKind[_c].ToString());
            }

            _tokenKind = FullTokenKind.IntValue;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanFloatExponent()
    {
        if (++_index == _length)
            throw SyntaxException.UnexpectedEndOfFile(Location);

        _c = _text[_index++];
        if ((_mapKind[_c] == FullTokenKind.Minus) || (_mapKind[_c] == FullTokenKind.Plus))
        {
            if (_index == _length)
                throw SyntaxException.UnexpectedEndOfFile(Location);

            _c = _text[_index++];
        }

        if (_mapKind[_c] != FullTokenKind.Digit)
        {
            _index--;
            throw SyntaxException.ExponentMustHaveDigit(Location);
        }

        while (_index < _length && _mapKind[_text[_index]] == FullTokenKind.Digit)
            _index++;

        if (_index == _length)
        {
            _tokenKind = FullTokenKind.FloatValue;
            return;
        }

        switch (_mapKind[_text[_index]])
        {
            case FullTokenKind.Dot:
            case FullTokenKind.Underscore:
            case FullTokenKind.Letter:
                throw SyntaxException.FloatCannotBeFollowed(Location, _mapKind[_text[_index]].ToString());
        }

        _tokenKind = FullTokenKind.FloatValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanString()
    {
        if (((_index + 1) < _length) && (_text[_index] == '"') && (_text[_index + 1] == '"'))
            ScanBlockString();
        else
            ScanSimpleString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanBlockString()
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
            switch (_mapKind[_text[_index++]])
            {
                case FullTokenKind.Skip:
                    break;
                case FullTokenKind.NewLine:
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
                case FullTokenKind.CarriageReturn:
                    // Skip over any optional following newline
                    int lastLine = _index;
                    if ((_index < _length) && (_text[_index] == '\n'))
                        _index++;

                    currentLine = _index;

                    if (findingFirstReal)
                    {
                        if (onlyWhitespace)
                            firstRealLine = currentLine;
                        else
                            findingFirstReal = false;
                    }

                    if (lastRealChar > lastRealLine)
                        lastRealLine = lastLine - 1;

                    isFirstLine = false;
                    onlyWhitespace = true;
                    break;
                case FullTokenKind.DoubleQuote:
                    if (((_index + 1) < _length) && (_text[_index] == '"') && (_text[_index + 1] == '"'))
                    {
                        GenerateStringForBlockString(firstRealLine, lastRealChar, lastRealLine, indent);
                        return;
                    }
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

        throw SyntaxException.UnexpectedEndOfFile(Location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateStringForBlockString(int firstRealLine, int lastRealChar, int lastRealLine, int indent)
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
            switch (_mapKind[_text[i]])
            {
                case FullTokenKind.Skip:
                    break;
                case FullTokenKind.NewLine:
                    AppendLineToBlockString(currentLine, firstRealLine, i, onlyWhitespace, indent);

                    firstRealLine = i + 1;
                    currentLine = firstRealLine;
                    onlyWhitespace = true;
                    break;
                case FullTokenKind.CarriageReturn:
                    AppendLineToBlockString(currentLine, firstRealLine, i, onlyWhitespace, indent);

                    // Skip over any optional following newline
                    if ((i < endIndex) && (_text[i + 1] == '\n'))
                        i++;

                    firstRealLine = i + 1;
                    currentLine = firstRealLine;
                    onlyWhitespace = true;
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
            AppendLineToBlockString(currentLine, firstRealLine, endIndex, onlyWhitespace, indent);

        _index += 2;
        _tokenKind = FullTokenKind.StringValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendLineToBlockString(int currentLine, int firstRealLine, int endIndex, bool onlyWhitespace, int indent)
    {
        if (_sb.Length > 0)
            _sb.Append('\n');

        // The first line never has indent applied
        if (currentLine == _tokenIndex)
            _sb.Append(_text[_tokenIndex..endIndex]);
        else if (onlyWhitespace)
        {
            int len = endIndex - currentLine - indent;
            if (len > 0)
                _sb.Append(' ', len);
        }
        else
        {
            int len = firstRealLine - currentLine - indent;
            if (len > 0)
                _sb.Append(' ', len);
            
            _sb.Append(_text[firstRealLine..endIndex]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanSimpleString()
    {
        _sb.Clear();

        _tokenIndex = _index;
        int copyIndex = _tokenIndex;

        while (_index < _length)
        {
            _c = _text[_index];
            if (_c == '"')
            {
                if (copyIndex < _index)
                    _sb.Append(_text[copyIndex.._index]);

                _index++;
                _tokenKind = FullTokenKind.StringValue;
                return;
            }
            else if (_c == '\\')
            {
                if (++_index == _length)
                    throw SyntaxException.UnexpectedEndOfFile(Location);

                _c = _text[_index];
                EscapeKind k =_escKind[_c];
                switch (k)
                {
                    case EscapeKind.DoubleQuote:
                    case EscapeKind.Backslash:
                    case EscapeKind.Slash:
                    case EscapeKind.Backspace:
                    case EscapeKind.Feed:
                    case EscapeKind.NewLine:
                    case EscapeKind.CarriageReturn:
                    case EscapeKind.Tab:
                        // Copy waiting characters, except the previous '\'
                        if (copyIndex < (_index - 1))
                            _sb.Append(_text[copyIndex..(_index - 1)]);

                        _sb.Append(_escChar[(int)k]);
                        copyIndex = _index + 1;
                        break;
                    case EscapeKind.u:
                        _index++;
                        if (_index == _length)
                            throw SyntaxException.UnexpectedEndOfFile(Location);

                        _c = _text[_index];
                        if (_c == '{')
                        {
                            // Copy waiting characters, except the previous '\u'
                            if (copyIndex < (_index - 2))
                                _sb.Append(_text[copyIndex..(_index - 2)]);

                            int digits = 0;
                            while (true)
                            {
                                if (++_index == _length)
                                    throw SyntaxException.UnexpectedEndOfFile(Location);

                                _c = _text[_index];
                                FullTokenKind hexToken = _hexKind[_c];
                                if (hexToken == FullTokenKind.Hexadecimal)
                                    digits++;
                                else
                                {
                                    if (hexToken == FullTokenKind.RightCurlyBracket)
                                    {
                                        if (digits == 0)
                                            throw SyntaxException.EscapeAtLeast1Hex(Location);

                                        try
                                        {
                                            // This format is rare, so just use the builtin conversion because speed does not matter!
                                            uint val = uint.Parse(_text.Slice(_index - digits, digits), System.Globalization.NumberStyles.HexNumber);
                                            _sb.Append((char)val);
                                        }
                                        catch
                                        {
                                            throw SyntaxException.EscapeCannotBeConverted(Location, new string(_text.Slice(_index - digits, digits)));
                                        }

                                        copyIndex = _index + 1;
                                        break;
                                    }
                                    else
                                        throw SyntaxException.EscapeOnlyUsingHex(Location);
                                }
                            }
                        }
                        else
                        {
                            if ((_index + 3) >= _length)
                            {
                                _index = _length;
                                throw SyntaxException.UnexpectedEndOfFile(Location);
                            }

                            FullTokenKind hexToken1 = _hexKind[_text[_index++]];
                            FullTokenKind hexToken2 = _hexKind[_text[_index++]];
                            FullTokenKind hexToken3 = _hexKind[_text[_index++]];
                            FullTokenKind hexToken4 = _hexKind[_text[_index]];
                            if ((hexToken1 != FullTokenKind.Hexadecimal) || (hexToken2 != FullTokenKind.Hexadecimal) ||
                                (hexToken3 != FullTokenKind.Hexadecimal) || (hexToken4 != FullTokenKind.Hexadecimal))
                            {
                                _index++;
                                throw SyntaxException.EscapeOnlyUsingHex(Location);
                            }

                            // Copy waiting characters, except the previous '\uXXXX'
                            if (copyIndex < (_index - 5))
                                _sb.Append(_text[copyIndex..(_index - 5)]);

                            _sb.Append((char)(_hexValues[_text[_index]] | 
                                             (_hexValues[_text[_index - 1]] << 4) | 
                                             (_hexValues[_text[_index - 2]] << 8) | 
                                             (_hexValues[_text[_index - 3]] << 12)));

                            copyIndex = _index + 1;
                        }
                        break;
                    default:
                        throw SyntaxException.EscapeMustBeOneOf(Location);
                }
            }

            _index++;
        }

        throw SyntaxException.UnexpectedEndOfFile(Location);
    }

    private enum FullTokenKind : byte
    {
        IllegalCharacter = 0,
        Skip = 1,
        NewLine = 2,
        CarriageReturn = 3,
        Hash = 4,
        Exclamation = 5,
        Dollar = 6,
        Ampersand = 7,
        LeftParenthesis = 8,
        RightParenthesis = 9,
        Colon = 10,
        Equals = 11,
        At = 12,
        LeftSquareBracket = 13,
        RightSquareBracket = 14,
        LeftCurlyBracket = 15,
        RightCurlyBracket = 16,
        Vertical = 17,
        Dot = 18,
        Underscore = 19,
        Letter = 20,
        Digit = 21,
        Minus = 22,
        DoubleQuote = 23,
        Plus = 24,
        Hexadecimal = 25,
        StartOfText = 26,
        EndOfText = 27,
        Spread = 28,
        Name = 29,
        IntValue = 30,
        FloatValue = 31,
        StringValue = 32
    }

    private enum EscapeKind : byte
    {
        Other = 0,
        DoubleQuote = 1,
        Backslash = 2,
        Slash = 3,
        Backspace = 4,
        Feed = 5,
        NewLine = 6,
        CarriageReturn = 7,
        Tab = 8,
        u = 9
    }
}
