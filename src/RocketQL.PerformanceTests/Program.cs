using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Y = RocketQL.Core;
using GraphQLParser;
using X = HotChocolate.Language;
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
        public void GraphQL()
        {
            int resetPosition = 0;
            GraphQLParser.Token token;
            while ((token = Lexer.Lex(_graphQL, resetPosition)).Kind != GraphQLParser.TokenKind.EOF)
            {
                resetPosition = token.End;
            }
        }

        [Benchmark]
        public void HotChocolate()
        {
            var s = string.Empty;
            var reader = new X.Utf8GraphQLReader(_graphQLBytes);
            while (reader.Read())
            {
                switch (reader.Kind)
                {
                    case X.TokenKind.String:
                    case X.TokenKind.BlockString:
                        s = reader.GetString();
                        break;
                    case X.TokenKind.Name:
                        s = reader.GetName();
                        break;
                }
            }
        }

        [Benchmark]
        public void RocketQL()
        {
            var s = string.Empty;
            var t = new Y.Tokenizer(_graphQL.AsSpan());
            while (t.Next())
            {
                switch(t.Token)
                {
                    case Y.TokenKind.StringValue:
                        s = t.TokenString;
                        break;
                    case Y.TokenKind.Name:
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
            X.Utf8GraphQLParser.Parse(_graphQL);
        }
    }
}