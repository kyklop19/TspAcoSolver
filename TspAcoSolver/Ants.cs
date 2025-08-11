namespace TspAcoSolver
{
    public class Ant
    {
        public int CurrVertex { get; set; }
        public Tour LastTour { get; set; }

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

        int ChooseNbr(PheromoneGraph graph, List<int> unvisited_nbrs)
        {
            double sum = 0;

            double[] probabilities = new double[unvisited_nbrs.Count];
            for (int i = 0; i < unvisited_nbrs.Count; i++)
            {
                double attractiveness = 1 / graph.Weight(CurrVertex, unvisited_nbrs[i]);
                double trailLevel = graph.Pheromones[CurrVertex, unvisited_nbrs[i]];
                double probability = trailLevel * attractiveness;
                probabilities[i] = probability;
                sum += probability;
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
        public int AntCount { get => _ants.Length; }
        public Colony(int antCount)
        {
            _ants = new Ant[antCount];
            for (int i = 0; i < antCount; i++)
            {
                _ants[i] = new();
            }
        }

        public List<Tour> GenerateSolutions(PheromoneGraph graph)
        {
            List<Tour> solutions = new();
            for (int i = 0; i < AntCount; i++)
            {
                Ant ant = _ants[i];
                ant.FindTour(graph);
                if (ant.LastTour.IsValid())
                {
                    solutions.Add(ant.LastTour);
                }
            }
            return solutions;
        }
    }
}