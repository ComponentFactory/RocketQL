using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using GraphQLParser;

namespace DotNetQL.PerformanceTests
{
    internal class Program
    {
        public static string _graphQL = string.Empty;

        static void Main()
        {
            _graphQL = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "github.graphql"));
            BenchmarkRunner.Run<TokenizerBenchmark>();
            BenchmarkRunner.Run<ParserBenchmark>();
        }
    }

    [MemoryDiagnoser]
    public class TokenizerBenchmark
    {
        [Benchmark]
        public void GraphQL()
        {
            string graphQL = Program._graphQL;
            int resetPosition = 0;
            Token token;
            while ((token = Lexer.Lex(graphQL, resetPosition)).Kind != TokenKind.EOF)
            {
                resetPosition = token.End;
            }
        }
    }

    [MemoryDiagnoser]
    public class ParserBenchmark
    {
        [Benchmark]
        public void GraphQL()
        {
            GraphQLParser.Parser.Parse(Program._graphQL);
        }
    }
}