using System;
using TspAcoSolver;

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
        SolvingParams sParams = new Config().Read(@"default_config.yaml");

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
                        AcsSolver solver = new((IProblem)x, sParams);
                        ITour sol = solver.Solve();
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