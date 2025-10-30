using Nt.Syntax.Structures;

namespace Nt.Syntax.LLParsing.Tests {
    public class LL1ParserTest
    {

        private void TestDerecursivation(string rawpath, string resultpath)
        {
            var parser = new SyntaxParser();
            var grammar = parser.ParseFile(rawpath);
            grammar = Derecursivation.EliminateRecursivity(grammar);

            var result_grammar = parser.ParseFile(resultpath);
            Assert.True(Comparator.CompareGrammars(grammar, result_grammar));
        }
        private void TestFactorisation(string rawpath, string resultpath)
        {
            var parser = new SyntaxParser();
            var grammar = parser.ParseFile(rawpath);
            grammar = Factorisation.Factorise(grammar);

            var result_grammar = parser.ParseFile(resultpath);
            Assert.True(Comparator.CompareGrammars(grammar, result_grammar));
        }

        [Fact]
        public void LL1Parser_rec_test1()
        {
            TestDerecursivation("../../../Resources/Rec/Raw/rec_test1.txt", "../../../Resources/Rec/Result/rec_test1_result.txt");
        }

        [Fact]
        public void LL1Parser_rec_test2()
        {
            TestDerecursivation("../../../Resources/Rec/Raw/rec_test2.txt", "../../../Resources/Rec/Result/rec_test2_result.txt");
        }

        [Fact]
        public void LL1Parser_fact_test1()
        {
            TestFactorisation("../../../Resources/Fact/Raw/fact_test1.txt", "../../../Resources/Fact/Result/fact_test1_result.txt");
        }

        [Fact]
        public void LL1Parser_fact_test2()
        {
            TestFactorisation("../../../Resources/Fact/Raw/fact_test2.txt", "../../../Resources/Fact/Result/fact_test2_result.txt");
        }

        [Fact]
        public void LL1Parser_fact_test4()
        {
            TestFactorisation("../../../Resources/Fact/Raw/fact_test4.txt", "../../../Resources/Fact/Result/fact_test4_result.txt");
        }

    }
}
