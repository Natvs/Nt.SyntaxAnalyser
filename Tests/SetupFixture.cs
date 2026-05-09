using Nt.Parser.Symbols;
using Nt.Syntax;
using Nt.Syntax.LLParsing;
using static Nt.Syntax.LLAnalysing.Utils;

namespace Nt.Tests.Syntax
{
    public class SetupFixture
    {
        private static SyntaxParserConfig Config { get; } = SyntaxParserConfig.GetInstance();
        private static SyntaxSymbolFactory Factory { get; } = new SyntaxSymbolFactory();
        public SetupFixture()
        {
            Config.SetSymbolFactory(Factory);
        }

        protected static void TestParsing(string rawpath, string resultpath)
        {
            var parser = new SyntaxParser();
            var grammar = parser.ParseFile(rawpath);
            grammar = LL1Parser.Parse(grammar);

            var result_grammar = parser.ParseFile(resultpath);
            Assert.True(Comparator.CompareGrammars(grammar, result_grammar));
        }

    }
}
