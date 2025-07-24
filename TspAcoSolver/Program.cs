using System;

namespace TspAcoSolver
{
    class Program
    {
        static void Main()
        {
            Cli cli = new Cli();

            cli.Run();
            // Config conf = new Config();
            // Console.WriteLine(conf.Read("test").EvaporationCoef);
        }
    }
}