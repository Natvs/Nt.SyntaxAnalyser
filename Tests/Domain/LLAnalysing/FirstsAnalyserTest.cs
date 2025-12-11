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

        [Fact]
        public void Firsts_test1()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/firsts_test1.txt");

            Assert.Single(firsts);
            Assert.Single(firsts[0]);
            Assert.Contains(0, firsts[0]);
        }

        [Fact]
        public void Firsts_test2()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/firsts_test2.txt");

            Assert.Single(firsts);
            Assert.Single(firsts[0]);
            Assert.Contains(0, firsts[0]);
        }

        [Fact]
        public void Firsts_test3()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/firsts_test3.txt");

            Assert.Equal(2, firsts.Count);
            Assert.Single(firsts[0]);
            Assert.Single(firsts[1]);
            Assert.Contains(0, firsts[0]);
            Assert.Contains(0, firsts[1]);
        }

        [Fact]
        public void Firsts_test4()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/firsts_test4.txt");

            Assert.Equal(2, firsts.Count);
            Assert.Single(firsts[0]);
            Assert.Empty(firsts[1]);
            Assert.Contains(0, firsts[0]);
        }

        [Fact]
        public void Firsts_test5()
        {
            var firsts = GetFirsts("../../../Resources/Analyse/firsts_test5.txt");

            Assert.Equal(2, firsts.Count);
            Assert.Equal(2, firsts[0].Count);
            Assert.Single(firsts[1]);
            Assert.Contains(0, firsts[0]);
            Assert.Contains(1, firsts[0]);
            Assert.Contains(1, firsts[1]);
        }

    }
}
