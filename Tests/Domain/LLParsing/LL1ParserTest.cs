using Nt.SyntaxParser.Syntax;
using Nt.SyntaxAnalyser.LLParsing;
using Nt.SyntaxParser.Syntax.Structures;

namespace Nt.SyntaxAnalyser.Tests.Domain.LLParsing
{
    public class LL1ParserTest
    {
        [Fact]
        public void LL1Parser_test1()
        {
            var parser = new SyntaxParser.Syntax.SyntaxParser();
            var grammar = parser.ParseFile("../../../Resources/fact_test1.txt");
            var new_grammar = Derecursivation.EliminateRecursivity(grammar);
            new_grammar.ToString();
        }


    }
}
