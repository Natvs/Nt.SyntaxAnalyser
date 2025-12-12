using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nt.Syntax.Tests
{
    internal class Utils
    {
        static internal void AssertContains<T>(ICollection<T> collection, ICollection<T> values)
        {
            foreach (var value in values)
            {
                Assert.Contains(value, collection);
            }
        }
    }
}
