using Nt.Syntax;
using Nt.Syntax.LLAnalysing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nt.SyntaxAnalyser.Tests.Domain.LLAnalysing
{
    public class FirstsAnalyserTest
    {

        public List<List<int>> GetFirsts(string filename)
        {
            var parser = new SyntaxParser();
            var grammar = parser.ParseFile(filename);
            var empty_generators = EmptyAnalyser.Analyse(grammar);
            var firsts = FirstsAnalyser.Analyse(grammar, empty_generators);

            var firsts_list = new List<List<int>>();
            for (int i = 0; i < firsts.Length; i++) 
            {
                firsts_list.Add(new List<int>(firsts[i]));
            }
            return firsts_list;
        }

        private void AssertContains<T>(ICollection<T> collection, ICollection<T> values)
        {
            foreach (var value in values)
            {
                Assert.Contains(value, collection);
            }
        }

        [Fact]
        public void Firsts_test1()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/Firsts/firsts_test1.txt");

            Assert.Single(firsts);
            Assert.Single(firsts[0]);
            Assert.Contains(0, firsts[0]);
        }

        [Fact]
        public void Firsts_test2()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/Firsts/firsts_test2.txt");

            Assert.Single(firsts);
            Assert.Single(firsts[0]);
            Assert.Contains(0, firsts[0]);
        }

        [Fact]
        public void Firsts_test3()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/Firsts/firsts_test3.txt");

            Assert.Equal(2, firsts.Count);
            Assert.Single(firsts[0]);
            Assert.Single(firsts[1]);
            Assert.Contains(0, firsts[0]);
            Assert.Contains(0, firsts[1]);
        }

        [Fact]
        public void Firsts_test4()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/Firsts/firsts_test4.txt");

            Assert.Equal(2, firsts.Count);
            Assert.Single(firsts[0]);
            Assert.Empty(firsts[1]);
            Assert.Contains(0, firsts[0]);
        }

        [Fact]
        public void Firsts_test5()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/Firsts/firsts_test5.txt");

            Assert.Equal(2, firsts.Count);
            Assert.Equal(2, firsts[0].Count);
            Assert.Single(firsts[1]);
            Assert.Contains(0, firsts[0]);
            Assert.Contains(1, firsts[0]);
            Assert.Contains(1, firsts[1]);
        }

        [Fact]
        public void Firsts_test6()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/Firsts/firsts_test6.txt");

            Assert.Equal(4, firsts.Count);

            // Non terminal A
            Assert.Equal(8, firsts[0].Count);
            AssertContains(firsts[0], [0, 1, 2, 3, 4, 5, 6, 7]);

            // Non terminal B
            Assert.Equal(5, firsts[1].Count);
            AssertContains(firsts[1], [3, 4, 5, 6, 7]);

            // Non terminal C
            Assert.Single(firsts[2]);
            Assert.Contains(4, firsts[2]);

            // Non terminal D
            Assert.Equal(2, firsts[3].Count);
            AssertContains(firsts[3], [6, 7]);
        }

    }
}
