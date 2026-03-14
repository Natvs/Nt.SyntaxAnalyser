// See https://aka.ms/new-console-template for more information

using Nt.Syntax.Automaton;

internal class Program
{
    private static void Main(string[] args)
    {
        var context = new ApplicationContext();

        // Iterate until the user escapes from the initial state
        while (!context.Automaton.IsEmpty())
        {
            var answer = Console.ReadLine();
            if (answer == null) continue;
            context.Automaton.Read(new ApplicationToken(answer));
        }
    }

}