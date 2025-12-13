// See https://aka.ms/new-console-template for more information

using Nt.Syntax;
using Nt.Syntax.Structures;
using Nt.Syntax.LLParsing;
using Nt.Syntax.LLAnalysing;
using Nt.Parsing;
using Nt.Parsing.Structures;

internal class Program
{
    private static void Main(string[] args)
    {
        SourceCodeAnalysis();
    }

    private static void SourceCodeAnalysis()
    {
        var syntaxparser = new SyntaxParser();
        var grammar = syntaxparser.ParseFile("../../../Resources/TestGrammar.txt");
        var checkpoints = new SymbolsList([";"]);
        LL1Parser.Parse(grammar);
        var analyseSet = LL1AnalyseSet.Get(grammar);

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
            var analyseResult = LL1Analyser.Analyse(analyseSet, parserResult, checkpoints);
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
            Console.WriteLine($"Syntax error at line {syntaxException.Data0.Line}: unexpected symbol { parserResult.Symbols[syntaxException.Data0.TokenIndex].Name }.");
        }
        foreach (var regexException in analyseResult.RegexExceptions)
        {
            error = true;
            Console.WriteLine($"Syntax error at line {regexException.Data0.Line}: string {parserResult.Symbols[regexException.Data0.TokenIndex].Name } does not match the pattern {regexException.Data1}");
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

    private static void GrammarPretreatment()
    {
        bool continue_parsing = true;

        while (continue_parsing)
        {
            string? text = null;
            string input = "";
            var generator = new SyntaxParser();
            Console.WriteLine("Enter text to generate grammar");
            while (text != "end")
            {
                text = Console.ReadLine();
                if (text != "end") input += text + "\n";
            }
            try
            {
                Grammar grammar = generator.ParseString(input);
                LL1Parser.Parse(grammar);
                Console.WriteLine("\nParsed grammar:\n" + grammar.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError while parsing grammar:\n" + ex.Message);
            }

            continue_parsing = false;
            Console.WriteLine();
            Console.WriteLine("Continue parsing new grammar?");
            string? answer = Console.ReadLine();
            if (answer == null) return;
            if (answer.ToLower().Equals("y") || answer.ToLower().Equals("yes")) continue_parsing = true;
        }
    }
}

