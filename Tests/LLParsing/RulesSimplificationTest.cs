using System;
using System.Collections.Generic;
using System.Text;

namespace Nt.Tests.Syntax.LLParsing
{
    public class RulesSimplificationTest : SetupFixture
    {

        [Fact]
        public void RulesSimplification_DoubleRule_ShouldRemoveOneRule()
        {
            TestParsing("../../../Resources/Rules/Raw/rules_test1.txt", "../../../Resources/Rules/Result/rules_test1_result.txt");
        }

        [Fact]
        public void RulesSimplification_UnreachableRule_ShouldRemoveRule()
        {
            TestParsing("../../../Resources/Rules/Raw/rules_test2.txt", "../../../Resources/Rules/Result/rules_test2_result.txt");
        }

        [Fact]
        public void RulesSimplification_UnreachableRule_ShouldRemoveTransitiveRules()
        {
            TestParsing("../../../Resources/Rules/Raw/rules_test3.txt", "../../../Resources/Rules/Result/rules_test3_result.txt");
        }

        [Fact]
        public void RulesSimplification_SymbolThatDefineOnlyOneRule_ShouldBeSubstituted()
        {
            TestParsing("../../../Resources/Rules/Raw/rules_test4.txt", "../../../Resources/Rules/Result/rules_test4_result.txt");
        }

        [Fact]
        public void RulesSimplification_SymbolThatDefineMultipleRules_ShouldNotBeSubstituted()
        {
            TestParsing("../../../Resources/Rules/Raw/rules_test5.txt", "../../../Resources/Rules/Result/rules_test5_result.txt");
        }

    }
}
