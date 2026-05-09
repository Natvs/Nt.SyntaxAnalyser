using Nt.Syntax.LLParsing;
using Nt.Syntax;

namespace Nt.Tests.Syntax.LLParsing
{
    public class RegexMergeTest : SetupFixture
    {

        [Fact]
        public void RegexMerge_SameSymbolRegexs_ShouldMergeRegexs()
        {
            // Test a single factorisation
            TestParsing("../../../Resources/Regex/Raw/regex_test1.txt", "../../../Resources/Regex/Result/regex_test1_result.txt");
        }

        [Fact]
        public void RegexMerge_SeveralSameSymbolRegexs_ShouldMergeRegexs()
        {
            // Test multiple factorisations
            TestParsing("../../../Resources/Regex/Raw/regex_test2.txt", "../../../Resources/Regex/Result/regex_test2_result.txt");
        }

        [Fact]
        public void RegexMerge_SameSymbolRuleStaringWithRegex_ShouldMergeRulesAndRegexs()
        {
            // Test nested factorisations
            TestParsing("../../../Resources/Regex/Raw/regex_test3.txt", "../../../Resources/Regex/Result/regex_test3_result.txt");
        }

    }
}
