using Nt.Syntax.Automaton;
using Nt.Syntax.LLAnalysing;

namespace Nt.Syntax.Actions
{

    internal class CodeAnalysis(ApplicationContext context) : ProgramMethod(context)
    {
        public override void Perform()
        {
            if (Context.Grammar == null)
            {
                Console.WriteLine("No grammar loaded. Please load a grammar before performing code analysis.");
                Transition();
                Context.Automaton.Pop(true);
                return;
            }
            try
            {
                var reader = SourceReader.CreateFromGrammar(Context.Grammar, [' ', '\n'], ["{", "}", ";"], [';', '}']);

                bool continue_analysis = true;
                while (continue_analysis)
                {
                    string? text = null;
                    string input = "";
                    Console.WriteLine("Enter text to analyze");
                    while (text != "end")
                    {
                        text = Console.ReadLine();
                        if (text != "end") input += text + "\n";
                    }
                    var analyseResult = reader.AnalyseString(input);
                    if (Context.Grammar == null) return;
                    PrintAnalyseResult(analyseResult);

                    continue_analysis = false;
                    Console.WriteLine();
                    Console.WriteLine("Continue analyzing new text?");
                    string? answer = Console.ReadLine();
                    if (answer == null) return;
                    if (answer.ToLower().Equals("y") || answer.ToLower().Equals("yes")) continue_analysis = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred during code analysis: " + ex.Message);
            }
            finally
            {
                Transition();
                Context.Automaton.Pop(true);
            }
        }

        private static void PrintAnalyseResult(LL1Analyser.AnalyseResult analyseResult)
        {
            var error = false;
            foreach (var syntaxException in analyseResult.SyntaxExceptions)
            {
                error = true;
                Console.WriteLine($"Syntax error at line {syntaxException.Data0.Line}: unexpected symbol {syntaxException.Data0.Symbol.Name}.");
            }
            if (analyseResult.EndOfFileStatus == LL1Analyser.EndOfFileStatus.Failed)
            {
                error = true;
                Console.WriteLine("Syntax error: unexpected end of file.");
            }
            if (!error)
            {
                Console.WriteLine("Content is valid");
            }
        }
    }

}
