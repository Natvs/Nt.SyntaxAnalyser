using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nt.Syntax.LLParsing;
using Nt.Syntax.LLAnalysing;
using Nt.Syntax;

namespace Nt.SyntaxAnalyser.Tests.Domain.LLAnalysing
{
    public class EmptyAnalyserTest
    {
        private List<int> GetEmptyGenerators(string filename)
        {
            var parser = new SyntaxParser();
            var grammar = parser.ParseFile(filename);

            var emptygenerators = EmptyAnalyser.Analyse(grammar);

            return [.. emptygenerators];
        }

        [Fact]
        public void EmptyGenerator_test1()
        {
            var emptygenerators = GetEmptyGenerators("../../../Resources/Analyse/Empty/empty_test1.txt");

            Assert.Empty(emptygenerators);
        }

        [Fact]
        public void EmptyGenerator_test2()
        {
            var emptygenerators = GetEmptyGenerators("../../../Resources/Analyse/Empty/empty_test2.txt");

            Assert.Single(emptygenerators);
            Assert.Contains(0, emptygenerators);
        }

        [Fact]
        public void EmptyGenerator_test3()
        {
            var emptygenerators = GetEmptyGenerators("../../../Resources/Analyse/Empty/empty_test3.txt");

            Assert.Single(emptygenerators);
            Assert.Contains(1, emptygenerators);
        }

        [Fact]
        public void EmptyGenertor_test4()
        {
            var emptygenerators = GetEmptyGenerators("../../../Resources/Analyse/Empty/empty_test4.txt");

            Assert.Equal(2, emptygenerators.Count);
            Assert.Contains(0, emptygenerators);
            Assert.Contains(1, emptygenerators);
        }

        [Fact]
        public void EmptyGenertor_test5()
        {
            var emptygenerators = GetEmptyGenerators("../../../Resources/Analyse/Empty/empty_test5.txt");

            Assert.Single(emptygenerators);
            Assert.Contains(1, emptygenerators);
        }

    }
}
