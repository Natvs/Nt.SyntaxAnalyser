// See https://aka.ms/new-console-template for more information

using Nt.Syntax.Programs;

internal class Program
{

    public Program()
    {
        var currentMethod = new Home(this);
        currentMethod.Execute();
    }

    public static void SetNewMethod(ProgramMethod newmethod) { newmethod.Execute(); }

    private static void Main(string[] args)
    {
        var program = new Program();
    }

}