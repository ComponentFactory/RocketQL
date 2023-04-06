using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Text;
using GQL = GraphQLParser;
using HC = HotChocolate.Language;
using RQL = RocketQL.Core;


namespace DotNetQL.PerformanceTests
{
    internal class Program
    {
        static void Main()
        {
            //var graphQL = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "github.graphql"));

            // var s = string.Empty;

            // for(int i= 1; i <10000; i++)
            // {
            //     var t = new Y.Tokenizer(graphQL);
            //     while (t.Next())
            //     {
            //         switch (t.Token)
            //         {
            //             case Y.TokenKind.StringValue:
            //                 s = t.TokenString;
            //                 break;
            //             case Y.TokenKind.Name:
            //                 s = t.TokenValue;
            //                 break;
            //         }
            //     }
            // }

            // BenchmarkRunner.Run<TokenizerBenchmark>();
            // BenchmarkRunner.Run<ParserBenchmark>();
            BenchmarkRunner.Run<TempBenchmark>();
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
            var s = string.Empty;
            int resetPosition = 0;
            GQL.Token token;
            while ((token = GQL.Lexer.Lex(_graphQL, resetPosition)).Kind != GraphQLParser.TokenKind.EOF)
            {
                resetPosition = token.End;
                switch (token.Kind)
                {
                    case GQL.TokenKind.STRING:
                    case GQL.TokenKind.INT:
                    case GQL.TokenKind.FLOAT:
                    case GQL.TokenKind.NAME:
                    case GQL.TokenKind.COMMENT:
                        s = token.Value.ToString();
                        break;
                }
            }
        }

        [Benchmark]
        public void HotChocolate()
        {
            var s = string.Empty;
            var reader = new HC.Utf8GraphQLReader(_graphQLBytes);
            while (reader.Read())
            {
                switch (reader.Kind)
                {
                    case HC.TokenKind.String:
                    case HC.TokenKind.BlockString:
                        s = reader.GetString();
                        break;
                    case HC.TokenKind.Name:
                        s = reader.GetName();
                        break;
                    case HC.TokenKind.Integer:
                    case HC.TokenKind.Float:
                        s = reader.GetScalarValue();
                        break;
                    case HC.TokenKind.Comment:
                        s = reader.GetComment();
                        break;

                }
            }
        }

        [Benchmark]
        public void RocketQL()
        {
            var s = string.Empty;
            var t = new RQL.Tokenizer(_graphQL);
            while (t.Next())
            {
                switch (t.TokenKind)
                {
                    case RQL.TokenKind.StringValue:
                        s = t.TokenString;
                        break;
                    case RQL.TokenKind.IntValue:
                    case RQL.TokenKind.FloatValue:
                    case RQL.TokenKind.Name:
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
            HC.Utf8GraphQLParser.Parse(_graphQL);
        }
    }

    [MemoryDiagnoser]
    public class TempBenchmark
    {
        public string _directives = string.Empty;

        [GlobalSetup]
        public void Setup()
        {
            _directives = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "directives.graphql"));
        }

        [Benchmark]
        public void ParserGraphQL()
        {
            GraphQLParser.Parser.Parse(_directives);
        }

        [Benchmark]
        public void ParserHC()
        {
            HC.Utf8GraphQLParser.Parse(_directives);
        }

        [Benchmark]
        public void RocketQL()
        {
            new RQL.Parser(_directives.AsSpan()).Parse();
        }
    }
}