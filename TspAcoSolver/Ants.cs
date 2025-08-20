namespace TspAcoSolver
{
    public class Ant
    {
        public double TrailLevelFactor { get; init; }
        public double AttractivenessFactor { get; init; }
        public int CurrVertex { get; set; }
        public Tour LastTour { get; set; }

        public Ant(double trailLevelFactor, double attractivenessFactor)
        {
            TrailLevelFactor = trailLevelFactor;
            AttractivenessFactor = attractivenessFactor;
        }

        public void FindTour(PheromoneGraph graph)
        {
            LastTour = new(graph);

            Random rnd = new Random();
            int firstVertex = rnd.Next(0, graph.VertexCount);
            CurrVertex = firstVertex;
            LastTour.Add(CurrVertex);

            while (!LastTour.HasAllVerteces() && !LastTour.HasDeadEnd())
            {
                CurrVertex = ChooseNbr(graph, LastTour.NextPossibleVertices());
                LastTour.Add(CurrVertex);
            }
        }

        double ScoreEdge(PheromoneGraph graph, int nbr)
        {
            double attractiveness = 1 / graph.Weight(CurrVertex, nbr);
            double trailLevel = graph.Pheromones[CurrVertex, nbr];
            double score = Math.Pow(trailLevel, TrailLevelFactor) * Math.Pow(attractiveness, AttractivenessFactor);
            return score;
        }

        int ChooseNbr(PheromoneGraph graph, List<int> unvisited_nbrs)
        {
            double sum = 0;

            double[] probabilities = new double[unvisited_nbrs.Count];
            for (int i = 0; i < unvisited_nbrs.Count; i++)
            {
                double score = ScoreEdge(graph, unvisited_nbrs[i]);
                probabilities[i] = score;
                sum += score;
            }
            for (int i = 0; i < unvisited_nbrs.Count; i++)
            {
                probabilities[i] /= sum;
                if (i != 0)
                {
                    probabilities[i] += probabilities[i - 1];
                }
            }

            Random rnd = new();
            double rndNum = rnd.NextDouble();
            int index = 0;
            while (index != probabilities.Length - 1 && rndNum > probabilities[index]) index++;

            // System.Console.WriteLine($"Rnd: {rndNum}");
            // foreach (double prob in probabilities)
            // {
            //     System.Console.Write($"{prob} ");
            // }
            // System.Console.WriteLine($"Index: {index}");


            return unvisited_nbrs[index];
        }
    }

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
                Console.WriteLine($"Ant {i} solving in Thread {Thread.CurrentThread.ManagedThreadId}");

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