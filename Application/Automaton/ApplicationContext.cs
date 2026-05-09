using Nt.Automaton.Automatons;
using Nt.Syntax.Actions;
using Nt.Syntax.Structures;

namespace Nt.Syntax.Automaton
{
    internal class ApplicationContext
    {
        public ApplicationContext()
        {
            SyntaxParserConfig.GetInstance().SetSymbolFactory(new LLAnalysing.Utils.SyntaxSymbolFactory());
            Automaton.Push(Home.GetState(this), true);
        }

        public Grammar? Grammar { get; set; } = null;

        public StackAutomaton<string> Automaton { get; private set; } = new StackAutomaton<string>().SetAutoPerformAction();

    }
}
