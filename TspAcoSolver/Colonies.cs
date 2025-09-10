using Microsoft.Extensions.Options;

namespace TspAcoSolver
{

    public interface IColony
    {
        public List<ITour> GenerateSolutions(PheromoneGraph graph);
    }
    public abstract class ColonyBase : IColony
    {
        protected IAnt[] _ants;

        protected int _threadCount;
        public int AntCount { get => _ants.Length; }
        public ColonyBase(IAntFactory<IAnt> antFactory, IOptions<ColonyParams> colonyParams)
        {
            _ants = antFactory.CreateAnts((int)colonyParams.Value.AntCount);
            _threadCount = (int)colonyParams.Value.ThreadCount;
        }

        protected Tour Generate1Solution(PheromoneGraph graph, int antIndex)
        {
            IAnt ant = _ants[antIndex];
            ant.FindTour(graph);
            return ant.LastTour;
        }

        public abstract List<ITour> GenerateSolutions(PheromoneGraph graph);
    }

    public class AsColony : ColonyBase
    {
        public AsColony(IAntFactory<IAnt> antFactory, IOptions<ColonyParams> colonyParamsOpt) : base(antFactory, colonyParamsOpt){}
        List<Tour> GenerateSolutionsInRange(PheromoneGraph graph, int from, int to)
        {
            List<Tour> solutions = new();
            for (int i = from; i < to; i++)
            {
                // Console.WriteLine($"Ant {i} solving in Thread {Thread.CurrentThread.ManagedThreadId}");
                Tour solution = Generate1Solution(graph, i);
                if (solution.IsValid())
                {
                    solutions.Add(solution);
                }
            }

            return solutions;
        }
        public override List<ITour> GenerateSolutions(PheromoneGraph graph)
        {
            List<ITour> solutions = new();
            Thread[] threads = new Thread[_threadCount];
            int antCountForThread = AntCount / _threadCount;

            List<Tour>[] results = new List<Tour>[_threadCount];
            Lock threadLock = new();
            for (int i = 0; i < _threadCount; i++)
            {
                int from = antCountForThread * i;
                int to;
                if (i == _threadCount - 1)
                {
                    to = AntCount;
                }
                else
                {
                    to = antCountForThread * (i + 1);
                }

                int j = i;
                threads[i] = new Thread(() =>
                {
                    results[j] = GenerateSolutionsInRange(graph, from, to);
                });
                threads[i].Start();
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
            for (int i = 0; i < _threadCount; i++)
            {
                solutions.AddRange(results[i]);
            }
            return solutions;
        }
    }

    public class AcsColony : ColonyBase
    {
        public AcsColony(IAntFactory<IAnt> antFactory, IOptions<ColonyParams> colonyParamsOpt) : base(antFactory, colonyParamsOpt){}

        public override List<ITour> GenerateSolutions(PheromoneGraph graph)
        {
            List<ITour> solutions = new();
            for (int i = 0; i < AntCount; i++)
            {
                Tour solution = Generate1Solution(graph, i);
                // Console.WriteLine($"Local update");

                graph.UpdateLocallyPheromones(solution);
                if (solution.IsValid())
                {
                    solutions.Add(solution);
                }
            }

            return solutions;
        }
    }
}