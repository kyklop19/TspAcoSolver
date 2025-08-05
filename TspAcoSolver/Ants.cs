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
            HashSet<int> unvisited = new();

            for (int i = 0; i < graph.VertexCount; i++)
            {
                unvisited.Add(i);
            }
            unvisited.Remove(CurrVertex);

            bool FoundDeadEnd = false;
            //TODO: add last edge
            while (unvisited.Count != 0 && !FoundDeadEnd)
            {
                CurrVertex = ChooseNbr(graph, unvisited);
                if (CurrVertex == -1)
                {
                    FoundDeadEnd = true;
                }
                else
                {
                    unvisited.Remove(CurrVertex);
                    LastTour.Add(CurrVertex);
                }
            }

            if (!FoundDeadEnd && graph.Nbrs(LastTour.Vertices[LastTour.Vertices.Count - 1]).Contains(firstVertex))
            {
                LastTour.Add(firstVertex);
            }
        }

        int ChooseNbr(PheromoneGraph graph, HashSet<int> unvisited)
        {
            double sum = 0;
            List<int> nbrs = graph.Nbrs(CurrVertex);
            List<int> unvisited_nbrs = new List<int>();
            foreach (int nbr in nbrs)
            {
                if (unvisited.Contains(nbr))
                {
                    unvisited_nbrs.Add(nbr);
                }
            }
            if (unvisited_nbrs.Count == 0)
            {
                return -1;
            }
            nbrs = unvisited_nbrs;
            double[] probabilities = new double[nbrs.Count];
            for (int i = 0; i < nbrs.Count; i++)
            {
                double attractiveness = 1 / graph.Weight(CurrVertex, nbrs[i]);
                double trailLevel = graph.Pheromones[CurrVertex, nbrs[i]];
                double probability = trailLevel * attractiveness;
                probabilities[i] = probability;
                sum += probability;
            }
            for (int i = 0; i < nbrs.Count; i++)
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
            while (index != probabilities.Length - 1 && rndNum < probabilities[index]) index++;

            return nbrs[index];
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

        public Tour[] GenerateSolutions(PheromoneGraph graph)
        {
            Tour[] solutions = new Tour[AntCount];
            for (int i = 0; i < AntCount; i++)
            {
                Ant ant = _ants[i];
                ant.FindTour(graph);
                solutions[i] = ant.LastTour;
            }
            return solutions;
        }
    }
}