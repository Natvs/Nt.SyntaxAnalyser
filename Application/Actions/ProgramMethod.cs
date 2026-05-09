using Nt.Automaton.Actions;
using Nt.Syntax.Automaton;

namespace Nt.Syntax.Actions
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
