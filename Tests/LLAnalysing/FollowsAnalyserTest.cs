using Nt.Parser.Symbols;
using Nt.Syntax;
using Nt.Syntax.LLAnalysing;
using static Nt.Tests.Syntax.Utils;

namespace Nt.Tests.Syntax.LLAnalysing
{
    public class FollowsAnalyserTest : SetupFixture
    {
        private static Dictionary<string, IEnumerable<string>> GetFollows(string filename)
        {
            var parser = new SyntaxParser();
            var grammar = parser.ParseFile(filename);
            var empty_generators = EmptyAnalyser.Analyse(grammar);
            var firsts = FirstsAnalyser.Analyse(grammar, empty_generators);
            var follows = FollowsAnalyser.Analyse(grammar, empty_generators, firsts);

            var follows_dic = new Dictionary<string, IEnumerable<string>>();
            foreach (var line in follows)
            {
                follows_dic[((ISymbol)line.Key).Name] = line.Value.Select(s => ((ISymbol)s).Name);
            }
            return follows_dic;
        }

        [Fact]
        public void Follows_test1()
        {
            var follows = GetFollows("../../../Resources/Analyse/Follows/follows_test1.txt");

            AssertContains(follows["A"], ["EOF"]);
        }

        [Fact]
        public void Follows_test2()
        {
            var follows = GetFollows("../../../Resources/Analyse/Follows/follows_test2.txt");

            AssertContains(follows["A"], ["EOF"]);
            AssertContains(follows["B"], ["a"]);
        }

        [Fact]
        public void Follows_test3()
        {
            var follows = GetFollows("../../../Resources/Analyse/Follows/follows_test3.txt");

            AssertContains(follows["A"], ["EOF"]);
            AssertContains(follows["B"], ["EOF"]);
        }

        [Fact]
        public void Follows_test4()
        {
            var follows = GetFollows("../../../Resources/Analyse/Follows/follows_test4.txt");

            AssertContains(follows["A"], ["EOF"]);
            AssertContains(follows["B"], ["a"]);
            AssertContains(follows["C"], ["a"]);
        }

        [Fact]
        public void Follows_test5()
        {
            var follows = GetFollows("../../../Resources/Analyse/Follows/follows_test5.txt");

            AssertContains(follows["A"], ["EOF"]);
            AssertContains(follows["B"], ["b"]);
            AssertContains(follows["C"], ["EOF"]);
        }

        [Fact]
        public void Follows_test6()
        {
            var follows = GetFollows("../../../Resources/Analyse/Follows/follows_test6.txt");

            AssertContains(follows["A"], ["EOF"]);
            AssertContains(follows["B"], ["b", "EOF"]);
            AssertContains(follows["C"], ["EOF"]);
        }
    }

}
