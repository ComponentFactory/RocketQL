using DotNetQL.Parser;

namespace DotNetQL.UnitTests
{
    public class TokenizerTests
    {
        [Theory]
        [InlineData("\uFEFF")]
        [InlineData("\uFEFF﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿\uFEFF\uFEFF")]
        public void ByteOrderMark(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.EndOfText, t.Token);
            Assert.Equal(1, t.LineNumber);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\t\t\t")]
        [InlineData("\t  \t\t  ")]
        public void Whitespace(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.EndOfText, t.Token);
            Assert.Equal(1, t.LineNumber);
        }

        [Theory]
        [InlineData("\n", 2)]
        [InlineData("\n\n\n", 4)]
        [InlineData("\r", 2)]
        [InlineData("\r\r\r", 4)]
        [InlineData("\r\n", 2)]
        [InlineData("\r\n\r\n\r\n", 4)]
        public void LineTerminators(string text, int lineNumber)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.EndOfText, t.Token);
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
            var t = new Tokenizer(text);
            Assert.Equal(Token.EndOfText, t.Token);
        }

        [Theory]
        [InlineData(",")]
        [InlineData(",,,")]
        public void Comma(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.EndOfText, t.Token);
            Assert.Equal(1, t.LineNumber);
        }

        [Theory]
        [InlineData("﻿\uFEFF \t \r\n,")]
        [InlineData("﻿\uFEFF \t \r\n,\uFEFF﻿ \t \r\n,")]
        [InlineData("﻿ ,,, \t\r\t\t\n ,\t,\t,\r\n\t,,,")]
        [InlineData("﻿ ,,, \t\r\t\t#你好Ȳ \n ,\t,\t,\r\n\t,,,")]
        public void Ignored(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.EndOfText, t.Token);
        }

        [Theory]
        [InlineData("!", Token.Exclamation)]
        [InlineData("$", Token.Dollar)]
        [InlineData("&", Token.Ampersand)]
        [InlineData("(", Token.LeftParenthesis)]
        [InlineData(")", Token.RightParenthesis)]
        [InlineData(":", Token.Colon)]
        [InlineData("=", Token.Equals)]
        [InlineData("@", Token.At)]
        [InlineData("[", Token.LeftSquareBracket)]
        [InlineData("]", Token.RightSquareBracket)]
        [InlineData("{", Token.LeftCurlyBracket)]
        [InlineData("}", Token.RightCurlyBracket)]
        [InlineData("|", Token.Vertical)]
        [InlineData("...", Token.Spread)]
        public void Punctuators(string text, Token token)
        {
            var t = new Tokenizer(text);
            Assert.Equal(token, t.Token);
            t.Next();
            Assert.Equal(Token.EndOfText, t.Token);
            Assert.Equal(1, t.LineNumber);
            Assert.Equal(1, t.ColumnNumber);
        }

        [Theory]
        [InlineData("!$&", Token.Exclamation, Token.Dollar, Token.Ampersand)]
        [InlineData("! $ &", Token.Exclamation, Token.Dollar, Token.Ampersand)]
        [InlineData("\uFEFF\t!,...   &\r\n", Token.Exclamation, Token.Spread, Token.Ampersand)]
        [InlineData("...... \t,\t...", Token.Spread, Token.Spread, Token.Spread)]
        public void PunctuatorsAndWhitespace(string text, Token token1, Token token2, Token token3)
        {
            var t = new Tokenizer(text);
            Assert.Equal(token1, t.Token);
            t.Next();
            Assert.Equal(token2, t.Token);
            t.Next();
            Assert.Equal(token3, t.Token);
            t.Next();
            Assert.Equal(Token.EndOfText, t.Token);
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
            var t = new Tokenizer(text);
            Assert.Equal(Token.Name, t.Token);
            t.Next();
            Assert.Equal(Token.EndOfText, t.Token);
            Assert.Equal(1, t.ColumnNumber);
        }

        [Theory]
        [InlineData("World#Hello")]
        [InlineData("# Hello 你好Ȳ\nWorld\t# fish")]
        [InlineData("#Fish#你好Ȳ\rWorld")]
        [InlineData("#Fish#你好Ȳ\rWorld\n")]
        public void NameAndWhitespace(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.Name, t.Token);
            t.Next();
            Assert.Equal(Token.EndOfText, t.Token);
        }

        [Fact]
        public void ColumnNumber()
        {
            var t = new Tokenizer("aA $ , # Ȳ");
            Assert.Equal(Token.Name, t.Token);
            Assert.Equal(1, t.ColumnNumber);
            t.Next();
            Assert.Equal(Token.Dollar, t.Token);
            Assert.Equal(4, t.ColumnNumber);
            t.Next();
            Assert.Equal(Token.EndOfText, t.Token);
            Assert.Equal(11, t.ColumnNumber);
        }
    }
}