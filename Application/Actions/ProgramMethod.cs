using Nt.Automaton.Actions;
using Nt.Automaton.States;
using Nt.Syntax.Automaton;

namespace Nt.Syntax.Programs
{
    internal abstract class ProgramMethod : IAction
    {
        public abstract void Perform();

        protected ApplicationContext Context { get; set; }
        public ProgramMethod(ApplicationContext context) => this.Context = context;


        protected static void Transition()
        {
            Console.WriteLine("");
            Console.WriteLine("- - - - - - - - - - - - - - - - - - - -");
            Console.WriteLine("");
        }
    }
}
