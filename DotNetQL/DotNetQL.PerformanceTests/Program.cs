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
        public static string _graphQL = string.Empty;
        public static byte[] _graphQLBytes = Array.Empty<byte>();

        static void Main()
        {
            _graphQL = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "github.graphql"));
            _graphQLBytes = Encoding.ASCII.GetBytes(_graphQL);

            //int countGQL = 0;
            //int resetPosition = 0;
            //GraphQLParser.Token token;
            //while ((token = Lexer.Lex(_graphQL, resetPosition)).Kind != GraphQLParser.TokenKind.EOF)
            //{
            //    resetPosition = token.End;
            //    countGQL++;
            //}

            //int countHC = 0;
            //var reader = new Utf8GraphQLReader(_graphQLBytes);
            //while (reader.Read())
            //{
            //    countHC++;
            //}

            Dictionary<Parser.TokenKind, int> occ = new();
            int countT = 0;
            var t = new Tokenizer(_graphQL.AsSpan());
            while (t.Next() != Parser.TokenKind.EndOfText)
            {
                if (occ.TryGetValue(t.Token, out int val))
                    occ[t.Token] = val + 1;
                else
                    occ[t.Token] = 1;


                countT++;
            }

            foreach(var pair in occ)
            {
                Console.WriteLine($"{pair.Key} = {pair.Value}");
            }
            Console.WriteLine($"{t.Blocks}  {t.Simples}");

            BenchmarkRunner.Run<TokenizerBenchmark>();
            // BenchmarkRunner.Run<ParserBenchmark>();
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
            while (t.Next() != Parser.TokenKind.EndOfText) ;
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