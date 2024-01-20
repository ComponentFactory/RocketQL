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
using Path = System.IO.Path;

namespace DotNetQL.PerformanceTests
{
    internal class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<DeserializerBenchmark>();
            BenchmarkRunner.Run<TokenizerBenchmark>();
            BenchmarkRunner.Run<ParserBenchmark>();
            BenchmarkRunner.Run<ValidateBenchmark>();
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

        public readonly JsonSerializerOptions GraphQLOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters =
            {
                new InputsJsonConverter(),
                new JsonConverterBigInteger(),
            }
        };

        [Benchmark]
        public void JsonSerializer_Small_Deserial()
        {
            JsonSerializer.Deserialize<Inputs>(_input, GraphQLOptions);
        }

        [Benchmark]
        public void HotChocolate___Small_Deserial()
        {
            HC.Utf8GraphQLRequestParser.ParseJsonObject(_input);
        }

        [Benchmark]
        public void RocketQL______Small_Deserial()
        {
            RQL.Serializers.Serialization.JsonDeserialize(_input);
        }
    }

    [MemoryDiagnoser]
    public class TokenizerBenchmark
    {
        public string _introspection = "";
        public byte[] _introspectionBytes = [];
        public string _github = "";
        public byte[] _githubBytes = [];
        public string _onegraph = "";
        public byte[] _onegraphBytes = [];

        [GlobalSetup]
        public void Setup()
        {
            _introspection = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "introspection.graphql"));
            _introspectionBytes = Encoding.ASCII.GetBytes(_introspection);

            _github = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "github.graphql"));
            _githubBytes = Encoding.ASCII.GetBytes(_github);

            _onegraph = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "onegraph.graphql"));
            _onegraphBytes = Encoding.ASCII.GetBytes(_onegraph);
        }

        [Benchmark]
        public void GraphQL______Intro____Token()
        {
            GraphQL(_introspection);
        }

        [Benchmark]
        public void HotChocolate_Intro____Token()
        {
            HotChocolate(_introspectionBytes);
        }

        [Benchmark]
        public void RocketQL_____Intro____Token()
        {
            RocketQL(_introspection);
        }

        [Benchmark]
        public void GraphQL______GitHub___Token()
        {
            GraphQL(_github);
        }

        [Benchmark]
        public void HotChocolate_GitHub___Token()
        {
            HotChocolate(_githubBytes);
        }

        [Benchmark]
        public void RocketQL_____GitHub___Token()
        {
            RocketQL(_github);
        }

        [Benchmark]
        public void GraphQL______Onegraph_Token()
        {
            GraphQL(_onegraph);
        }

        [Benchmark]
        public void HotChocolate_Onegraph_Token()
        {
            HotChocolate(_onegraphBytes);
        }

        [Benchmark]
        public void RocketQL_____Onegraph_Token()
        {
            RocketQL(_onegraph);
        }

        private static void GraphQL(string schema)
        {
            var s = "";
            var resetPosition = 0;
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

        private static void HotChocolate(byte[] schemaBytes)
        {
            var reader = new HC.Utf8GraphQLReader(schemaBytes);
            while (reader.Read())
            {
                switch (reader.Kind)
                {
                    case HC.TokenKind.String:
                    case HC.TokenKind.BlockString:
                        reader.GetString();
                        break;
                    case HC.TokenKind.Name:
                        reader.GetName();
                        break;
                    case HC.TokenKind.Integer:
                    case HC.TokenKind.Float:
                        reader.GetScalarValue();
                        break;
                    case HC.TokenKind.Comment:
                        reader.GetComment();
                        break;
                }
            }
        }

        private static void RocketQL(string schema)
        {
            var s = "";
            var t = new RQL.Tokenizers.DocumentTokenizer(schema);
            while (t.Next())
            {
                switch (t.TokenKind)
                {
                    case RQL.Tokenizers.DocumentTokenKind.StringValue:
                        s = t.TokenString;
                        break;
                    case RQL.Tokenizers.DocumentTokenKind.IntValue:
                    case RQL.Tokenizers.DocumentTokenKind.FloatValue:
                    case RQL.Tokenizers.DocumentTokenKind.Name:
                        s = t.TokenValue;
                        break;
                }
            }
        }
    }

    [MemoryDiagnoser]
    public class ParserBenchmark
    {
        public string _github = "";
        public string _introspection = "";
        public string _onegraph = "";

        [GlobalSetup]
        public void Setup()
        {
            _introspection = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "introspection.graphql"));
            _github = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "github.graphql"));
            _onegraph = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "onegraph.graphql"));
        }

        [Benchmark]
        public void GraphQL______Intro____Parse()
        {
            GraphQLParser.Parser.Parse(_introspection);
        }

        [Benchmark]
        public void HotChocolate_Intro____Parse()
        {
            HC.Utf8GraphQLParser.Parse(_introspection);
        }

        [Benchmark]
        public void RocketQL_____Intro____Parse()
        {
            RQL.Serializers.Serialization.RequestDeserialize(_introspection);
        }

        [Benchmark]
        public void GraphQL______GitHub___Parse()
        {
            GraphQLParser.Parser.Parse(_github);
        }

        [Benchmark]
        public void HotChocolate_GitHub___Parse()
        {
            HC.Utf8GraphQLParser.Parse(_github);
        }

        [Benchmark]
        public void RocketQL_____GitHub___Parse()
        {
            RQL.Serializers.Serialization.SchemaDeserialize(_github);
        }

        [Benchmark]
        public void GraphQL______Onegraph_Parse()
        {
            GraphQLParser.Parser.Parse(_onegraph);
        }

        [Benchmark]
        public void HotChocolate_Onegraph_Parse()
        {
            HC.Utf8GraphQLParser.Parse(_onegraph);
        }

        [Benchmark]
        public void RocketQL_____Onegraph_Parse()
        {
            RQL.Serializers.Serialization.SchemaDeserialize(_onegraph);
        }
    }

    [MemoryDiagnoser]
    public class ValidateBenchmark
    {
        public string _onegraph = "";

        [GlobalSetup]
        public void Setup()
        {
            _onegraph = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "onegraph.graphql"));
        }

        [Benchmark]
        public void HotChocolate_Onegraph_Validate()
        {
            SchemaBuilder
                .New()
                .Use(next => context => throw new NotSupportedException())
                .AddDocumentFromString(_onegraph)
                .Create();
        }

        [Benchmark]
        public void RocketQL_____Onegraph_Validate()
        {
            var schema = new RQL.Base.Schema();
            schema.Add(_onegraph);
            schema.Validate();
        }
    }
}
