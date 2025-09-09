using System.CommandLine;
using System.CommandLine.Help;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ScottPlot.Plottables;

namespace TspAcoSolver
{
    public enum ProblemType
    {
        Explicit,
        Euclid,
    }

    /// <summary>
    /// <c>Cli</c> represents user interface for reading problems from files, set solving parameters and running solvers.
    /// </summary>
    public class Cli
    {
        ISolver _solver;
        IProblem _problem;
        SolvingParams _sParams;

        Dictionary<string, Command> _commands;
        bool _quit = false;

        bool _enableHeatmap = false;
        public Cli()
        {
            Config config = new();
            _sParams = config.Read(@"default_config.yaml"); // ../../../../
            // Console.WriteLine($"{Path.GetFullPath()}");
            Console.WriteLine($"{_sParams}");

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
            Option<bool> enableHeatmapOpt = new("--heatmap", ["-hm"]);
            solveCommand.Options.Add(enableHeatmapOpt);
            solveCommand.SetAction(parseResult =>
            {
                _enableHeatmap = parseResult.GetValue(enableHeatmapOpt);
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
                    case ProblemType.Explicit:
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
            _sParams = config.Read(@"default_config.yaml");
            _sParams.Overwrite(config.Read(path));
            Console.WriteLine($"{_sParams}");

        }

        ITour GetSolution()
        {
            Stopwatch stopWatch = new();
            stopWatch.Start();

            ServiceCollection serviceCollection = new();
            serviceCollection.Configure<SolvingParams>(sParams =>
            {
                sParams.Algorithm = _sParams.Algorithm;
                sParams.PheromoneParams = _sParams.PheromoneParams;
                sParams.TerminationParams = _sParams.TerminationParams;
                sParams.ColonyParams = _sParams.ColonyParams;
            });
            serviceCollection.Configure<ColonyParams>(cParams =>
            {
                cParams.AntCount = _sParams.ColonyParams.AntCount;
                cParams.ThreadCount = _sParams.ColonyParams.ThreadCount;
                cParams.TrailLevelFactor = _sParams.ColonyParams.TrailLevelFactor;
                cParams.AttractivenessFactor = _sParams.ColonyParams.AttractivenessFactor;
                cParams.ExploProportionConst = _sParams.ColonyParams.ExploProportionConst;
            });
            serviceCollection.Configure<TerminationParams>(tParams =>
            {
                tParams.TerminationRule = _sParams.TerminationParams.TerminationRule;
                tParams.IterationCount = _sParams.TerminationParams.IterationCount;
                tParams.CeilingPercentage = _sParams.TerminationParams.CeilingPercentage;
                tParams.InRowTerminationCount = _sParams.TerminationParams.InRowTerminationCount;
            });
            serviceCollection.Configure<ReinitializationParams>(rParams =>
            {
                rParams.ReinitializationRule = _sParams.ReinitializationParams.ReinitializationRule;
                rParams.IterIncrement = _sParams.ReinitializationParams.IterIncrement;
                rParams.StagnationCeiling = _sParams.ReinitializationParams.StagnationCeiling;
            });

            serviceCollection.AddSingleton<IRandom, RandomGen>();
            serviceCollection.AddTransient<ITerminationChecker, TerminationChecker>();
            serviceCollection.AddTransient<IReinitializer, Reinitializer>();

            if (_enableHeatmap)
            {
                serviceCollection.AddTransient<IPheromoneGraphVisualiser, HeatmapPheromoneVisualiser>();
            }
            else
            {
                serviceCollection.AddTransient<IPheromoneGraphVisualiser, NullPheromoneVisualiser>();
            }

            switch (_sParams.Algorithm)
            {
                case "AS":
                    serviceCollection.AddTransient<IAntFactory<IAnt>, AntFactory<AsAnt>>();
                    serviceCollection.AddTransient<IColony, AsColony>();
                    serviceCollection.AddTransient<ISolver, AsSolver>();
                    break;
                case "ACS":
                    Console.WriteLine($"Using ACS Solver");
                    serviceCollection.AddTransient<IAntFactory<IAnt>, AntFactory<AcsAnt>>();
                    serviceCollection.AddTransient<IColony, AcsColony>();
                    serviceCollection.AddTransient<ISolver, AcsSolver>();
                    break;
            }
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            _solver = serviceProvider.GetService<ISolver>();
            ITour res = _solver.Solve(_problem);

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