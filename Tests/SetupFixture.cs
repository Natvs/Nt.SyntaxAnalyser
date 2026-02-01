using Nt.Parser.Symbols;
using Nt.Syntax;
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

    }
}
