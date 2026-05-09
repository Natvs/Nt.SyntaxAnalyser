using Nt.Syntax.Automaton;
using Nt.Syntax.LLParsing;
using Nt.Syntax.Structures;

namespace Nt.Syntax.Actions
{

    internal class GrammarLoader(ApplicationContext context) : ProgramMethod(context)
    {
        private static List<string> paths = [".", "../../../Resources"];

        public override void Perform()
        {
            var files = new List<string>();
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    files.AddRange(Directory.EnumerateFiles(path, "*.txt", SearchOption.TopDirectoryOnly));
                }
            }
            try
            {
                if (files.Count > 0) LoadFile(files);
                else LoadFromPath();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError while loading/parsing grammar: { ex.Message }\nDirecting back to home\n");
            }
            finally
            {
                Transition();
                Context.Automaton.Pop(true);
            }
        }

        /// <summary>
        /// Prompts the user to select a file from the specified collection or to provide a custom file path, then loads the grammar from the selected file.
        /// </summary>
        /// <remarks>If the user selects a file from the provided list, the grammar is loaded from that file. 
        /// If the user chooses to specify another path, the method prompts for a custom file path and loads the grammar from the specified location.</remarks>
        /// <param name="files">A collection of file paths to display as selectable options. Each path should refer to a valid file.</param>
        private void LoadFile(IEnumerable<string> files)
        {
            // Display files in the current directory
            Console.WriteLine("Some files with .txt extension have been found in the current directory:");
            for (int i = 0; i < files.Count(); i++)
            {
                Console.WriteLine($"{i + 1}. {Path.GetFileName(files.ElementAt(i))}");
            }
            Console.WriteLine($"{files.Count() + 1}. Select an other path");

            // Load selected file
            string? answer_string = Console.ReadLine();
            if (answer_string == null)
            {
                Console.WriteLine("Invalid input. Directing back to home.");
                return;
            }
            int answer = int.Parse(answer_string);

            if (answer > 0 && answer <= files.Count())
            {
                // If a file was selected, load the grammar from it
                string filePath = files.ElementAt(new Index(answer - 1));
                Context.Grammar = LoadFromPath(filePath);
            }
            else
            {
                // If the "Select an other path" option was selected, request a path and load the grammar from it
                Context.Grammar = LoadFromPath();
            }
        }

        /// <summary>
        /// Loads a grammar definition from the specified file path and parses it into a Grammar object.
        /// </summary>
        /// <remarks>The method reads the entire contents of the file at the specified path and parses it as a grammar.</remarks>
        /// <param name="filePath">The path to the file containing the grammar definition to load. Cannot be null or empty.</param>
        /// <returns>A Grammar object representing the parsed grammar from the specified file.</returns>
        private static Grammar LoadFromPath(string filePath)
        {
            var generator = new SyntaxParser();

            // Generate grammar from file content
            string fileContent = File.ReadAllText(filePath);
            var grammar = generator.ParseString(fileContent);

            // Display loaded grammar
            Transition();
            Console.WriteLine("Loaded grammar:\n" + grammar.ToString());

            // Display parsed grammar
            LL1Parser.Parse(grammar);
            Transition();
            Console.WriteLine("Parsed grammar:\n" + grammar.ToString());

            return grammar;
        }

        /// <summary>
        /// Prompts the user to enter the full path to a grammar file and loads the grammar from the specified file.
        /// </summary>
        /// <remarks>The method reads input from the console to obtain the file path.</remarks>
        /// <returns>A <see cref="Grammar"/> Grammar loaded from the file at the specified path.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the file specified by the user does not exist.</exception>
        private static Grammar LoadFromPath()
        {
            // Request file path
            Console.WriteLine("Enter the full path to the grammar file:");
            string? customPath = Console.ReadLine();
            if (!File.Exists(customPath)) throw new FileNotFoundException($"The specified file { customPath } does not exist.");

            var grammar = LoadFromPath(customPath);
            return grammar;
        }
    }


}
