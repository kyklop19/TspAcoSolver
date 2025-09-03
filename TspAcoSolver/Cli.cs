using System.CommandLine;
using System.CommandLine.Help;
using System.Diagnostics;

namespace TspAcoSolver
{
    public enum ProblemType
    {
        Tsp,
        Euc2D,
    }

    /// <summary>
    /// <c>Cli</c> represents user interface for reading problems from files, set solving parameters and running solvers.
    /// </summary>
    public class Cli
    {
        SolverBase _solver;
        IProblem _problem;
        SolvingParams _sParams;

        Dictionary<string, Command> _commands;
        bool _quit = false;
        public Cli()
        {
            Config config = new();
            _sParams = config.Read(@"default_config.yaml"); // ../../../../
            // Console.WriteLine($"{Path.GetFullPath()}");
            Console.WriteLine($"{_sParams.ColonyParams.AntCount}");

            CreateCommands();
        }
        void CreateCommands()
        {
            HelpOption helpOption = new();

            const string quitCmdName = "quit";

            Command quitCommand = new(quitCmdName, "Quit the program");
            quitCommand.Options.Add(helpOption);
            quitCommand.SetAction(parseResult =>
            {
                Quit();
            });

            const string solveCmdName = "solve";

            Command solveCommand = new(solveCmdName, "Solve problem");
            solveCommand.Options.Add(helpOption);
            solveCommand.SetAction(parseResult =>
            {
                ITour sol = GetSolution();
                Console.WriteLine(sol);
                Console.WriteLine($"Length: {sol.Length}");
            });

            const string confCmdName = "conf";

            Argument<string> pathArg = new("path");
            Command confCommand = new(confCmdName, "Set config");

            confCommand.Arguments.Add(pathArg);

            confCommand.Options.Add(helpOption);

            confCommand.SetAction(parseResult =>
            {
                SetConfig(parseResult.GetValue(pathArg));
            });

            const string probCmdName = "prob";

            Argument<string> problemPathArg = new("path");
            Option<ProblemType?> problemTypeOpt = new("--type", ["-t"]);
            Command probCommand = new(probCmdName, "Set problem");

            probCommand.Arguments.Add(problemPathArg);

            probCommand.Options.Add(helpOption);
            probCommand.Options.Add(problemTypeOpt);

            probCommand.SetAction(parseResult =>
            {
                SetProblem(parseResult.GetValue(problemPathArg), parseResult.GetValue(problemTypeOpt));
            });

            _commands = new()
            {
                {quitCmdName, quitCommand},
                {solveCmdName, solveCommand},
                {confCmdName, confCommand},
                {probCmdName, probCommand},
            };
        }

        public void Run()
        {
            while (!_quit)
            {
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;
                string[] cmd = input.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (_commands.TryGetValue(cmd[0], out Command command))
                {
                    ParseResult parseResult = command.Parse(cmd[1..]);
                    parseResult.Invoke();
                }
                else
                {
                    Console.WriteLine($"Invalid command: {cmd[0]}");
                }
            }
        }

        void SetProblem(string path, ProblemType? problemType)
        {
            if (path.EndsWith(".csv"))
            {
                switch (problemType)
                {
                    case null:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Csv file needs specified problem type. Use \"--type\" option.");
                        Console.ResetColor();
                        break;
                    case ProblemType.Tsp:
                        CsvParser csvParser = new();
                        _problem = csvParser.Parse(path);
                        break;
                }
            }
            else if (path.EndsWith(".tsp"))
            {
                TspParser tspParser = new();
                _problem = tspParser.Parse(path);
            }
        }

        void SetConfig(string path)
        {
            Config config = new();
            _sParams = config.Read(path);
        }

        ITour GetSolution()
        {
            Stopwatch stopWatch = new();
            stopWatch.Start();

            switch (_sParams.Algorithm)
            {
                case "AS":
                    _solver = new AsSolver(_problem, _sParams);
                    break;
                case "ACS":
                    Console.WriteLine($"Using ACS Solver");
                    _solver = new AcsSolver(_problem, _sParams);
                    break;
            }
            ITour res = _solver.Solve();

            stopWatch.Stop();

            Console.WriteLine($"Time elapsed: {stopWatch.Elapsed} | Iteration count: {_solver.CurrIterationCount}");
            return res;
        }
        void Quit()
        {
            _quit = true;
        }
    }
}