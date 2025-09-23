using Nt.SyntaxParser.Parsing.Structures;
using Nt.SyntaxParser.Syntax.Structures;

namespace Nt.SyntaxAnalyser.Tests.Domain
{
    public class UtilsTest
    {
        [Fact]
        public void Test1()
        {
            var terminals = new TokensList(["a", "b"]);
            var nonterminals = new TokensList(["A", "B"]);
            var derivation = new Derivation(terminals, nonterminals);
            var newrule = Utils.create_rule(terminals, nonterminals, 0, derivation);

            Assert.NotNull(newrule);
            Assert.NotNull(newrule.Token);
            Assert.Equal(0, newrule.Token.Index);
            Assert.Empty(newrule.Derivation);
        }
    }
}
