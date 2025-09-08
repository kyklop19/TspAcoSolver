using System;
using System.CommandLine;
using TspAcoSolver;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public record ProbTest(string name, double optLen);

public record Result(string name, int iterCount, double solLen, double err);

class Program
{
    static void Main()
    {
        ProbTest[] problems = new ProbTest[]
        {
            new("a280", 2579),
            new("berlin52", 7542),
            new("bier127", 118282),
            new("ch130", 6110),
            new("ch150", 6528),
            new("d198", 15780),
            new("d493", 35002),
            new("d657", 48912),
            new("d1291", 50801),
            new("d1655", 62128),
        };

        TspParser parser = new();
        SolvingParams solvingParams = new Config().Read(@"default_config.yaml");

        int toProcess = problems.Length;

        List<Result> results = new();
        Lock threadLock = new();
        using (ManualResetEvent resetEvent = new(false))
        {
            foreach (ProbTest problem in problems)
            {
                IProblem prob = parser.Parse(@"../../../../data/problems/benchmark/" + problem.name + ".tsp");
                ThreadPool.QueueUserWorkItem(
                    new WaitCallback((object x) =>
                    {
                        ServiceCollection serviceCollection = new();
                        serviceCollection.Configure<SolvingParams>(sParams =>
                        {
                            sParams.Algorithm = solvingParams.Algorithm;
                            sParams.PheromoneParams = solvingParams.PheromoneParams;
                            sParams.TerminationParams = solvingParams.TerminationParams;
                            sParams.ColonyParams = solvingParams.ColonyParams;
                        });
                        serviceCollection.Configure<ColonyParams>(cParams =>
                        {
                            cParams.AntCount = solvingParams.ColonyParams.AntCount;
                            cParams.ThreadCount = solvingParams.ColonyParams.ThreadCount;
                            cParams.TrailLevelFactor = solvingParams.ColonyParams.TrailLevelFactor;
                            cParams.AttractivenessFactor = solvingParams.ColonyParams.AttractivenessFactor;
                            cParams.ExploProportionConst = solvingParams.ColonyParams.ExploProportionConst;
                        });
                        serviceCollection.Configure<TerminationParams>(tParams =>
                        {
                            tParams.TerminationRule = solvingParams.TerminationParams.TerminationRule;
                            tParams.IterationCount = solvingParams.TerminationParams.IterationCount;
                            tParams.CeilingPercentage = solvingParams.TerminationParams.CeilingPercentage;
                            tParams.InRowTerminationCount = solvingParams.TerminationParams.InRowTerminationCount;
                        });
                        serviceCollection.Configure<ReinitializationParams>(rParams =>
                        {
                            rParams.ReinitializationRule = solvingParams.ReinitializationParams.ReinitializationRule;
                            rParams.IterIncrement = solvingParams.ReinitializationParams.IterIncrement;
                            rParams.StagnationCeiling = solvingParams.ReinitializationParams.StagnationCeiling;
                        });

                        serviceCollection.AddSingleton<IRandom, RandomGen>();
                        serviceCollection.AddTransient<ITerminationChecker, TerminationChecker>();
                        serviceCollection.AddTransient<IReinitializer, Reinitializer>();

                        serviceCollection.AddTransient<IPheromoneGraphVisualiser, NullPheromoneVisualiser>();

                        serviceCollection.AddTransient<IAntFactory<IAnt>, AntFactory<AcsAnt>>();
                        serviceCollection.AddTransient<IColony, AcsColony>();
                        serviceCollection.AddTransient<SolverBase, AcsSolver>();

                        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
                        SolverBase solver = serviceProvider.GetService<SolverBase>();

                        // AcsSolver solver = new(Options.Create(sParams));
                        ITour sol = solver.Solve((IProblem)x);
                        lock (threadLock)
                        {
                            results.Add(new(problem.name, solver.CurrIterationCount, sol.Length, CalcErr(problem.optLen, sol.Length)));
                        }
                        if (Interlocked.Decrement(ref toProcess) == 0)
                        {
                            resetEvent.Set();
                        }
                    }), prob);
            }
            resetEvent.WaitOne();
        }

        foreach (Result res in results)
        {
            Console.WriteLine($"{res.name} | iter: {res.iterCount} | sol. len.: {res.solLen} | err.: {Double.Round(res.err, 2)}%");
        }
        Console.WriteLine($"Avg. err.: {Double.Round(results.Average((Result res) => res.err), 2)}%");

    }

    static double CalcErr(double optLen, double foundLen)
    {
        return (foundLen / (optLen / 100)) - 100;
    }
}