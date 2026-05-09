using Nt.Syntax.LLParsing;
using Nt.Syntax;

namespace Nt.Tests.Syntax.LLParsing
{
    public class RegexMergeTest : SetupFixture
    {
        private static void TestMerge(string rawpath, string resultpath)
        {
            var parser = new SyntaxParser();
            var grammar = parser.ParseFile(rawpath);
            grammar = LL1Parser.Parse(grammar);

            var result_grammar = parser.ParseFile(resultpath);
            Assert.True(Comparator.CompareGrammars(grammar, result_grammar));
        }

        [Fact]
        public void RegexMerge_SameSymbolRegexs_ShouldMergeRegexs()
        {
            // Test a single factorisation
            TestMerge("../../../Resources/Regex/Raw/regex_test1.txt", "../../../Resources/Regex/Result/regex_test1_result.txt");
        }

        [Fact]
        public void RegexMerge_SeveralSameSymbolRegexs_ShouldMergeRegexs()
        {
            // Test multiple factorisations
            TestMerge("../../../Resources/Regex/Raw/regex_test2.txt", "../../../Resources/Regex/Result/regex_test2_result.txt");
        }

        [Fact]
        public void RegexMerge_SameSymbolRuleStaringWithRegex_ShouldMergeRulesAndRegexs()
        {
            // Test nested factorisations
            TestMerge("../../../Resources/Regex/Raw/regex_test3.txt", "../../../Resources/Regex/Result/regex_test3_result.txt");
        }

    }
}
