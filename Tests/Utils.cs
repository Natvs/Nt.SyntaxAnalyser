using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nt.Tests.Syntax
{
    internal class Utils
    {
        internal static void AssertContains<T>(IEnumerable<T> collection, IEnumerable<T> expected)
        {
            Assert.Equal(expected.Count(), collection.Count());
            foreach (var item in expected)
            {
                Assert.Contains(item, collection);
            }
        }
        internal static void AssertEmpty(Dictionary<string, IEnumerable<string>> dict, string key)
        {
            Assert.False(dict.ContainsKey(key));
        }
    }
}
