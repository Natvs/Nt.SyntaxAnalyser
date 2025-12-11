using Nt.Syntax;
using Nt.Syntax.LLAnalysing;
using Nt.Parsing;

namespace Nt.SyntaxAnalyser.LLAnalysing.Tests
{
    public class LL1AnalyserTest
    {

        [Fact]
        public void LL1Analyser_test1()
        {
            var syntaxparser = new SyntaxParser();
            var grammar = syntaxparser.ParseFile("../../../Resources/Analyse/TestGrammar.txt");

            var parser = new Parser(['\n'], []);
            var parserResult = parser.Parse("a");

            var analyseResult = LL1Analyser.Analyse(grammar, parserResult, []);

            Assert.Empty(analyseResult.SyntaxExceptions);
            Assert.Empty(analyseResult.RegexExceptions);
            Assert.Equal(LL1Analyser.EndOfFileStatus.Valid, analyseResult.EndOfFileStatus);
        }

        [Fact]
        public void LL1Analyser_test2()
        {
            var syntaxparser = new SyntaxParser();
            var grammar = syntaxparser.ParseFile("../../../Resources/Analyse/TestGrammar.txt");

            var parser = new Parser(['\n'], []);
            var parserResult = parser.Parse("b");

            var analyseResult = LL1Analyser.Analyse(grammar, parserResult, []);

            Assert.Equal(1, analyseResult.SyntaxExceptions.Length);
            var syntaxException = analyseResult.SyntaxExceptions[0];
            Assert.Equal("a", grammar.Terminals[syntaxException.Data1.Index].Name);
            Assert.Equal("b", parserResult.Symbols[syntaxException.Data0.TokenIndex].Name);

            Assert.Empty(analyseResult.RegexExceptions);
        }
    }
}
