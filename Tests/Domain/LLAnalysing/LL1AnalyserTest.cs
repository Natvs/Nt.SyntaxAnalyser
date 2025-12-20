using Nt.Syntax;
using Nt.Syntax.LLAnalysing;
using Nt.Parsing;
using Nt.Parsing.Structures;
using Nt.Syntax.Structures;
using Nt.Syntax.LLParsing;

namespace Nt.SyntaxAnalyser.LLAnalysing.Tests
{
    record AnalyseResultWrapper(Grammar Grammar, ParserResult ParserResult, LL1Analyser.AnalyseResult AnalyseResult) { }

    public class LL1AnalyserTest
    {
        private static AnalyseResultWrapper AnalyseString(string filename, string value)
        {
            var syntaxparser = new SyntaxParser();
            var grammar = syntaxparser.ParseFile(filename);

            var parser = new Parser(['\n', ' '], [";", "="]);
            var parserResult = parser.Parse(value);
            LL1Parser.Parse(grammar);

            var analyseSet = LL1AnalyseSet.Get(grammar, new SymbolsList([";"]));
            var analyseResult = LL1Analyser.Analyse(analyseSet, parserResult);

            return new AnalyseResultWrapper(grammar, parserResult, analyseResult);
        }

        private static void AssertSyntaxError(AnalyseResultWrapper result, int errorIndex, int expectedLine, string unexpectedSymbol)
        {
            Assert.True(errorIndex < result.AnalyseResult.SyntaxExceptions.Length);
            Assert.Equal(expectedLine, result.AnalyseResult.SyntaxExceptions[errorIndex].Data0.Line);
            Assert.Equal(result.ParserResult.Symbols.IndexOf(unexpectedSymbol), result.AnalyseResult.SyntaxExceptions[errorIndex].Data0.TokenIndex);
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

            Assert.Single(result.AnalyseResult.SyntaxExceptions);
            AssertSyntaxError(result, 0, 1, "b");
        }

        [Fact]
        public void LL1Analyser_WithoutRegex_test3()
        {
            var analyseResult = AnalyseString("../../../Resources/Analyse/grammar_2.txt", "var a = 1;");

            Assert.Empty(analyseResult.AnalyseResult.SyntaxExceptions);
            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, analyseResult.AnalyseResult.EndOfFileStatus);
        }

        [Fact]
        public void LL1Analyser_WithoutRegex_test4()
        {
            var analyseResult = AnalyseString("../../../Resources/Analyse/grammar_2.txt", "var d = 1;");

            Assert.Single(analyseResult.AnalyseResult.SyntaxExceptions);
            AssertSyntaxError(analyseResult, 0, 1, "d");

            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, analyseResult.AnalyseResult.EndOfFileStatus);
        }

        [Fact]
        public void LL1Analyser_WithRegex_test1()
        {
            var analyseResult = AnalyseString("../../../Resources/Analyse/grammar_3.txt", "var d = 123;");

            Assert.Empty(analyseResult.AnalyseResult.SyntaxExceptions);
            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, analyseResult.AnalyseResult.EndOfFileStatus);
        }

        [Fact]
        public void LL1Analyser_WithRegex_test2()
        {
            var analyseResult = AnalyseString("../../../Resources/Analyse/grammar_3.txt", "var d% = 123;");

            Assert.Single(analyseResult.AnalyseResult.SyntaxExceptions);
            AssertSyntaxError(analyseResult, 0, 1, "d%");

            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, analyseResult.AnalyseResult.EndOfFileStatus);
        }

        [Fact]
        public void LL1Analyser_WithRegex_test3()
        {
            var analyseResult = AnalyseString("../../../Resources/Analyse/grammar_3.txt", "var d = 12_3;");

            Assert.Single(analyseResult.AnalyseResult.SyntaxExceptions);
            AssertSyntaxError(analyseResult, 0, 1, "12_3");

            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, analyseResult.AnalyseResult.EndOfFileStatus);
        }

        [Fact]
        public void LL1Analyser_WithRegex_test4()
        {
            var analyseResult = AnalyseString("../../../Resources/Analyse/grammar_4.txt", "var d = e;");

            Assert.Empty(analyseResult.AnalyseResult.SyntaxExceptions);
            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, analyseResult.AnalyseResult.EndOfFileStatus);
        }

        [Fact]
        public void LL1Analyser_WithRegex_test5()
        {
            var analyseResult = AnalyseString("../../../Resources/Analyse/grammar_4.txt", "var d = 42;");

            Assert.Empty(analyseResult.AnalyseResult.SyntaxExceptions);
            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, analyseResult.AnalyseResult.EndOfFileStatus);
        }

        [Fact]
        public void LL1Analyser_WithRegex_test6()
        {
            var analyseResult = AnalyseString("../../../Resources/Analyse/grammar_4.txt", "var d = $;");

            Assert.Single(analyseResult.AnalyseResult.SyntaxExceptions);
            AssertSyntaxError(analyseResult, 0, 1, "$");

            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, analyseResult.AnalyseResult.EndOfFileStatus);
        }
    }
}
