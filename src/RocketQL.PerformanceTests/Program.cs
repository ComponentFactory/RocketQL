using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using RocketQL.Core;
using GraphQLParser;
using HotChocolate.Language;
using System.Text;

namespace DotNetQL.PerformanceTests
{
    internal class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<TokenizerBenchmark>();
            //BenchmarkRunner.Run<ParserBenchmark>();
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
            var s = string.Empty;
            var reader = new Utf8GraphQLReader(_graphQLBytes);
            while (reader.Read())
            {
                switch (reader.Kind)
                {
                    case HotChocolate.Language.TokenKind.String:
                    case HotChocolate.Language.TokenKind.BlockString:
                        s = reader.GetString();
                        break;
                    case HotChocolate.Language.TokenKind.Name:
                        s = reader.GetName();
                        break;
                }
            }
        }

        [Benchmark]
        public void Tokenizer()
        {
            var s = string.Empty;
            var t = new Tokenizer(_graphQL.AsSpan());
            while (t.Next())
            {
                switch(t.Token)
                {
                    case RocketQL.Core.TokenKind.StringValue:
                        s = t.TokenString;
                        break;
                    case RocketQL.Core.TokenKind.Name:
                        s = t.TokenValue;
                        break;
                }
            }
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