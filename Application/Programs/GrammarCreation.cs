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
                Grammar grammar = generator.ParseString(input);
                LL1Parser.Parse(grammar);

                Console.WriteLine();
                Console.WriteLine("Grammar successfully created and parsed to LL1. Do you want to save it?");
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
                Program.SetNewMethod(new CodeAnalysis(Program, grammar));
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError while parsing grammar:\n" + ex.Message);
            }
        }
    }

}
