namespace TspAcoSolver
{
    public abstract class ColonyBase
    {
        protected AntBase[] _ants;

        protected int _threadCount;
        public int AntCount { get => _ants.Length; }
        public ColonyBase(ColonyParams colonyParams)
        {
            _threadCount = colonyParams.ThreadCount;
        }

        protected Tour Generate1Solution(PheromoneGraph graph, int antIndex)
        {
            AntBase ant = _ants[antIndex];
            ant.FindTour(graph);
            return ant.LastTour;
        }

        public abstract List<Tour> GenerateSolutions(PheromoneGraph graph);
    }

    public class AsColony : ColonyBase
    {
        public AsColony(ColonyParams colonyParams) : base(colonyParams)
        {
            _ants = new AsAnt[colonyParams.AntCount];
            for (int i = 0; i < colonyParams.AntCount; i++)
            {
                _ants[i] = new AsAnt(colonyParams);
            }
        }
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
        public override List<Tour> GenerateSolutions(PheromoneGraph graph)
        {
            List<Tour> solutions = new();
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
        public AcsColony(ColonyParams colonyParams) : base(colonyParams)
        {
            _ants = new AcsAnt[colonyParams.AntCount];
            for (int i = 0; i < colonyParams.AntCount; i++)
            {
                _ants[i] = new AcsAnt(colonyParams);
            }
        }

        public override List<Tour> GenerateSolutions(PheromoneGraph graph)
        {
            List<Tour> solutions = new();
            for (int i = 0; i < AntCount; i++)
            {
                Tour solution = Generate1Solution(graph, i);
                Console.WriteLine($"Local update");

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