namespace TspAcoSolver
{
    public class Cli
    {
        Solver Solver { get; set; }
        IProblem Problem { get; set; }
        SolvingParams sParams { get; set; }

        public Cli()
        {
            Config config = new();
            sParams = config.Read(@"../../../../data/default_config.yaml");
            // Console.WriteLine($"{Path.GetFullPath()}");
            Console.WriteLine($"{sParams.AntCount}");

        }

        public void Run()
        {
            bool quit = false;
            while (!quit)
            {
                string[] cmd = Console.ReadLine().Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                switch (cmd[0])
                {
                    case "prob":
                        SetProblem(cmd[1], cmd[2]);
                        break;
                    case "conf":
                        SetConfig(cmd[1]);
                        break;
                    case "solve":
                        Console.WriteLine(GetSolution());
                        break;
                    case "quit":
                        quit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid command");
                        break;
                }
            }
        }

        void SetProblem(string problemType, string path)
        {
            switch (problemType.ToLower())
            {
                case "tsp":
                    Problem = new TravelingSalesmanProblem();
                    Problem.Read(path);
                    break;
            }
        }

        void SetConfig(string path)
        {
            Config config = new();
            sParams = config.Read(path);
        }

        ITour GetSolution()
        {
            Solver = new(Problem, sParams);
            return Solver.Solve();
        }
    }
}