using Nt.Automaton.Actions;
using Nt.Automaton.States;
using Nt.Syntax.Automaton;

namespace Nt.Syntax.Programs
{
    internal abstract class ProgramMethod(ApplicationContext context) : IAction
    {
        public abstract void Perform();

        protected ApplicationContext Context { get; set; } = context;

        protected static void Transition()
        {
            Console.WriteLine("");
            Console.WriteLine("- - - - - - - - - - - - - - - - - - - -");
            Console.WriteLine("");
        }
    }
}
