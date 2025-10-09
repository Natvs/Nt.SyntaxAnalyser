using Nt.SyntaxParser.Syntax;
using Nt.SyntaxAnalyser.LLParsing;
using Nt.SyntaxParser.Syntax.Structures;
using Microsoft.FSharp.Collections;

namespace Nt.SyntaxAnalyser.Tests.Domain.LLParsing
{
    public class LL1ParserTest
    {
        [Fact]
        public void LL1Parser_rec_test1()
        {
            var parser = new SyntaxParser.Syntax.SyntaxParser();
            var grammar = parser.ParseFile("../../../Resources/fact_test1.txt");
            grammar = Derecursivation.EliminateRecursivity(grammar);
            var result_grammar = parser.ParseFile("../../../Resources/fact_test1_result.txt");

            Assert.True(Comparator.CompareGrammars(grammar, result_grammar));
        }

        [Fact]
        public void LL1Parser_rec_test2()
        {
            var parser = new SyntaxParser.Syntax.SyntaxParser();
            var grammar = parser.ParseFile("../../../Resources/fact_test2.txt");
            grammar = Derecursivation.EliminateRecursivity(grammar);
            var result_grammar = parser.ParseFile("../../../Resources/fact_test2_result.txt");

            Assert.True(Comparator.CompareGrammars(grammar, result_grammar));
        }


    }
}
