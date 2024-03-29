﻿namespace RocketQL.Core.UnitTests.JsonTokenizerTests;

public class Individual : UnitTestBase
{
    [Fact]
    public void NullText()
    {
        var t = new JsonTokenizer(null!);
        Assert.Equal(JsonTokenKind.EndOfText, t.TokenKind);
        Assert.Equal(1, t.LineNumber);
        Assert.Equal(1, t.ColumnNumber);

    }

    [Theory]
    [InlineData("\uFEFF")]
    [InlineData("﻿\uFEFF\uFEFF\uFEFF")]
    public void ByteOrderMark(string text)
    {
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(JsonTokenKind.EndOfText, t.TokenKind);
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
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(JsonTokenKind.EndOfText, t.TokenKind);
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
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(JsonTokenKind.EndOfText, t.TokenKind);
        Assert.Equal(lineNumber, t.LineNumber);
    }

    [Theory]
    [InlineData("﻿\uFEFF \t \r\n")]
    [InlineData("﻿\uFEFF \t \r\n\uFEFF﻿ \t \r\n")]
    public void Ignored(string text)
    {
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(JsonTokenKind.EndOfText, t.TokenKind);
    }

    [Theory]
    [InlineData(":", JsonTokenKind.Colon)]
    [InlineData("[", JsonTokenKind.LeftSquareBracket)]
    [InlineData("]", JsonTokenKind.RightSquareBracket)]
    [InlineData("{", JsonTokenKind.LeftCurlyBracket)]
    [InlineData("}", JsonTokenKind.RightCurlyBracket)]
    [InlineData(",", JsonTokenKind.Comma)]
    public void Punctuator(string text, JsonTokenKind token)
    {
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(token, t.TokenKind);
        Assert.Equal(text, t.TokenValue);
        t.Next();
        Assert.Equal(JsonTokenKind.EndOfText, t.TokenKind);
        Assert.Equal(1, t.LineNumber);
        Assert.Equal(text.Length + 1, t.ColumnNumber);
    }

    [Theory]
    [InlineData("true", JsonTokenKind.TrueValue)]
    [InlineData("false", JsonTokenKind.FalseValue)]
    [InlineData("null", JsonTokenKind.NullValue)]
    public void Keywords(string text, JsonTokenKind token)
    {
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(token, t.TokenKind);
        Assert.Equal(text, t.TokenValue);
        t.Next();
        Assert.Equal(JsonTokenKind.EndOfText, t.TokenKind);
        Assert.Equal(1, t.LineNumber);
        Assert.Equal(text.Length + 1, t.ColumnNumber);
    }

    [Theory]
    [InlineData("0", 0, JsonTokenKind.EndOfText)]
    [InlineData("9", 9, JsonTokenKind.EndOfText)]
    [InlineData("12345", 12345, JsonTokenKind.EndOfText)]
    [InlineData("9876543", 9876543, JsonTokenKind.EndOfText)]
    [InlineData("-0", -0, JsonTokenKind.EndOfText)]
    [InlineData("-9", -9, JsonTokenKind.EndOfText)]
    [InlineData("-12345", -12345, JsonTokenKind.EndOfText)]
    [InlineData("-9876543", -9876543, JsonTokenKind.EndOfText)]
    [InlineData("0 ", 0, JsonTokenKind.EndOfText)]
    [InlineData("9\t", 9, JsonTokenKind.EndOfText)]
    [InlineData("12345\r\n", 12345, JsonTokenKind.EndOfText)]
    [InlineData("9876543,", 9876543, JsonTokenKind.Comma)]
    [InlineData("-0{", -0, JsonTokenKind.LeftCurlyBracket)]
    [InlineData("-0 {", -0, JsonTokenKind.LeftCurlyBracket)]
    [InlineData("-9876543\uFEFF", -9876543, JsonTokenKind.EndOfText)]
    public void IntValue(string text, int val, JsonTokenKind nextToken)
    {
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(JsonTokenKind.IntValue, t.TokenKind);
        Assert.Equal(val, int.Parse(t.TokenValue));
        t.Next();
        Assert.Equal(nextToken, t.TokenKind);
    }

    [Theory]
    [InlineData("0.0", 0.0, JsonTokenKind.EndOfText)]
    [InlineData("9.1", 9.1, JsonTokenKind.EndOfText)]
    [InlineData("12345.0123", 12345.0123, JsonTokenKind.EndOfText)]
    [InlineData("9876543.987", 9876543.987, JsonTokenKind.EndOfText)]
    [InlineData("-0.0", -0.0, JsonTokenKind.EndOfText)]
    [InlineData("-9.1", -9.1, JsonTokenKind.EndOfText)]
    [InlineData("-12345.0123", -12345.0123, JsonTokenKind.EndOfText)]
    [InlineData("-9876543.987", -9876543.987, JsonTokenKind.EndOfText)]
    [InlineData("1e0", 1e0, JsonTokenKind.EndOfText)]
    [InlineData("2e123", 2e123, JsonTokenKind.EndOfText)]
    [InlineData("1e+2", 1e+2, JsonTokenKind.EndOfText)]
    [InlineData("2e+33", 2e+33, JsonTokenKind.EndOfText)]
    [InlineData("1e-2", 1e-2, JsonTokenKind.EndOfText)]
    [InlineData("2e-33", 2e-33, JsonTokenKind.EndOfText)]
    [InlineData("1.1e0", 1.1e0, JsonTokenKind.EndOfText)]
    [InlineData("2.2e123", 2.2e123, JsonTokenKind.EndOfText)]
    [InlineData("1.1e+2", 1.1e+2, JsonTokenKind.EndOfText)]
    [InlineData("2.2e+33", 2.2e+33, JsonTokenKind.EndOfText)]
    [InlineData("1.1e-2", 1.1e-2, JsonTokenKind.EndOfText)]
    [InlineData("2.2e-33", 2.2e-33, JsonTokenKind.EndOfText)]
    [InlineData("0.0 ", 0.0, JsonTokenKind.EndOfText)]
    [InlineData("12345.0123\r\n", 12345.0123, JsonTokenKind.EndOfText)]
    [InlineData("9876543.987{", 9876543.987, JsonTokenKind.LeftCurlyBracket)]
    [InlineData("-0.0﻿\uFEFF", -0.0, JsonTokenKind.EndOfText)]
    public void FloatValue(string text, double val, JsonTokenKind nextToken)
    {
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(JsonTokenKind.FloatValue, t.TokenKind);
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
    public void StringValue(string text, string contents)
    {
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        t.Next();
        Assert.Equal(JsonTokenKind.StringValue, t.TokenKind);
        Assert.Equal(contents, t.TokenString); t.Next();
        t.Next();
        Assert.Equal(JsonTokenKind.EndOfText, t.TokenKind);
    }

    [Theory]
    [InlineData("\u0001", '\u0001', 1, 1, 1)]
    [InlineData("   \u0001", '\u0001', 1, 4, 4)]
    [InlineData("\n\n   \u0001", '\u0001', 3, 4, 6)]
    public void IllegalCharacterCode(string text, char code, int line, int column, int position)
    {
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Illegal character code '{(int)code}' for this location.", ex.Message);
            Assert.Equal(line, ex.Location.Line);
            Assert.Equal(column, ex.Location.Column);
            Assert.Equal(position, ex.Location.Position);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Theory]
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
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Unexpected end of file encountered.", ex.Message);
            Assert.Equal(line, ex.Location.Line);
            Assert.Equal(column, ex.Location.Column);
            Assert.Equal(position, ex.Location.Position);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Theory]
    [InlineData("tru", 1, 1, 3)]
    [InlineData("True", 1, 1, 4)]
    [InlineData("TRUE", 1, 1, 4)]
    [InlineData("fals", 1, 1, 4)]
    [InlineData("False", 1, 1, 5)]
    [InlineData("FALSE", 1, 1, 5)]
    [InlineData("nul", 1, 1, 3)]
    [InlineData("Null", 1, 1, 4)]
    [InlineData("NULL", 1, 1, 4)]
    public void UnexpectedKeyword(string text, int line, int column, int position)
    {
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Unrecognized keyword '{text}'.", ex.Message);
            Assert.Equal(line, ex.Location.Line);
            Assert.Equal(column, ex.Location.Column);
            Assert.Equal(position, ex.Location.Position);
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
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Minus sign must be followed by a digit.", ex.Message);
            Assert.Equal(line, ex.Location.Line);
            Assert.Equal(column, ex.Location.Column);
            Assert.Equal(position, ex.Location.Position);
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
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Decimal point must be followed by a digit.", ex.Message);
            Assert.Equal(line, ex.Location.Line);
            Assert.Equal(column, ex.Location.Column);
            Assert.Equal(position, ex.Location.Position);
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
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Exponent must have at least one digit.", ex.Message);
            Assert.Equal(line, ex.Location.Line);
            Assert.Equal(column, ex.Location.Column);
            Assert.Equal(position, ex.Location.Position);
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
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Floating point value cannot be followed by a {param}.", ex.Message);
            Assert.Equal(line, ex.Location.Line);
            Assert.Equal(column, ex.Location.Column);
            Assert.Equal(position, ex.Location.Position);
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
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Integer value cannot be followed by a {param}.", ex.Message);
            Assert.Equal(line, ex.Location.Line);
            Assert.Equal(column, ex.Location.Column);
            Assert.Equal(position, ex.Location.Position);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Theory]
    [InlineData("\"\\u.111", 1, 2, 7)]
    [InlineData("\"\\u1.11", 1, 2, 7)]
    [InlineData("\"\\u11.1", 1, 2, 7)]
    [InlineData("\"\\u111.", 1, 2, 7)]
    public void EscapeOnlyUsingHex(string text, int line, int column, int position)
    {
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Escaped character must be specificed only using hexadecimal values.", ex.Message);
            Assert.Equal(line, ex.Location.Line);
            Assert.Equal(column, ex.Location.Column);
            Assert.Equal(position, ex.Location.Position);
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
        var t = new JsonTokenizer(text);
        Assert.Equal(JsonTokenKind.StartOfText, t.TokenKind);
        try
        {
            t.Next();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Escaped character is not one of \\\" \\\\ \\/ \\b \\f \\n \\r \\t.", ex.Message);
            Assert.Equal(line, ex.Location.Line);
            Assert.Equal(column, ex.Location.Column);
            Assert.Equal(position, ex.Location.Position);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

