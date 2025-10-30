namespace Nt.Syntax.LLParsing.Tests {
    public class LL1ParserTest
    {

        // Derecursivation testing

        private static void TestDerecursivation(string rawpath, string resultpath)
        {
            var parser = new SyntaxParser();
            var grammar = parser.ParseFile(rawpath);
            grammar = Derecursivation.EliminateRecursivity(grammar);

            var result_grammar = parser.ParseFile(resultpath);
            Assert.True(Comparator.CompareGrammars(grammar, result_grammar));
        }
        private static void TestFactorisation(string rawpath, string resultpath)
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
            // Test simple direct recursivity elimination
            TestDerecursivation("../../../Resources/Rec/Raw/rec_test1.txt", "../../../Resources/Rec/Result/rec_test1_result.txt");
        }

        [Fact]
        public void LL1Parser_rec_test2()
        {
            // Test direct recursivity elimination on two non-terminals
            TestDerecursivation("../../../Resources/Rec/Raw/rec_test2.txt", "../../../Resources/Rec/Result/rec_test2_result.txt");
        }


        // Factorisation testing

        [Fact]
        public void LL1Parser_fact_test1()
        {
            // Test a single factorisation
            TestFactorisation("../../../Resources/Fact/Raw/fact_test1.txt", "../../../Resources/Fact/Result/fact_test1_result.txt");
        }

        [Fact]
        public void LL1Parser_fact_test2()
        {
            // Test factorisation with two non-terminals to factorise
            TestFactorisation("../../../Resources/Fact/Raw/fact_test2.txt", "../../../Resources/Fact/Result/fact_test2_result.txt");
        }

        [Fact]
        public void LL1Parser_fact_test3()
        {
            // Test a factorisation with the empty word
            TestFactorisation("../../../Resources/Fact/Raw/fact_test3.txt", "../../../Resources/Fact/Result/fact_test3_result.txt");
        }

        [Fact]
        public void LL1Parser_fact_test4()
        {
            // Test factorisation with two different factorisations on the same non-terminal
            TestFactorisation("../../../Resources/Fact/Raw/fact_test4.txt", "../../../Resources/Fact/Result/fact_test4_result.txt");
        }

        [Fact]
        public void LL1Parser_fact_test5()
        {
            // Test nested factorisations
            TestFactorisation("../../../Resources/Fact/Raw/fact_test5.txt", "../../../Resources/Fact/Result/fact_test5_result.txt");
        }
 

    }
}
