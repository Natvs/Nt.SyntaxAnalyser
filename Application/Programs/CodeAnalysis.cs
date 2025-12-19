using Nt.Parsing.Structures;
using Nt.Parsing;
using Nt.Syntax.LLAnalysing;
using Nt.Syntax.Structures;

namespace Nt.SyntaxAnalyser.Application.Programs
{

    internal class CodeAnalysis(Program program, Grammar grammar) : ProgramMethod(program)
    {
        public override void Execute()
        {
            var checkpoints = new SymbolsList([";"]);
            var analyseSet = LL1AnalyseSet.Get(grammar, new SymbolsList([";"]));

            bool continue_analysis = true;
            while (continue_analysis)
            {
                string? text = null;
                string input = "";
                var parser = new Parser([' ', '\n'], ["{", "}", ";", "\""]);
                Console.WriteLine("Enter text to analyze");
                while (text != "end")
                {
                    text = Console.ReadLine();
                    if (text != "end") input += text + "\n";
                }
                var parserResult = parser.Parse(input);
                var analyseResult = LL1Analyser.Analyse(analyseSet, parserResult);
                PrintAnalyseResult(grammar, parserResult, analyseResult);

                continue_analysis = false;
                Console.WriteLine();
                Console.WriteLine("Continue analyzing new text?");
                string? answer = Console.ReadLine();
                if (answer == null) return;
                if (answer.ToLower().Equals("y") || answer.ToLower().Equals("yes")) continue_analysis = true;
            }
        }

        private static void PrintAnalyseResult(Grammar grammar, ParserResult parserResult, LL1Analyser.AnalyseResult analyseResult)
        {
            var error = false;
            foreach (var syntaxException in analyseResult.SyntaxExceptions)
            {
                error = true;
                Console.WriteLine($"Syntax error at line {syntaxException.Data0.Line}: unexpected symbol {parserResult.Symbols[syntaxException.Data0.TokenIndex].Name}.");
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
