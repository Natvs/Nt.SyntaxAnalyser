using Nt.Syntax.LLParsing;
using Nt.Syntax;

namespace Nt.Tests.Syntax.LLParsing
{
    public class DerecursivationTest : SetupFixture
    {
        private static void TestDerecursivation(string rawpath, string resultpath)
        {
            var parser = new SyntaxParser();
            var grammar = parser.ParseFile(rawpath);
            grammar = Derecursivation.EliminateRecursivity(grammar);

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

        [Fact]
        public void LL1Parser_rec_test3()
        {
            // Test indirect recursivity elimination
            TestDerecursivation("../../../Resources/Rec/Raw/rec_test3.txt", "../../../Resources/Rec/Result/rec_test3_result.txt");
        }

        [Fact]
        public void LL1Parser_rec_test4()
        {
            // Test double direct recursivity elimination
            TestDerecursivation("../../../Resources/Rec/Raw/rec_test4.txt", "../../../Resources/Rec/Result/rec_test4_result.txt");
        }

        [Fact]
        public void LL1Parser_rec_test5()
        {
            // Test double indirect recursivity elimination
            TestDerecursivation("../../../Resources/Rec/Raw/rec_test5.txt", "../../../Resources/Rec/Result/rec_test5_result.txt");
        }

        [Fact]
        public void LL1Parser_rec_test6()
        {
            // Test indirect derecursivity when first symbol has two derivations
            TestDerecursivation("../../../Resources/Rec/Raw/rec_test6.txt", "../../../Resources/Rec/Result/rec_test6_result.txt");
        }

        [Fact]
        public void LL1Parser_rec_test7()
        {
            // Test mixed direct and indirect recursivity elimination
            TestDerecursivation("../../../Resources/Rec/Raw/rec_test7.txt", "../../../Resources/Rec/Result/rec_test7_result.txt");
        }
    }
}
