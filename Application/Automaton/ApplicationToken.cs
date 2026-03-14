using Nt.Automaton.Tokens;

namespace Nt.Syntax.Automaton
{
    internal class ApplicationToken(string name): IAutomatonToken<string>
    {
        string IAutomatonToken<string>.Value => name;
    }

}
