using Nt.Automaton.States;
using Nt.Automaton.Transitions;
using Nt.Syntax.Automaton;
using Nt.Syntax.LLParsing;
using Nt.Syntax.Structures;

namespace Nt.Syntax.Programs
{
    internal class GrammarCreation(ApplicationContext context) : ProgramMethod(context)
    {
        public override void Perform()
        {
            string? text = null;
            string input = "";
            //SyntaxParserConfig.GetInstance().SymbolFactory = new Syntax
            var generator = new SyntaxParser();
            Console.WriteLine("Enter text to generate grammar. Write a new line 'end' when done with the grammar.");
            // Ask the user to fill the grammar
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while creating the grammar: " + ex.Message);
            }
            finally
            {
                Transition();
                Context.Automaton.Pop(true);
            }
        }
    }

}
