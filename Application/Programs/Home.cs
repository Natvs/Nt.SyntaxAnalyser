namespace Nt.Syntax.Programs
{
    internal class Home(Program program) : ProgramMethod(program)
    {

        public override void Execute()
        {
            var config = SyntaxParserConfig.GetInstance();
            config.SetSymbolFactory(new LLAnalysing.Utils.SyntaxSymbolFactory());

            Transition();

            var answer = "0";
            while (answer != "3")
            {
                Console.WriteLine("Welcome to Nt Syntax Analyser!");
                Console.WriteLine("Select a way of filling grammar");
                Console.WriteLine("1. Write new grammar");
                Console.WriteLine("2. Load existing grammar");
                Console.WriteLine("3. Exit");

                answer = Console.ReadLine();
                if (answer == "1")
                {
                    var grammarParsing = new GrammarCreation(Program);
                    grammarParsing.Execute();
                }
                else if (answer == "2")
                {
                    var codeAnalysis = new GrammarLoader(Program);
                    codeAnalysis.Execute();
                }
            }
            
        }
    }

}
