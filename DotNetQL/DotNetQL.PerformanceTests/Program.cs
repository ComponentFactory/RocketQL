using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DotNetQL.Parser;
using GraphQLParser;
using HotChocolate.Language;
using Microsoft.Extensions.Options;
using System.Reflection.PortableExecutable;
using System.Text;

namespace DotNetQL.PerformanceTests
{
    internal class Program
    {
        static void Main()
        {
            //BenchmarkRunner.Run<MicroBenchmark>();
            BenchmarkRunner.Run<TokenizerBenchmark>();
            //BenchmarkRunner.Run<ParserBenchmark>();
        }
    }

    [MemoryDiagnoser]
    public class MicroBenchmark
    {
        public string _graphQL = string.Empty;

        [GlobalSetup]
        public void Setup()
        {
            _graphQL = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "github.graphql"));
        }

        [Benchmark]
        public void AccessViaString()
        {
            int len = _graphQL.Length;
            char? c;
            for (int i = 0; i < len; i++)
            {
                c = _graphQL[i];
            }
        }

        [Benchmark]
        public void AccessViaSpan()
        {
            ReadOnlySpan<char> span = _graphQL.AsSpan();
            int len = span.Length;
            char? c;
            for (int i = 0; i < len; i++)
            {
                c = span[i];
            }
        }

        [Benchmark]
        public void Tokenizer()
        {
            var t = new Tokenizer(_graphQL.AsSpan());
            while (t.Next() != Parser.Token.EndOfText);
        }
    }

    [MemoryDiagnoser]
    public class TokenizerBenchmark
    {
        public string _graphQL = string.Empty;
        public byte[] _graphQLBytes = Array.Empty<byte>();

        [GlobalSetup]
        public void Setup()
        {
            _graphQL = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "github.graphql"));
            _graphQLBytes = Encoding.ASCII.GetBytes(_graphQL);
        }

        [Benchmark]
        public void TokenizerGraphQL()
        {
            int resetPosition = 0;
            GraphQLParser.Token token;
            while ((token = Lexer.Lex(_graphQL, resetPosition)).Kind != GraphQLParser.TokenKind.EOF)
            {
                resetPosition = token.End;
            }
        }

        [Benchmark]
        public void TokenizerHC()
        {
            var reader = new Utf8GraphQLReader(_graphQLBytes);
            while (reader.Read());
        }

        [Benchmark]
        public void Tokenizer()
        {
            var t = new Tokenizer(_graphQL.AsSpan());
            while (t.Next() != Parser.Token.EndOfText) ;
        }
    }

    [MemoryDiagnoser]
    public class ParserBenchmark
    {
        public string _graphQL = string.Empty;

        [GlobalSetup]
        public void Setup()
        {
            _graphQL = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "github.graphql"));
        }

        [Benchmark]
        public void ParserGraphQL()
        {
            GraphQLParser.Parser.Parse(_graphQL);
        }

        [Benchmark]
        public void ParserHC()
        {
            Utf8GraphQLParser.Parse(_graphQL);
        }
    }
}