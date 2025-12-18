using Nt.Syntax.LLParsing;
using Nt.Syntax;
using Nt.Syntax.Structures;
using System.Reflection.Emit;

namespace Nt.SyntaxAnalyser.Application.Programs
{
    internal class GrammarLoader(Program program) : ProgramMethod(program)
    {

        public override void Execute()
        {
            Transition();

            var files = Directory.EnumerateFiles(".", "*.txt", SearchOption.AllDirectories);
            if (files.Count() > 0)
            {
                DisplayCurrentFiles(files);   
            }
            else
            {
                LoadFromPath();
            }
        }

        private void DisplayCurrentFiles(IEnumerable<string> files)
        {
            Console.WriteLine("Some files with .txt extension have been found in the current directory:");
            for (int i = 0; i < files.Count(); i++)
            {
                Console.WriteLine($"{i + 1}. {files.ElementAt(i)}");
            }
            Console.WriteLine($"{files.Count() + 1} select an other path");
            string? answer_string = Console.ReadLine();
            if (answer_string == null)
            {
                Console.WriteLine("Invalid input. Directing back to home.");
                return;
            }
            int answer = int.Parse(answer_string);
            if (answer > 0 && answer <= files.Count())
            {
                string filePath = files.ElementAt(new Index(answer - 1));
                try
                {
                    string fileContent = File.ReadAllText(filePath);
                    var generator = new SyntaxParser();
                    Grammar grammar = generator.ParseString(fileContent);
                    LL1Parser.Parse(grammar);
                    Console.WriteLine("\nLoaded and parsed grammar:\n" + grammar.ToString());
                    Program.SetNewMethod(new CodeAnalysis(Program, grammar));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nError while loading/parsing grammar:\n" + ex.Message);
                }
            }
            else
            {
                LoadFromPath();
            }
        }

        private void LoadFromPath()
        {
            var generator = new SyntaxParser();
            Console.WriteLine("Enter the full path to the grammar file:");
            string? customPath = Console.ReadLine();
            if (customPath != null)
            {
                try
                {
                    string fileContent = System.IO.File.ReadAllText(customPath);
                    Grammar grammar = generator.ParseString(fileContent);
                    LL1Parser.Parse(grammar);
                    Console.WriteLine("\nLoaded and parsed grammar:\n" + grammar.ToString());
                    Program.SetNewMethod(new CodeAnalysis(Program, grammar));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nError while loading/parsing grammar:\n" + ex.Message);
                }
            }
        }
    }


}
