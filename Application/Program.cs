// See https://aka.ms/new-console-template for more information

using Nt.Syntax;
using Nt.Syntax.Structures;
using Nt.Syntax.LLParsing;
using Nt.Syntax.LLAnalysing;
using Nt.Parsing;
using Nt.Parsing.Structures;
using Nt.SyntaxAnalyser.Application.Programs;

internal class Program
{
    private ProgramMethod? currentMethod;

    public Program()
    {
        currentMethod = new Home(this);
        currentMethod.Execute();
    }

    public void SetNewMethod(ProgramMethod newmethod) { currentMethod = newmethod; currentMethod.Execute(); }

    private static void Main(string[] args)
    {
        var program = new Program();
    }

}

