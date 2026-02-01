using Nt.Syntax;
using Nt.Syntax.LLAnalysing;
using Nt.Syntax.Structures;
using Nt.Syntax.LLParsing;
using Nt.Parser;
using Nt.Parser.Symbols;
using Nt.Parser.Structures;
using static Nt.Syntax.LLAnalysing.Utils;

namespace Nt.Tests.Syntax.LLAnalysing
{
    record AnalyseResultWrapper(Grammar Grammar, ParserResult ParserResult, LL1Analyser.AnalyseResult AnalyseResult) { }

    public class LL1AnalyserTest : SetupFixture
    {
        private static AnalyseResultWrapper AnalyseString(string filename, string value)
        {
            var syntaxparser = new SyntaxParser();
            var grammar = syntaxparser.ParseFile(filename);

            var factory = new SyntaxSymbolFactory();
            var parser = new SymbolsParser(factory, ['\n', ' '], [";", "="]);
            var parserResult = parser.Parse(value);
            LL1Parser.Parse(grammar);

            var analyseSet = LL1AnalyseSet.Get(grammar, new SymbolsList(factory, [";"]));
            var analyseResult = LL1Analyser.Analyse(analyseSet, parserResult);

            return new AnalyseResultWrapper(grammar, parserResult, analyseResult);
        }

        private static void AssertSyntaxError(LL1Analyser.AnalyseResult result, List<string> expected)
        {
            Assert.Equal(expected.Count, result.SyntaxExceptions.Length);
            foreach (var expected_symbol in expected)
            {
                var exception = result.SyntaxExceptions.FirstOrDefault(e => e.Data0.Symbol.Name == expected_symbol);
                Assert.NotNull(exception);
            }
        }

        private static void AssertValidResult(LL1Analyser.AnalyseResult result)
        {
            Assert.Empty(result.SyntaxExceptions);
            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, result.EndOfFileStatus);
        }


        [Fact]
        public void LL1Analyser_WithoutRegex_test1()
        {
            var result = AnalyseString("../../../Resources/Analyse/grammar_1.txt", "a");

            Assert.Empty(result.AnalyseResult.SyntaxExceptions);
            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, result.AnalyseResult.EndOfFileStatus);
        }

        [Fact]
        public void LL1Analyser_WithoutRegex_test2()
        {
            var result = AnalyseString("../../../Resources/Analyse/grammar_1.txt", "b");

            AssertSyntaxError(result.AnalyseResult, ["b"]);
        }

        [Fact]
        public void LL1Analyser_WithoutRegex_test3()
        {
            var result = AnalyseString("../../../Resources/Analyse/grammar_2.txt", "var a = 1;");

            AssertValidResult(result.AnalyseResult);
        }

        [Fact]
        public void LL1Analyser_WithoutRegex_test4()
        {
            var result = AnalyseString("../../../Resources/Analyse/grammar_2.txt", "var d = 1;");

            AssertSyntaxError(result.AnalyseResult, ["d"]);
        }

        [Fact]
        public void LL1Analyser_WithRegex_test1()
        {
            var analyseResult = AnalyseString("../../../Resources/Analyse/grammar_3.txt", "var d = 123;");

            AssertValidResult(analyseResult.AnalyseResult);
        }

        [Fact]
        public void LL1Analyser_WithRegex_test2()
        {
            var result = AnalyseString("../../../Resources/Analyse/grammar_3.txt", "var d% = 123;");

            AssertSyntaxError(result.AnalyseResult, ["d%"]);
        }

        [Fact]
        public void LL1Analyser_WithRegex_test3()
        {
            var result = AnalyseString("../../../Resources/Analyse/grammar_3.txt", "var d = 12_3;");

            AssertSyntaxError(result.AnalyseResult, ["12_3"]);
        }

        [Fact]
        public void LL1Analyser_WithRegex_test4()
        {
            var result = AnalyseString("../../../Resources/Analyse/grammar_4.txt", "var d = e;");

            Assert.Empty(result.AnalyseResult.SyntaxExceptions);
            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, result.AnalyseResult.EndOfFileStatus);
        }

        [Fact]
        public void LL1Analyser_WithRegex_test5()
        {
            var result = AnalyseString("../../../Resources/Analyse/grammar_4.txt", "var d = 42;");

            Assert.Empty(result.AnalyseResult.SyntaxExceptions);
            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, result.AnalyseResult.EndOfFileStatus);
        }

        [Fact]
        public void LL1Analyser_WithRegex_test6()
        {
            var result = AnalyseString("../../../Resources/Analyse/grammar_4.txt", "var d = $;");

            AssertSyntaxError(result.AnalyseResult, ["$"]);

            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, result.AnalyseResult.EndOfFileStatus);
        }
    }
}
