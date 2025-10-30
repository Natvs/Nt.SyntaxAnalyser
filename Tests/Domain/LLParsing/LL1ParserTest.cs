using Nt.Syntax.Structures;

namespace Nt.Syntax.LLParsing.Tests {
    public class LL1ParserTest
    {
        [Fact]
        public void LL1Parser_rec_test1()
        {
            var parser = new SyntaxParser();
            var grammar = parser.ParseFile("../../../Resources/rec_test1.txt");
            grammar = Derecursivation.EliminateRecursivity(grammar);
            var result_grammar = parser.ParseFile("../../../Resources/rec_test1_result.txt");

            Assert.True(Comparator.CompareGrammars(grammar, result_grammar));
        }

        [Fact]
        public void LL1Parser_rec_test2()
        {
            var parser = new SyntaxParser();
            var grammar = parser.ParseFile("../../../Resources/rec_test2.txt");
            grammar = Derecursivation.EliminateRecursivity(grammar);
            var result_grammar = parser.ParseFile("../../../Resources/rec_test2_result.txt");

            Assert.True(Comparator.CompareGrammars(grammar, result_grammar));
        }

        [Fact]
        public void LL1Parser_fact_test1()
        {
            var parser = new SyntaxParser();
            var grammar = parser.ParseFile("../../../Resources/fact_test1.txt");
            grammar = Factorisation.Factorise(grammar);
            var result_grammar = parser.ParseFile("../../../Resources/fact_test1_result.txt");

            Assert.True(Comparator.CompareGrammars(grammar, result_grammar));
        }

        [Fact]
        public void LL1Parser_fact_test4()
        {
            var parser = new SyntaxParser();
            var grammar = parser.ParseFile("../../../Resources/fact_test4.txt");
            grammar = Factorisation.Factorise(grammar);
            var result_grammar = parser.ParseFile("../../../Resources/fact_test4_result.txt");

            Assert.True(Comparator.CompareGrammars(grammar, result_grammar));
        }

    }
}
