using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nt.SyntaxAnalyser.Application.Programs
{
    internal abstract class ProgramMethod
    {

        protected Program Program { get; set; }

        public ProgramMethod(Program program) => this.Program = program;

        abstract public void Execute();

        protected void Transition()
        {
            Console.WriteLine("");
            Console.WriteLine("- - - - - - - - - - - - - - - - - - - -");
            Console.WriteLine("");
        }

    }
}
