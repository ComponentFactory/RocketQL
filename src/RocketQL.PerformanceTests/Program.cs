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
            BenchmarkRunner.Run<TokenizerBenchmark>();
            BenchmarkRunner.Run<ParserBenchmark>();
        }
    }

    [MemoryDiagnoser]
    public class TokenizerBenchmark
    {
        public string _github = string.Empty;
        public byte[] _githubBytes = Array.Empty<byte>();
        public string _introspection = string.Empty;
        public byte[] _introspectionBytes = Array.Empty<byte>();

        [GlobalSetup]
        public void Setup()
        {
            _github = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "github.graphql"));
            _githubBytes = Encoding.ASCII.GetBytes(_github);

            _introspection = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "introspection.graphql"));
            _introspectionBytes = Encoding.ASCII.GetBytes(_introspection);
        }

        [Benchmark]
        public void Token_GraphQL_GitHub_Token()
        {
            GraphQL(_github);
        }

        [Benchmark]
        public void HotChocolate_GitHub_Token()
        {
            HotChocolate(_githubBytes);
        }

        [Benchmark]
        public void RocketkQL_GitHub_Token()
        {
            RocketQL(_github);
        }

        [Benchmark]
        public void GraphQL_Intro_Token()
        {
            GraphQL(_introspection);
        }

        [Benchmark]
        public void HotChocolate_Intro_Token()
        {
            HotChocolate(_introspectionBytes);
        }

        [Benchmark]
        public void RocketQL_Intro_Token()
        {
            RocketQL(_introspection);
        }

        private void GraphQL(string schema)
        {
            var s = string.Empty;
            int resetPosition = 0;
            GQL.Token token;
            while ((token = GQL.Lexer.Lex(schema, resetPosition)).Kind != GraphQLParser.TokenKind.EOF)
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

        private void HotChocolate(byte[] schemaBytes)
        {
            var s = string.Empty;
            var reader = new HC.Utf8GraphQLReader(schemaBytes);
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

        private void RocketQL(string schema)
        {
            var s = string.Empty;
            var t = new RQL.Tokenizer(schema);
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
        public string _github = string.Empty;
        public string _introspection = string.Empty;

        [GlobalSetup]
        public void Setup()
        {
            _github = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "github.graphql"));
            _introspection = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "introspection.graphql"));
        }

        [Benchmark]
        public void GraphQL_GitHub_Parse()
        {
            GraphQLParser.Parser.Parse(_github);
        }

        [Benchmark]
        public void HotChocolate_GitHub_Parse()
        {
            HC.Utf8GraphQLParser.Parse(_github);
        }

        [Benchmark]
        public void RocketQL_GitHub_Parse()
        {
            new RQL.Parser(_github).Parse();
        }

        [Benchmark]
        public void GraphQL_Intro_Parse()
        {
            GraphQLParser.Parser.Parse(_introspection);
        }

        [Benchmark]
        public void HotChocolate_Intro_Parse()
        {
            HC.Utf8GraphQLParser.Parse(_introspection);
        }

        [Benchmark]
        public void RocketQL_Intro_Parse()
        {
            new RQL.Parser(_introspection).Parse();
        }
    }
}