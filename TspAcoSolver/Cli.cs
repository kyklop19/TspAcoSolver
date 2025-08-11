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
                        ITour sol = GetSolution();
                        Console.WriteLine(sol);
                        Console.WriteLine($"Length: {sol.Length}");

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
            if (path.EndsWith(".csv"))
            {
                switch (problemType.ToLower())
                {
                    case "tsp":
                        CsvParser csvParser = new();
                        Problem = csvParser.Parse(path);
                        break;
                }
            }
            else if (path.EndsWith(".tsp"))
            {
                TspParser tspParser = new();
                Problem = tspParser.Parse(path);
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