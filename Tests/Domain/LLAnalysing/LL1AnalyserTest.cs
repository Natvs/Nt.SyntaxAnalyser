using Nt.Syntax;
using Nt.Syntax.LLAnalysing;
using Nt.Parsing;
using Nt.Parsing.Structures;
using Nt.Syntax.Structures;

namespace Nt.SyntaxAnalyser.LLAnalysing.Tests
{
    record AnalyseResultWrapper(Grammar Grammar, ParserResult ParserResult, LL1Analyser.AnalyseResult AnalyseResult) { }

    public class LL1AnalyserTest
    {
        private AnalyseResultWrapper AnalyseString(string filename, string value)
        {
            var syntaxparser = new SyntaxParser();
            var grammar = syntaxparser.ParseFile(filename);

            var parser = new Parser(['\n', ' '], [";", "="]);
            var parserResult = parser.Parse(value);

            var analyseSet = LL1AnalyseSet.Get(grammar);
            var checkpoints = new SymbolsList([";"]);
            var analyseResult = LL1Analyser.Analyse(analyseSet, parserResult, checkpoints);

            return new AnalyseResultWrapper(grammar, parserResult, analyseResult);
        }

        private void AssertSyntaxError(AnalyseResultWrapper result, int errorIndex, int expectedLine, string unexpectedSymbol)
        {
            Assert.True(errorIndex < result.AnalyseResult.SyntaxExceptions.Length);
            Assert.Equal(expectedLine, result.AnalyseResult.SyntaxExceptions[errorIndex].Data0.Line);
            Assert.Equal(result.ParserResult.Symbols.IndexOf(unexpectedSymbol), result.AnalyseResult.SyntaxExceptions[errorIndex].Data0.TokenIndex);
        }

        [Fact]
        public void LL1Analyser_test1()
        {
            var result = AnalyseString("../../../Resources/Analyse/grammar_1.txt", "a");

            Assert.Empty(result.AnalyseResult.SyntaxExceptions);
            Assert.Empty(result.AnalyseResult.RegexExceptions);
            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, result.AnalyseResult.EndOfFileStatus);
        }

        [Fact]
        public void LL1Analyser_test2()
        {
            var result = AnalyseString("../../../Resources/Analyse/grammar_1.txt", "b");

            Assert.Single(result.AnalyseResult.SyntaxExceptions);
            AssertSyntaxError(result, 0, 1, "b");

            Assert.Empty(result.AnalyseResult.RegexExceptions);
        }

        [Fact]
        public void LL1Analyser_test3()
        {
            var analyseResult = AnalyseString("../../../Resources/Analyse/grammar_2.txt", "var a = 1;");

            Assert.Empty(analyseResult.AnalyseResult.SyntaxExceptions);
            Assert.Empty(analyseResult.AnalyseResult.RegexExceptions);
            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, analyseResult.AnalyseResult.EndOfFileStatus);
        }

        [Fact]
        public void LL1Analyser_test4()
        {
            var analyseResult = AnalyseString("../../../Resources/Analyse/grammar_2.txt", "var d = 1;");

            Assert.Single(analyseResult.AnalyseResult.SyntaxExceptions);
            AssertSyntaxError(analyseResult, 0, 1, "d");

            Assert.Empty(analyseResult.AnalyseResult.RegexExceptions);
            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, analyseResult.AnalyseResult.EndOfFileStatus);
        }
    }
}
