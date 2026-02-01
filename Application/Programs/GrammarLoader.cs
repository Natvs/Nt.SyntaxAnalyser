using Nt.Syntax.LLParsing;
using Nt.Syntax.Structures;

namespace Nt.Syntax.Programs
{

    internal class GrammarLoader(Program program) : ProgramMethod(program)
    {
        private static List<string> paths = [".", "./Resources", "../../../Resources"];

        public override void Execute()
        {
            Transition();

            var files = new List<string>();
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    files.AddRange(Directory.EnumerateFiles(path, "*.txt", SearchOption.AllDirectories));
                }
            }
            try
            {
                if (files.Any()) LoadExistingFile(files);
                else LoadFromPath();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError while loading/parsing grammar:\n" + ex.Message);
                Transition();
            }
        }

        private void LoadExistingFile(IEnumerable<string> files)
        {
            // Displays files in the current directory
            Console.WriteLine("Some files with .txt extension have been found in the current directory:");
            for (int i = 0; i < files.Count(); i++)
            {
                Console.WriteLine($"{i + 1}. {Path.GetFileName(files.ElementAt(i))}");
            }
            Console.WriteLine($"{files.Count() + 1}. Select an other path");

            // Loads selected file
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
                string fileContent = File.ReadAllText(filePath);
                var generator = new SyntaxParser();
                var grammar = generator.ParseString(fileContent);

                Transition();
                Console.WriteLine("Loaded grammar:\n" + grammar.ToString());
                LL1Parser.Parse(grammar);

                Transition();
                Console.WriteLine("Parsed grammar:\n" + grammar.ToString());

                // Starts code analysis
                var codeAnalysis = new CodeAnalysis(Program, grammar);
                codeAnalysis.Execute();
            }
            else
            {
                var grammar = LoadFromPath();

                // Starts code analysis
                var codeAnalysis = new CodeAnalysis(Program, grammar);
                codeAnalysis.Execute();
            }
        }

        private static Grammar LoadFromPath()
        {
            var generator = new SyntaxParser();

            Console.WriteLine("Enter the full path to the grammar file:");
            string? customPath = Console.ReadLine();
            string fileContent = File.ReadAllText(customPath);
            Grammar grammar = generator.ParseString(fileContent);

            // Displays loaded grammar
            Transition();
            Console.WriteLine("Loaded grammar:\n" + grammar.ToString());

            // Parses the grammar
            LL1Parser.Parse(grammar);
            Transition();
            Console.WriteLine("\nLoaded and parsed grammar:\n" + grammar.ToString());

            return grammar;
        }
    }


}
