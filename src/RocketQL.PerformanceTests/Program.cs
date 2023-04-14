using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Text;
using GQLParser = GraphQLParser;
using GQLJson = GraphQL.SystemTextJson;
using HC = HotChocolate.Language;
using RQL = RocketQL.Core;
using System.Text.Json;
using GraphQL;
using GraphQL.SystemTextJson;
using HotChocolate.Language;
using GraphQL.Types;
using static BenchmarkDotNet.Attributes.MarkdownExporterAttribute;

namespace DotNetQL.PerformanceTests
{
    internal class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<DeserializerBenchmark>();
            BenchmarkRunner.Run<TokenizerBenchmark>();
            BenchmarkRunner.Run<ParserBenchmark>();
        }
    }

    [MemoryDiagnoser]
    public class DeserializerBenchmark
    {
        public string _input =
        """
        {
            "int": 123,
            "float": 3.14,
            "string": "hello",
            "listints": [1, 2, 3],
            "object": {
                "int": 123,
                "float": 3.14,
                "string": "hello",
                "listints": [1, 2, 3],
                "child": {
                    "int": 123,
                    "float": 3.14,
                    "string": "hello",
                    "listints": [1, 2, 3],
                    "object": {
                        "int": 123,
                        "float": 3.14,
                        "string": "hello",
                        "listints": [1, 2, 3]
                    }
                }
            },
            "listobj": [{
                "int": 123,
                "float": 3.14,
                "string": "hello",
                "listints": [1, 2, 3],
                "object": {
                    "int": 123,
                    "float": 3.14,
                    "string": "hello",
                    "listints": [1, 2, 3],
                    "child": {
                        "int": 123,
                        "float": 3.14,
                        "string": "hello",
                        "listints": [1, 2, 3],
                        "object": {
                            "int": 123,
                            "float": 3.14,
                            "string": "hello",
                            "listints": [1, 2, 3]
                        }
                    }
                }
            },{
                "int": 123,
                "float": 3.14,
                "string": "hello",
                "listints": [1, 2, 3],
                "object": {
                    "int": 123,
                    "float": 3.14,
                    "string": "hello",
                    "listints": [1, 2, 3],
                    "child": {
                        "int": 123,
                        "float": 3.14,
                        "string": "hello",
                        "listints": [1, 2, 3],
                        "object": {
                            "int": 123,
                            "float": 3.14,
                            "string": "hello",
                            "listints": [1, 2, 3]
                        }
                    }
                }
            },{
                "int": 123,
                "float": 3.14,
                "string": "hello",
                "listints": [1, 2, 3],
                "object": {
                    "int": 123,
                    "float": 3.14,
                    "string": "hello",
                    "listints": [1, 2, 3],
                    "child": {
                        "int": 123,
                        "float": 3.14,
                        "string": "hello",
                        "listints": [1, 2, 3],
                        "object": {
                            "int": 123,
                            "float": 3.14,
                            "string": "hello",
                            "listints": [1, 2, 3]
                        }
                    }
                }
            }]
        }
        """;

        public readonly JsonSerializerOptions _graphQLOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters =
            {
                new InputsJsonConverter(),
                new JsonConverterBigInteger(),
            }
        };

        [GlobalSetup]
        public void Setup()
        {
        }

        [Benchmark]
        public void GraphQL_Small_Deserial()
        {
            var inputs = JsonSerializer.Deserialize<Inputs>(_input, _graphQLOptions);
        }

        [Benchmark]
        public void HotChocolate_Small_Deserial()
        {
            var reader = HC.Utf8GraphQLRequestParser.ParseJsonObject(_input);
        }        

        [Benchmark]
        public void RocketQL_Small_Deserial()
        {
            var valueNode = new RQL.JsonParser(_input).Parse();
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
        public void _GraphQL_GitHub_Token()
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
            GQLParser.Token token;
            while ((token = GQLParser.Lexer.Lex(schema, resetPosition)).Kind != GraphQLParser.TokenKind.EOF)
            {
                resetPosition = token.End;
                switch (token.Kind)
                {
                    case GQLParser.TokenKind.STRING:
                    case GQLParser.TokenKind.INT:
                    case GQLParser.TokenKind.FLOAT:
                    case GQLParser.TokenKind.NAME:
                    case GQLParser.TokenKind.COMMENT:
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
            var t = new RQL.DocumentTokenizer(schema);
            while (t.Next())
            {
                switch (t.TokenKind)
                {
                    case RQL.DocumentTokenKind.StringValue:
                        s = t.TokenString;
                        break;
                    case RQL.DocumentTokenKind.IntValue:
                    case RQL.DocumentTokenKind.FloatValue:
                    case RQL.DocumentTokenKind.Name:
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
            new RQL.TypeSystemParser(_github).Parse();
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
            new RQL.ExecutableParser(_introspection).Parse();
        }
    }
}