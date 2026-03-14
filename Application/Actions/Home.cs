using Nt.Automaton.States;
using Nt.Automaton.Transitions;
using Nt.Syntax.Automaton;

namespace Nt.Syntax.Programs
{
    internal class Home(ApplicationContext context) : ProgramMethod(context)
    {
        public static State<string> GetState(ApplicationContext context)
        {
            var creationState = new State<string>(new GrammarCreation(context));
            var loaderState = new State<string>(new GrammarLoader(context));
            var analyseState = new State<string>(new CodeAnalysis(context));

            var newState = new State<string>(new Home(context));
            newState.AddTransition(new Transition<string>("1", creationState));
            newState.AddTransition(new Transition<string>("2", loaderState));
            newState.AddTransition(new Transition<string>("3", analyseState));

            return newState;
        }

        public override void Perform()
        {
            var config = SyntaxParserConfig.GetInstance();
            config.SetSymbolFactory(new LLAnalysing.Utils.SyntaxSymbolFactory());

            Console.WriteLine("Welcome to Nt Syntax Analyser!");
            Console.WriteLine("Select a program to execute");
            Console.WriteLine("1. Write new grammar");
            Console.WriteLine("2. Load existing grammar");
            Console.WriteLine("3. Analyse source code");
            Console.WriteLine("4. Exit");
        }
    }

}
