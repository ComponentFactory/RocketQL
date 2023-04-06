namespace RocketQL.Core.UnitTests.Tokenizer;

public class Individual
{
    [Fact]
    public void NullText()
    {
        var t = new Core.Tokenizer(null);
        Assert.Equal(TokenKind.EndOfText, t.TokenKind);
        Assert.Equal(1, t.LineNumber);
        Assert.Equal(1, t.ColumnNumber);

    }

    [Theory]
    [InlineData("\uFEFF")]
    [InlineData("﻿\uFEFF\uFEFF\uFEFF")]
    public void ByteOrderMark(string text)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(TokenKind.EndOfText, t.TokenKind);
        Assert.Equal(1, t.LineNumber);
        Assert.Equal(text.Length + 1, t.ColumnNumber);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\t\t\t")]
    [InlineData("\t  \t\t  ")]
    public void Whitespace(string text)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(TokenKind.EndOfText, t.TokenKind);
        Assert.Equal(1, t.LineNumber);
        Assert.Equal(text.Length + 1, t.ColumnNumber);
    }

    [Theory]
    [InlineData("\n", 2)]
    [InlineData("\n\n\n", 4)]
    [InlineData("\r", 2)]
    [InlineData("\r\r\r", 4)]
    [InlineData("\r\n", 2)]
    [InlineData("\r\n\r\n\r\n", 4)]
    public void LineTerminator(string text, int lineNumber)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(TokenKind.EndOfText, t.TokenKind);
        Assert.Equal(lineNumber, t.LineNumber);
    }

    [Theory]
    [InlineData("#")]
    [InlineData("# ")]
    [InlineData("#\r")]
    [InlineData("# \n")]
    [InlineData("#你好Ȳ\r\n")]
    [InlineData("###")]
    [InlineData("#\n#\n#")]
    [InlineData("#\n#\n\r# ")]
    public void Comment(string text)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(TokenKind.EndOfText, t.TokenKind);
    }

    [Theory]
    [InlineData(",")]
    [InlineData(",,,")]
    public void Comma(string text)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(TokenKind.EndOfText, t.TokenKind);
        Assert.Equal(1, t.LineNumber);
        Assert.Equal(text.Length + 1, t.ColumnNumber);
    }

    [Theory]
    [InlineData("﻿\uFEFF \t \r\n,")]
    [InlineData("﻿\uFEFF \t \r\n,\uFEFF﻿ \t \r\n,")]
    [InlineData("﻿ ,,, \t\r\t\t\n ,\t,\t,\r\n\t,,,")]
    [InlineData("﻿ ,,, \t\r\t\t#你好Ȳ \n ,\t,\t,\r\n\t,,,")]
    public void Ignored(string text)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(TokenKind.EndOfText, t.TokenKind);
    }

    [Theory]
    [InlineData("!", TokenKind.Exclamation)]
    [InlineData("$", TokenKind.Dollar)]
    [InlineData("&", TokenKind.Ampersand)]
    [InlineData("(", TokenKind.LeftParenthesis)]
    [InlineData(")", TokenKind.RightParenthesis)]
    [InlineData(":", TokenKind.Colon)]
    [InlineData("=", TokenKind.Equals)]
    [InlineData("@", TokenKind.At)]
    [InlineData("[", TokenKind.LeftSquareBracket)]
    [InlineData("]", TokenKind.RightSquareBracket)]
    [InlineData("{", TokenKind.LeftCurlyBracket)]
    [InlineData("}", TokenKind.RightCurlyBracket)]
    [InlineData("|", TokenKind.Vertical)]
    [InlineData("...", TokenKind.Spread)]
    public void Punctuator(string text, TokenKind token)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(token, t.TokenKind);
        Assert.Equal(text, t.TokenValue);
        t.Next();
        Assert.Equal(TokenKind.EndOfText, t.TokenKind);
        Assert.Equal(1, t.LineNumber);
        Assert.Equal(text.Length + 1, t.ColumnNumber);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AA")]
    [InlineData("AAAA")]
    [InlineData("a")]
    [InlineData("aaaa")]
    [InlineData("aaaaaaaa")]
    [InlineData("aAzZ")]
    [InlineData("_")]
    [InlineData("_____")]
    [InlineData("_1A2b_3_d_eezZ")]
    public void Name(string text)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(TokenKind.Name, t.TokenKind);
        Assert.Equal(text, t.TokenValue);
        Assert.Equal(1, t.ColumnNumber);
        t.Next();
        Assert.Equal(TokenKind.EndOfText, t.TokenKind);
        Assert.Equal(text.Length + 1, t.ColumnNumber);
    }

    [Theory]
    [InlineData("0", 0, TokenKind.EndOfText)]
    [InlineData("9", 9, TokenKind.EndOfText)]
    [InlineData("12345", 12345, TokenKind.EndOfText)]
    [InlineData("9876543", 9876543, TokenKind.EndOfText)]
    [InlineData("-0", -0, TokenKind.EndOfText)]
    [InlineData("-9", -9, TokenKind.EndOfText)]
    [InlineData("-12345", -12345, TokenKind.EndOfText)]
    [InlineData("-9876543", -9876543, TokenKind.EndOfText)]
    [InlineData("0 ", 0, TokenKind.EndOfText)]
    [InlineData("0#", 0, TokenKind.EndOfText)]
    [InlineData("9\t", 9, TokenKind.EndOfText)]
    [InlineData("12345\r\n", 12345, TokenKind.EndOfText)]
    [InlineData("9876543,", 9876543, TokenKind.EndOfText)]
    [InlineData("-0{", -0, TokenKind.LeftCurlyBracket)]
    [InlineData("-0 {", -0, TokenKind.LeftCurlyBracket)]
    [InlineData("-9876543\uFEFF", -9876543, TokenKind.EndOfText)]
    public void IntValue(string text, int val, TokenKind nextToken)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(TokenKind.IntValue, t.TokenKind);
        Assert.Equal(val, int.Parse(t.TokenValue));
        t.Next();
        Assert.Equal(nextToken, t.TokenKind);
    }

    [Theory]
    [InlineData("0.0", 0.0, TokenKind.EndOfText)]
    [InlineData("9.1", 9.1, TokenKind.EndOfText)]
    [InlineData("12345.0123", 12345.0123, TokenKind.EndOfText)]
    [InlineData("9876543.987", 9876543.987, TokenKind.EndOfText)]
    [InlineData("-0.0", -0.0, TokenKind.EndOfText)]
    [InlineData("-9.1", -9.1, TokenKind.EndOfText)]
    [InlineData("-12345.0123", -12345.0123, TokenKind.EndOfText)]
    [InlineData("-9876543.987", -9876543.987, TokenKind.EndOfText)]
    [InlineData("1e0", 1e0, TokenKind.EndOfText)]
    [InlineData("2e123", 2e123, TokenKind.EndOfText)]
    [InlineData("1e+2", 1e+2, TokenKind.EndOfText)]
    [InlineData("2e+33", 2e+33, TokenKind.EndOfText)]
    [InlineData("1e-2", 1e-2, TokenKind.EndOfText)]
    [InlineData("2e-33", 2e-33, TokenKind.EndOfText)]
    [InlineData("1.1e0", 1.1e0, TokenKind.EndOfText)]
    [InlineData("2.2e123", 2.2e123, TokenKind.EndOfText)]
    [InlineData("1.1e+2", 1.1e+2, TokenKind.EndOfText)]
    [InlineData("2.2e+33", 2.2e+33, TokenKind.EndOfText)]
    [InlineData("1.1e-2", 1.1e-2, TokenKind.EndOfText)]
    [InlineData("2.2e-33", 2.2e-33, TokenKind.EndOfText)]
    [InlineData("0.0 ", 0.0, TokenKind.EndOfText)]
    [InlineData("9.1#", 9.1, TokenKind.EndOfText)]
    [InlineData("12345.0123\r\n", 12345.0123, TokenKind.EndOfText)]
    [InlineData("9876543.987{", 9876543.987, TokenKind.LeftCurlyBracket)]
    [InlineData("-0.0﻿\uFEFF", -0.0, TokenKind.EndOfText)]
    public void FloatValue(string text, double val, TokenKind nextToken)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(TokenKind.FloatValue, t.TokenKind);
        Assert.Equal(val, double.Parse(t.TokenValue));
        t.Next();
        Assert.Equal(nextToken, t.TokenKind);
    }

    [Theory]
    [InlineData("\"\"", "")]
    [InlineData("\"a\"", "a")]
    [InlineData("\"abc\"", "abc")]
    [InlineData("\"你好Ȳ\"", "你好Ȳ")]
    [InlineData("\"\\\\\"", "\\")]
    [InlineData("\"\\/\"", "/")]
    [InlineData("\"\\b\"", "\b")]
    [InlineData("\"\\f\"", "\f")]
    [InlineData("\"\\n\"", "\n")]
    [InlineData("\"\\r\"", "\r")]
    [InlineData("\"\\t\"", "\t")]
    [InlineData("\"\\\"\"", "\"")]
    [InlineData("\"\\t\\r\\n\\f\\b\\/\\\\\\\"\"", "\t\r\n\f\b/\\\"")]
    [InlineData("\"\\thello world\\t\"", "\thello world\t")]
    [InlineData("\"\\u0041\"", "A")]
    [InlineData("\"Z\\u0041\"", "ZA")]
    [InlineData("\"Z\\u00411\"", "ZA1")]
    [InlineData("\"\\u0041\\u0042\"", "AB")]
    [InlineData("\"\\u00411\\u0042\"", "A1B")]
    [InlineData("\"\\u{41}\"", "A")]
    [InlineData("\"\\u{41}\\u{42}\\u{43}\"", "ABC")]
    [InlineData("\"\\u{41}你好Ȳ\\u0041\t\\u{42}\\u{43}\"", "A你好ȲA\tBC")]
    public void SimpleString(string text, string contents)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(TokenKind.StringValue, t.TokenKind);
        Assert.Equal(contents, t.TokenString); t.Next();
        t.Next();
        Assert.Equal(TokenKind.EndOfText, t.TokenKind);
    }

    [Theory]
    [InlineData("\"\"\"\"\"\"", "")]
    [InlineData("\"\"\"a\"\"\"", "a")]
    [InlineData("\"\"\"\na\"\"\"", "a")]
    [InlineData("\"\"\"   \na\"\"\"", "a")]
    [InlineData("\"\"\"   \n  \n\na\"\"\"", "a")]
    [InlineData("\"\"\"abc\"\"\"", "abc")]
    [InlineData("\"\"\"\nabc\"\"\"", "abc")]
    [InlineData("\"\"\"   \nabc\"\"\"", "abc")]
    [InlineData("\"\"\"   \n  \n\nabc\"\"\"", "abc")]
    [InlineData("\"\"\"a\n\"\"\"", "a")]
    [InlineData("\"\"\"a\n  \"\"\"", "a")]
    [InlineData("\"\"\"a   \n\"\"\"", "a   ")]
    [InlineData("\"\"\"a   \n\n\n\"\"\"", "a   ")]
    [InlineData("\"\"\"a   \n  \n \n\"\"\"", "a   ")]
    [InlineData("\"\"\"abc\n\"\"\"", "abc")]
    [InlineData("\"\"\"abc\n  \"\"\"", "abc")]
    [InlineData("\"\"\"abc   \n\"\"\"", "abc   ")]
    [InlineData("\"\"\"abc   \n\n\n\"\"\"", "abc   ")]
    [InlineData("\"\"\"abc   \n  \n \n\"\"\"", "abc   ")]
    [InlineData("\"\"\"a\nbc\"\"\"", "a\nbc")]
    [InlineData("\"\"\"ab\nc\"\"\"", "ab\nc")]
    [InlineData("\"\"\"ab\ncd\nef\"\"\"", "ab\ncd\nef")]
    [InlineData("\"\"\"ab \ncd  \nef   \"\"\"", "ab \ncd  \nef   ")]
    [InlineData("\"\"\"   a\"\"\"", "   a")]
    [InlineData("\"\"\"  a\n  b\n  c\"\"\"", "  a\nb\nc")]
    [InlineData("\"\"\"  a\n    b\n   c\"\"\"", "  a\n b\nc")]
    [InlineData("\"\"\"  a\n      \n  c\"\"\"", "  a\n    \nc")]
    [InlineData("\"\"\" a\n  b\n  c\"\"\"", " a\nb\nc")]
    [InlineData("\"\"\"    a\n  b\n    c\"\"\"", "    a\nb\n  c")]
    [InlineData("\"\"\"ab\r\ncd\r\nef\"\"\"", "ab\ncd\nef")]
    [InlineData("\"\"\"ab\rcd\ref\"\"\"", "ab\ncd\nef")]
    public void BlockString(string text, string contents)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(TokenKind.StringValue, t.TokenKind);
        Assert.Equal(contents, t.TokenString);
        t.Next();
        Assert.Equal(TokenKind.EndOfText, t.TokenKind);
    }

    [Theory]
    [InlineData("\u0001", '\u0001', 1, 1, 1)]
    [InlineData("   \u0001", '\u0001', 1, 4, 4)]
    [InlineData("\n\n   \u0001", '\u0001', 3, 4, 6)]
    public void IllegalCharacterCode(string text, char code, int line, int column, int position)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Illegal character code '{(int)code}' for this location.", ex.Message);
            Assert.Equal(line, ex.Locations[0].Line);
            Assert.Equal(column, ex.Locations[0].Column);
            Assert.Equal(position, ex.Locations[0].Position);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Theory]
    [InlineData(".", 1, 1, 1)]
    [InlineData("..", 1, 1, 2)]
    [InlineData("-", 1, 1, 1)]
    [InlineData("0.", 1, 1, 2)]
    [InlineData("-1.", 1, 1, 3)]
    [InlineData("2e", 1, 1, 2)]
    [InlineData("2e+", 1, 1, 3)]
    [InlineData("2.0e", 1, 1, 4)]
    [InlineData("2.0e+", 1, 1, 5)]
    [InlineData("\"\\u", 1, 2, 3)]
    [InlineData("\"\\u{", 1, 2, 4)]
    [InlineData("\"\\u1", 1, 2, 4)]
    [InlineData("\"\\u12", 1, 2, 5)]
    [InlineData("\"\\u123", 1, 2, 6)]
    public void UnexpectedEndOfFile(string text, int line, int column, int position)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Unexpected end of file encountered.", ex.Message);
            Assert.Equal(line, ex.Locations[0].Line);
            Assert.Equal(column, ex.Locations[0].Column);
            Assert.Equal(position, ex.Locations[0].Position);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Theory]
    [InlineData(".  ", 1, 1, 1)]
    [InlineData(".. ", 1, 1, 2)]
    [InlineData("  .. ", 1, 3, 4)]
    [InlineData(" \n .. ", 2, 2, 5)]
    [InlineData(" \n\n .. ", 3, 2, 6)]
    public void SpreadNeedsThreeDots(string text, int line, int column, int position)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Spread operator requires 3 dots in sequence.", ex.Message);
            Assert.Equal(line, ex.Locations[0].Line);
            Assert.Equal(column, ex.Locations[0].Column);
            Assert.Equal(position, ex.Locations[0].Position);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Theory]
    [InlineData("-A", 1, 1, 1)]
    [InlineData("-.", 1, 1, 1)]
    [InlineData("- ", 1, 1, 1)]
    public void MinusMustBeFollowedByDigit(string text, int line, int column, int position)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Minus sign must be followed by a digit.", ex.Message);
            Assert.Equal(line, ex.Locations[0].Line);
            Assert.Equal(column, ex.Locations[0].Column);
            Assert.Equal(position, ex.Locations[0].Position);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Theory]
    [InlineData("0.A", 1, 1, 2)]
    [InlineData("0.-", 1, 1, 2)]
    [InlineData("0. ", 1, 1, 2)]
    public void PointMustBeFollowedByDigit(string text, int line, int column, int position)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Decimal point must be followed by a digit.", ex.Message);
            Assert.Equal(line, ex.Locations[0].Line);
            Assert.Equal(column, ex.Locations[0].Column);
            Assert.Equal(position, ex.Locations[0].Position);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Theory]
    [InlineData("0e ", 1, 1, 2)]
    [InlineData("0e!", 1, 1, 2)]
    [InlineData("1.0e ", 1, 1, 4)]
    [InlineData("1.0e!", 1, 1, 4)]
    public void ExponentMustHaveDigit(string text, int line, int column, int position)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Exponent must have at least one digit.", ex.Message);
            Assert.Equal(line, ex.Locations[0].Line);
            Assert.Equal(column, ex.Locations[0].Column);
            Assert.Equal(position, ex.Locations[0].Position);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Theory]
    [InlineData("0.0_", 1, 1, 3, "underscore")]
    [InlineData("0.0e1_", 1, 1, 5, "underscore")]
    [InlineData("0.0a", 1, 1, 3, "letter")]
    [InlineData("0.0e1a", 1, 1, 5, "letter")]
    [InlineData("0.0.", 1, 1, 3, "dot")]
    [InlineData("0.0e1.", 1, 1, 5, "dot")]
    public void FloatCannotBeFollowed(string text, int line, int column, int position, string param)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Floating point value cannot be followed by a {param}.", ex.Message);
            Assert.Equal(line, ex.Locations[0].Line);
            Assert.Equal(column, ex.Locations[0].Column);
            Assert.Equal(position, ex.Locations[0].Position);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Theory]
    [InlineData("0_", 1, 1, 1, "underscore")]
    [InlineData("0a", 1, 1, 1, "letter")]
    public void IntCannotBeFollowed(string text, int line, int column, int position, string param)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Integer value cannot be followed by a {param}.", ex.Message);
            Assert.Equal(line, ex.Locations[0].Line);
            Assert.Equal(column, ex.Locations[0].Column);
            Assert.Equal(position, ex.Locations[0].Position);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Theory]
    [InlineData("\"\\u{}", 1, 2, 4)]
    [InlineData("\n \"\\u{}", 2, 3, 6)]
    public void EscapeAtLeast1Hex(string text, int line, int column, int position)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Escaped character must have at least 1 hexadecimal value.", ex.Message);
            Assert.Equal(line, ex.Locations[0].Line);
            Assert.Equal(column, ex.Locations[0].Column);
            Assert.Equal(position, ex.Locations[0].Position);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }


    }

    [Theory]
    [InlineData("\"\\u{z", 1, 2, 4)]
    [InlineData("\"\\u{@", 1, 2, 4)]
    [InlineData("\"\\u{ }", 1, 2, 4)]
    [InlineData("\"\\u.111", 1, 2, 7)]
    [InlineData("\"\\u1.11", 1, 2, 7)]
    [InlineData("\"\\u11.1", 1, 2, 7)]
    [InlineData("\"\\u111.", 1, 2, 7)]
    public void EscapeOnlyUsingHex(string text, int line, int column, int position)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Escaped character must be specificed only using hexadecimal values.", ex.Message);
            Assert.Equal(line, ex.Locations[0].Line);
            Assert.Equal(column, ex.Locations[0].Column);
            Assert.Equal(position, ex.Locations[0].Position);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Theory]
    [InlineData("\"\\u{123456789}", 1, 2, 13, "123456789")]
    [InlineData("\"\\u{ABCDEFABC}", 1, 2, 13, "ABCDEFABC")]
    [InlineData("\"\\u{abcdefabc}", 1, 2, 13, "abcdefabc")]
    public void EscapeCannotBeConverted(string text, int line, int column, int position, string param)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Cannot escape characters using hexidecimal value '{param}'.", ex.Message);
            Assert.Equal(line, ex.Locations[0].Line);
            Assert.Equal(column, ex.Locations[0].Column);
            Assert.Equal(position, ex.Locations[0].Position);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Theory]
    [InlineData("\"\\k", 1, 2, 2)]
    [InlineData("\"\\ ", 1, 2, 2)]
    [InlineData("\"\\_ ", 1, 2, 2)]
    public void EscapeMustBeOneOf(string text, int line, int column, int position)
    {
        var t = new Core.Tokenizer(text);
        Assert.Equal(TokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Escaped character is not one of \\\" \\\\ \\/ \\b \\f \\n \\r \\t.", ex.Message);
            Assert.Equal(line, ex.Locations[0].Line);
            Assert.Equal(column, ex.Locations[0].Column);
            Assert.Equal(position, ex.Locations[0].Position);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

