using static Nt.Syntax.Tests.Utils;

namespace Nt.Syntax.LLAnalysing.Tests
{
    public class FollowsAnalyserTest
    {
        private List<List<int>> GetFollows(string filename)
        {
            var parser = new SyntaxParser();
            var grammar = parser.ParseFile(filename);
            var empty_generators = EmptyAnalyser.Analyse(grammar);
            var firsts = FirstsAnalyser.Analyse(grammar, empty_generators);
            var follows = FollowsAnalyser.Analyse(grammar, empty_generators, firsts);

            var follows_list = new List<List<int>>();
            for (int i = 0; i < firsts.Length; i++)
            {
                follows_list.Add(new List<int>(follows[i]));
            }
            return follows_list;
        }

        [Fact]
        public void Follows_test1()
        {
            var follows = GetFollows("../../../Resources/Analyse/Follows/follows_test1.txt");

            Assert.Single(follows);
            Assert.Single(follows[0]);
            Assert.Contains(1, follows[0]);
        }

        [Fact]
        public void Follows_test2()
        {
            var follows = GetFollows("../../../Resources/Analyse/Follows/follows_test2.txt");

            Assert.Equal(2, follows.Count);
            Assert.Single(follows[0]);
            Assert.Contains(2, follows[0]);
            Assert.Single(follows[1]);
            Assert.Contains(0, follows[1]);
        }

        [Fact]
        public void Follows_test3()
        {
            var follows = GetFollows("../../../Resources/Analyse/Follows/follows_test3.txt");

            Assert.Equal(2, follows.Count);
            Assert.Single(follows[0]);
            Assert.Contains(2, follows[0]);

            // Follow(B) = {EOF}
            Assert.Single(follows[1]);
            Assert.Contains(2, follows[1]);
        }

        [Fact]
        public void Follows_test4()
        {
            var follows = GetFollows("../../../Resources/Analyse/Follows/follows_test4.txt");

            Assert.Equal(3, follows.Count);

            // Follow(A) = {EOF}
            Assert.Single(follows[0]);
            Assert.Contains(2, follows[0]);

            // Follow(B) = {a}
            Assert.Single(follows[1]);
            Assert.Contains(0, follows[1]);

            // Follow(C) = {a}
            Assert.Single(follows[2]);
            Assert.Contains(0, follows[2]);
        }

        [Fact]
        public void Follows_test5()
        {
            var follows = GetFollows("../../../Resources/Analyse/Follows/follows_test5.txt");

            Assert.Equal(3, follows.Count);

            // Follow(A) = {EOF}
            Assert.Single(follows[0]);
            Assert.Contains(2, follows[0]);

            // Follow(B) = {a}
            Assert.Single(follows[1]);
            Assert.Contains(1, follows[1]);

            // Follow(C) = {EOF}
            Assert.Single(follows[2]);
            Assert.Contains(2, follows[2]);
        }

        [Fact]
        public void Follows_test6()
        {
            var follows = GetFollows("../../../Resources/Analyse/Follows/follows_test6.txt");

            Assert.Equal(3, follows.Count);

            // Follow(A) = {EOF}
            Assert.Single(follows[0]);
            Assert.Contains(2, follows[0]);

            // Follow(B) = {a, EOF}
            Assert.Equal(2, follows[1].Count);
            AssertContains(follows[1], [1, 2]);

            // Follow(C) = {EOF}
            Assert.Single(follows[2]);
            Assert.Contains(2, follows[2]);
        }
    }

}
