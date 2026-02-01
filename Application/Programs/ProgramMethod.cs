namespace Nt.Syntax.Programs
{
    internal abstract class ProgramMethod
    {

        protected Program Program { get; set; }

        public ProgramMethod(Program program) => this.Program = program;

        abstract public void Execute();

        protected static void Transition()
        {
            Console.WriteLine("");
            Console.WriteLine("- - - - - - - - - - - - - - - - - - - -");
            Console.WriteLine("");
        }

    }
}
