using Nt.Parser.Symbols;
using Nt.Syntax;
using Nt.Syntax.LLAnalysing;
using static Nt.Tests.Syntax.Utils;

namespace Nt.Tests.Syntax.LLAnalysing
{
    public class FirstsAnalyserTest : SetupFixture
    {

        private static Dictionary<string, IEnumerable<string>> GetFirsts(string filename)
        {
            var parser = new SyntaxParser();
            var grammar = parser.ParseFile(filename);
            var empty_generators = EmptyAnalyser.Analyse(grammar);
            var firsts = FirstsAnalyser.Analyse(grammar, empty_generators).ToDictionary();

            var firsts_dic = new Dictionary<string, IEnumerable<string>>();
            foreach (var line in firsts)
            {
                firsts_dic[((ISymbol)line.Key).Name] = line.Value.Select(s => ((ISymbol)s).Name);
            }
            return firsts_dic;
        }

        [Fact]
        public void Firsts_test1()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/Firsts/firsts_test1.txt");

            AssertContains(firsts["A"], ["a"]);
        }

        [Fact]
        public void Firsts_test2()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/Firsts/firsts_test2.txt");

            AssertContains(firsts["A"], ["a"]);
        }

        [Fact]
        public void Firsts_test3()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/Firsts/firsts_test3.txt");

            AssertContains(firsts["A"], ["b"]);
            AssertContains(firsts["B"], ["b"]);
        }

        [Fact]
        public void Firsts_test4()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/Firsts/firsts_test4.txt");

            AssertContains(firsts["A"], ["a"]);
            AssertEmpty(firsts, "B");

        }

        [Fact]
        public void Firsts_test5()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/Firsts/firsts_test5.txt");

            AssertContains(firsts["A"], ["a", "b"]);
            AssertContains(firsts["B"], ["b"]);
        }

        [Fact]
        public void Firsts_test6()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/Firsts/firsts_test6.txt");

            AssertContains(firsts["A"], ["a", "b", "c", "d", "e", "f", "g", "h"]);
            AssertContains(firsts["B"], ["d", "e", "f", "g", "h"]);
            AssertContains(firsts["C"], ["e"]);
            AssertContains(firsts["D"], ["g", "h"]);
        }

    }
}
