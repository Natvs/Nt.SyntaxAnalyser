using Nt.Syntax.LLParsing;
using Nt.Syntax;
using Nt.Syntax.Structures;

namespace Nt.SyntaxAnalyser.Application.Programs
{
    internal class GrammarCreation(Program program) : ProgramMethod(program)
    {

        public override void Execute()
        {
            Transition();

            string? text = null;
            string input = "";
            var generator = new SyntaxParser();
            Console.WriteLine("Enter text to generate grammar. Write a new line 'end' when done with the grammar.");
            while (text != "end")
            {
                text = Console.ReadLine();
                if (text != "end") input += text + "\n";
            }
            try
            {
                // Creates a new grammar from the user's input
                Grammar grammar = generator.ParseString(input);
                Console.WriteLine();
                Console.WriteLine("Grammar successfully created and parsed to LL1. Do you want to save it?");

                // Saves the grammar
                string? answer = Console.ReadLine();
                if (answer != null && (answer.ToLower().Equals("y") || answer.ToLower().Equals("yes")))
                {
                    Console.WriteLine("Enter file path to save the grammar:");
                    string? filePath = Console.ReadLine();
                    if (filePath != null)
                    {
                        File.WriteAllText(filePath, input);
                        Console.WriteLine("Grammar saved successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid file path. Grammar not saved.");
                    }
                }

                // Displays the parsed grammar
                Transition();
                Console.WriteLine("Created grammar:\n" + grammar.ToString());

                // Applies pre-analysis transformations to the grammar
                LL1Parser.Parse(grammar);
                Transition();
                Console.WriteLine("Parsed grammar:\n" + grammar.ToString());

                // Starts code analysis
                var codeAnalysis = new CodeAnalysis(Program, grammar);
                codeAnalysis.Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError while parsing grammar:\n" + ex.Message);
                Transition();
            }
        }
    }

}
