namespace TspAcoSolver
{
    public class Colony
    {
        Ant[] _ants;

        int _threadCount;
        public int AntCount { get => _ants.Length; }
        public Colony(int antCount, int threadCount, double trailLevelFactor, double attractivenessFactor)
        {
            _ants = new Ant[antCount];
            for (int i = 0; i < antCount; i++)
            {
                _ants[i] = new(trailLevelFactor, attractivenessFactor);
            }

            _threadCount = threadCount;
        }

        List<Tour> GenerateSolutionsInRange(PheromoneGraph graph, int from, int to)
        {
            List<Tour> solutions = new();
            for (int i = from; i < to; i++)
            {
                // Console.WriteLine($"Ant {i} solving in Thread {Thread.CurrentThread.ManagedThreadId}");

                Ant ant = _ants[i];
                ant.FindTour(graph);
                if (ant.LastTour.IsValid())
                {
                    solutions.Add(ant.LastTour);
                }
            }

            return solutions;
        }
        public List<Tour> GenerateSolutions(PheromoneGraph graph)
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
}